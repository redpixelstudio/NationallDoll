using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{

    /// <summary>
    /// Use to Water plant with the watering can
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/WaterPlant", order = 50)]
    public class ActionWaterPlant : AAction
    {
        public GroupData required_item;
        public float energy = 1f;

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            InventoryItemData item = character.EquipData.GetFirstItemInGroup(required_item);
            ItemData idata = ItemData.Get(item?.item_id);
            Plant plant = select.GetComponent<Plant>();
            Soil soil = select.GetComponent<Soil>();
            if (idata != null && (plant != null || soil != null))
            {
                //Remove water
                if (idata.durability_type == DurabilityType.UsageCount)
                    item.durability -= 1f;
                else
                    character.Inventory.RemoveEquipItem(idata.equip_slot);

                string animation = character.Animation ? character.Animation.water_anim : "";
                character.TriggerAnim(animation, select.transform.position, 1f);
                character.TriggerProgressBusy(1f, () =>
                {
                    //Add to soil
                    if(plant)
                        plant.Water();
                    if (soil)
                        soil.Water();

                    character.Attributes.AddAttribute(AttributeType.Energy, -energy);
                });
            }
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            Plant plant = select.GetComponent<Plant>();
            Soil soil = select.GetComponent<Soil>();
            bool has_energy = character.Attributes.GetAttributeValue(AttributeType.Energy) >= energy;
            return (plant != null || soil != null) && has_energy && character.EquipData.HasItemInGroup(required_item);
        }
    }

}
