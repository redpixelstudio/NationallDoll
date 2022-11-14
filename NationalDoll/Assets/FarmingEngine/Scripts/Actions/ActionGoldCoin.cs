using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Learn a crafting recipe
    /// </summary>
    

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/GoldCoin", order = 50)]
    public class ActionGoldCoin : AAction
    {
        public override void DoAction(PlayerCharacter character, ItemSlot slot)
        {
            InventoryData inventory = slot.GetInventory();
            int amount = slot.GetQuantity();
            inventory.RemoveItemAt(slot.index, amount);
            character.SaveData.gold += amount;
            ItemTakeFX.DoCoinTakeFX(character.transform.position, slot.GetItem(), character.player_id);
        }

        public override void DoSelectAction(PlayerCharacter character, ItemSlot slot)
        {
            InventoryData inventory = slot.GetInventory();
            int amount = slot.GetQuantity();
            inventory.RemoveItemAt(slot.index, amount);
            character.SaveData.gold += amount;
            ItemTakeFX.DoCoinTakeFX(character.transform.position, slot.GetItem(), character.player_id);
        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
        {
            return true;
        }
    }

}