using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Cook an item on the fire (like raw meat)
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Cook", order = 50)]
    public class ActionCook : MAction
    {
        public ItemData cooked_item;
        public float duration = 0.5f;
        public float energy = 1f;

        //Merge action
        public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            string anim = character.Animation ? character.Animation.use_anim : "";
            character.TriggerAnim(anim, select.transform.position);
            character.TriggerProgressBusy(duration, () =>
            {
                InventoryData inventory = slot.GetInventory();
                inventory.RemoveItemAt(slot.index, 1);
                character.Inventory.GainItem(cooked_item, 1);
                character.Attributes.AddAttribute(AttributeType.Energy, -energy);
            });
        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            return character.Attributes.GetAttributeValue(AttributeType.Energy) >= energy;
        }
    }

}
