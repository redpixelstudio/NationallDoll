using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Collect an animal product when clicking on it
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/CollectProduct", order = 50)]
    public class ActionCollectProduct : AAction
    {
        //Merge action
        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            AnimalLivestock animal = select.GetComponent<AnimalLivestock>();
            if (animal != null)
            {
                character.TriggerAnim("Take", animal.transform.position);
                character.TriggerBusy(0.5f, () =>
                {
                    animal.CollectProduct(character);
                });
            }
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            AnimalLivestock animal = select.GetComponent<AnimalLivestock>();
            return animal != null && animal.HasProduct();
        }
    }

}