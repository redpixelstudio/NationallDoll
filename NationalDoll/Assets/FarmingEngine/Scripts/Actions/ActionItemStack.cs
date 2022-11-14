using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Add item to stack container
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/ItemStack", order = 50)]
    public class ActionItemStack : MAction
    {
        //Merge action
        public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            InventoryData inventory = slot.GetInventory();
            InventoryItemData iidata = inventory.GetItem(slot.index);
            inventory.RemoveItemAt(slot.index, iidata.quantity);

            ItemStack stack = select.GetComponent<ItemStack>();
            stack.AddItem(iidata.quantity);
        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            ItemStack stack = select.GetComponent<ItemStack>();
            return stack != null && stack.item != null && stack.item.id == slot.GetItem().id && stack.GetItemCount() < stack.item_max;
        }
    }

}
