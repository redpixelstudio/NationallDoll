using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Collect an animal product, using a container (Like milk for cow)
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/CollectProductFill", order = 50)]
    public class ActionCollectProductFill : MAction
    {
        //Merge action
        public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            AnimalLivestock animal = select.GetComponent<AnimalLivestock>();
            if (select.HasGroup(merge_target) && animal != null)
            {
                character.TriggerAnim("Take", animal.transform.position);
                character.TriggerBusy(0.5f, () =>
                {
                    InventoryData inventory = slot.GetInventory();
                    inventory.RemoveItemAt(slot.index, 1);
                    animal.CollectProduct(character);
                });
            }
        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            AnimalLivestock animal = select.GetComponent<AnimalLivestock>();
            return select.HasGroup(merge_target) && animal != null && animal.HasProduct();
        }
    }

}