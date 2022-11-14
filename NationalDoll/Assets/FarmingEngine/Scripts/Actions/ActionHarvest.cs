using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Harvest the fruit of a plant
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Harvest", order = 50)]
    public class ActionHarvest : AAction
    {
        public float energy = 1f;

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            Plant plant = select.GetComponent<Plant>();
            if (plant != null)
            {
                string animation = character.Animation ? character.Animation.take_anim : "";
                character.TriggerAnim(animation, plant.transform.position);
                character.TriggerBusy(0.5f, () =>
                {
                    character.Attributes.AddAttribute(AttributeType.Energy, -energy);
                    plant.Harvest(character);
                });
            }
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            Plant plant = select.GetComponent<Plant>();
            if (plant != null)
            {
                return plant.HasFruit() && character.Attributes.GetAttributeValue(AttributeType.Energy) >= energy;
            }
            return false;
        }
    }

}