using System;
using System.Collections;
using System.Collections.Generic;
using FarmingEngine;
using UnityEngine;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

public class RhythmController : MonoBehaviour
{
    public static RhythmController I;

    [HideInInspector] public bool hasBeenUsed;
    
    [HideInInspector] public bool canBeActivated;
    
    [SerializeField] private GameObject rhythmObject;
    [SerializeField] private PlayerControls playerControlsObject;
    [SerializeField] private TheCamera cameraObject;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private SpriteRenderer blackSprite;
    [SerializeField] private Image blackImage;

    private TheUI uiObject;

    private Vector3 cameraLastPosition;
    private Vector3 playerLastPosition;

    private bool isOpened;

    private RhythmController()
    {
        I = this;
    }

    public async void ChangeRhythmGameState(bool isToOpen)
    {
        if (hasBeenUsed) return;
        if (!canBeActivated) return;
        isOpened = isToOpen;
        uiObject ??= FindObjectOfType<TheUI>();
        if (isToOpen)
        {
            playerControlsObject.gameObject.SetActive(!isOpened);
            playerControlsObject.Stop();
            uiObject.gameObject.SetActive(!isOpened);
            await PlayAnimation(isOpened);
            rhythmObject.SetActive(isOpened);
        }
        else
        {
            rhythmObject.SetActive(isOpened);
            await PlayAnimation(isOpened);
            uiObject.gameObject.SetActive(!isOpened);
            playerControlsObject.gameObject.SetActive(!isOpened);
            playerControlsObject.Stop();
        }
    }

    private async Task PlayAnimation(bool isToOpen)
    {
        canBeActivated = false;
        float lerp = 0;
        if (isToOpen)
        {
            cameraObject.ToggleCameraMove();
            playerLastPosition = playerObject.gameObject.transform.position;
            cameraLastPosition = cameraObject.gameObject.transform.position;
            blackImage.gameObject.SetActive(true);
            while (lerp < 0.75)
            {
                cameraObject.gameObject.transform.position = Vector3.Lerp(cameraLastPosition, playerLastPosition, lerp);
                blackImage.color = Color.Lerp(Color.clear, Color.black, lerp);
                lerp += Time.deltaTime;
                await Task.Yield();
            }
            lerp = 0;
            playerLastPosition = cameraObject.gameObject.transform.position;
            while (lerp < 1)
            {
                var color = blackImage.color;
                blackImage.color = Color.Lerp(color, Color.black, lerp);
                lerp += Time.deltaTime;
                await Task.Yield();
                
            }
            cameraObject.gameObject.transform.position = cameraLastPosition * 2;
            blackSprite.gameObject.SetActive(true);
            blackImage.gameObject.SetActive(false);
        }
        else
        {
            blackImage.gameObject.SetActive(true);
            blackSprite.gameObject.SetActive(false);
            while (lerp < 1)
            {
                cameraObject.gameObject.transform.position = Vector3.Lerp(playerLastPosition, cameraLastPosition, lerp);
                blackImage.color = Color.Lerp(Color.black, Color.clear, lerp);
                lerp += Time.deltaTime;
                await Task.Yield();
            }
            cameraObject.ToggleCameraMove();
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
    }

    private void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            ChangeRhythmGameState(!isOpened);
        }
    }
}
