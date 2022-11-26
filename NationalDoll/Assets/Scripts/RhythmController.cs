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
    [SerializeField] private PlayerCharacter playerObject;

    [HideInInspector] public TheUI uiObject;

    private Vector3 cameraLastPosition;
    private Quaternion cameraLastRotation;
    private Vector3 playerLastPosition;
    private Quaternion playerLastRotation;

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
        uiObject.ToggleRhythm(false);
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
            playerLastPosition = playerObject.gameObject.transform.position + new Vector3(6,-1,-6);
            playerLastRotation = playerObject.gameObject.transform.rotation;
            var cameraGameObject = cameraObject.gameObject;
            cameraLastPosition = cameraGameObject.transform.position;
            cameraLastRotation = cameraGameObject.transform.rotation;
            playerObject.rotate_speed = 0;
            while (lerp < 0.75)
            {
                cameraObject.gameObject.transform.position = Vector3.Lerp(cameraLastPosition, playerLastPosition, lerp);
                cameraObject.gameObject.transform.rotation = Quaternion.Lerp(cameraLastRotation, 
                    Quaternion.Euler(new Vector3(cameraLastRotation.x - 40,cameraLastRotation.y,cameraLastRotation.z)),lerp);
                playerObject.transform.rotation = Quaternion.Lerp(playerLastRotation,
                    Quaternion.Euler(new Vector3(playerLastRotation.x,Mathf.Abs(cameraObject.gameObject.transform.rotation.y) - 180, playerLastRotation.z)), lerp);
                lerp += Time.deltaTime;
                await Task.Yield();
            }
            var newPositionedCameraObject = cameraObject.gameObject;
            playerLastPosition = newPositionedCameraObject.transform.position;
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
}
