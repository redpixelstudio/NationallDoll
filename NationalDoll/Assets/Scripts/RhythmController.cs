using System;
using System.Collections;
using System.Collections.Generic;
using FarmingEngine;
using RhythmGameStarter;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

public class RhythmController : MonoBehaviour
{
    public static RhythmController I;

    [HideInInspector] public bool hasBeenUsed;

    [HideInInspector] public bool isSongPaused;
    
    [HideInInspector] public bool canBeActivated;
    
    [SerializeField] public SongManager songManager;
    [SerializeField] private PlayerControls playerControlsObject;
    [SerializeField] private TheCamera cameraObject;
    [SerializeField] public PlayerCharacter playerObject;
    [SerializeField] private SongChooser songChooser;

    [HideInInspector] public TheUI uiObject;

    private Vector3 cameraLastPosition;
    private Quaternion cameraLastRotation;
    private Vector3 playerLastPosition;
    private Quaternion playerLastRotation;

    private bool isOpened;
    private bool isDancing;

    private RhythmController()
    {
        I = this;
    }

    public async void InitSong()
    {
        songChooser ??= FindObjectOfType<SongChooser>();
        songChooser.choosingCanvas.gameObject.SetActive(true);
        uiObject.ToggleRhythm(false);
        playerControlsObject.gameObject.SetActive(false);
        uiObject.gameObject.SetActive(false);
        await songChooser.WaitForSong();
        songManager ??= FindObjectOfType<SongManager>();
        songManager.defaultSong = songChooser.chosenSong;
        ChangeRhythmGameState(true);
    }

    private async void ChangeRhythmGameState(bool isToOpen)
    {
        if (hasBeenUsed) return;
        if (!canBeActivated) return;
        isOpened = isToOpen;
        if (isToOpen)
        {
            playerControlsObject.gameObject.SetActive(!isOpened);
            playerControlsObject.Stop();
            uiObject.gameObject.SetActive(!isOpened);
            await PlayAnimation(isOpened);
            songManager ??= FindObjectOfType<SongManager>();
            songManager.gameObject.SetActive(isOpened);
            Dance();
        }
        else
        {
            songManager ??= FindObjectOfType<SongManager>();
            songManager.gameObject.SetActive(isOpened);
            await PlayAnimation(isOpened);
            uiObject.gameObject.SetActive(!isOpened);
            playerControlsObject.gameObject.SetActive(!isOpened);
            playerControlsObject.Stop();
        }
    }

    private async void Dance()
    {
        while (isDancing)
        {
            playerObject.character_anim.animator.SetTrigger("Dance");
            //Debug.Log("Change Dance set!");
            await Task.Delay(TimeSpan.FromSeconds(0.75));
        }
    }

    private async Task PlayAnimation(bool isToOpen)
    {
        canBeActivated = false;
        float lerp = 0;
        if (isToOpen)
        {
            cameraObject ??= FindObjectOfType<TheCamera>();
            playerObject ??= FindObjectOfType<PlayerCharacter>();
            cameraObject.ToggleCameraMove();
            playerLastPosition = playerObject.gameObject.transform.position + new Vector3(4,2,-6);
            playerLastRotation = playerObject.gameObject.transform.rotation;
            var cameraGameObject = cameraObject.gameObject;
            cameraLastPosition = cameraGameObject.transform.position;
            cameraLastRotation = cameraGameObject.transform.rotation;
            playerObject.rotate_speed = 0;
            while (lerp < 1)
            {
                cameraObject.gameObject.transform.position = Vector3.Lerp(cameraLastPosition, playerLastPosition, lerp);
                cameraObject.gameObject.transform.rotation = Quaternion.Lerp(cameraLastRotation, 
                    Quaternion.Euler(new Vector3(cameraLastRotation.x - 10,cameraLastRotation.y,cameraLastRotation.z)),lerp);
                playerObject.transform.rotation = Quaternion.Lerp(playerLastRotation,
                    Quaternion.Euler(new Vector3(playerLastRotation.x,
                        Mathf.Abs(cameraObject.gameObject.transform.rotation.y) + 180, playerLastRotation.z)), lerp);
                lerp += Time.deltaTime;
                await Task.Yield();
            }
            var newPositionedCameraObject = cameraObject.gameObject;
            playerLastPosition = newPositionedCameraObject.transform.position;
            isDancing = true;
            playerObject.character_anim.animator.SetBool("IsDancing", isDancing);
        }
        else
        {
            while (lerp < 1)
            {
                cameraObject.gameObject.transform.position = Vector3.Lerp(playerLastPosition, cameraLastPosition, lerp);
                cameraObject.gameObject.transform.rotation = Quaternion.Lerp(cameraObject.gameObject.transform.rotation,cameraLastRotation, lerp);
                lerp += Time.deltaTime;
                await Task.Yield();
            }
            playerObject.rotate_speed = 400;
            cameraObject.ToggleCameraMove();
            isDancing = false;
            playerObject.character_anim.animator.SetBool("IsDancing", isDancing);
        }
        canBeActivated = true;
    }
    
    private void ChangeGold(int gold)
    {
        var player = PlayerCharacter.GetFirst();
        player.Attributes.CharacterData.gold += gold;
    }

    private void ChangeEnergy(float energy)
    {
        var player = PlayerCharacter.GetFirst();
        player.Data.AddAttributeValue(AttributeType.Energy, -energy, player.Attributes.GetAttributeMax(AttributeType.Energy));
    }
    
    public void StopPlaying()
    {
        ChangeGold(50);
        ChangeEnergy(10);
        ChangeRhythmGameState(false);
        hasBeenUsed = true;
        songChooser.pauseButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        isDancing = false;
    }

    private void OnDestroy()
    {
        isDancing = false;
    }
}
