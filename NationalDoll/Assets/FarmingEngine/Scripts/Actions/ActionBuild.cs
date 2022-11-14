using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Build an item into a construction (trap, lure)
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Build", order = 50)]
    public class ActionBuild : SAction
    {
        public override void DoAction(PlayerCharacter character, ItemSlot slot)
        {
            ItemData item = slot.GetItem();
            InventoryData inventory = slot.GetInventory();
            if (item != null && item.construction_data != null)
            {
                character.Crafting.CraftConstructionBuildMode(item.construction_data, false, (Buildable build) =>
                {
                    InventoryItemData invdata = inventory.GetItem(slot.index);
                    inventory.RemoveItemAt(slot.index, 1);

                    BuiltConstructionData constru = PlayerData.Get().GetConstructed(build.GetUID());
                    if (invdata != null && constru != null && item.HasDurability())
                        constru.durability = invdata.durability; //Save durability
                });

                TheAudio.Get().PlaySFX("craft", item.craft_sound);
            }

            if (item != null && item.character_data != null)
            {
                character.Crafting.CraftCharacterBuildMode(item.character_data, false, (Buildable build) =>
                {
                    InventoryItemData invdata = inventory.GetItem(slot.index);
                    inventory.RemoveItemAt(slot.index, 1);
                });

                TheAudio.Get().PlaySFX("craft", item.craft_sound);
            }
        }
    }

}
