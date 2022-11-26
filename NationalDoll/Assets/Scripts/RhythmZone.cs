using System;
using System.Collections;
using System.Collections.Generic;
using FarmingEngine;
using UnityEngine;

public class RhythmZone : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerCharacter>(out _))
        {
            if (RhythmController.I.hasBeenUsed)
            {
                sprite.color = Color.yellow;
            }
            else
            {
                RhythmController.I.uiObject.ToggleRhythm(true);
                sprite.color = Color.cyan;
                RhythmController.I.canBeActivated = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerCharacter>(out _))
        {
            sprite.color = Color.white;
            RhythmController.I.canBeActivated = false;
            RhythmController.I.uiObject.ToggleRhythm(false);
        }
    }
}
