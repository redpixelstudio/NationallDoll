using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Dig using the shovel, put this on a Selectable to auto dig on left click
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/DigAuto", order = 50)]
    public class ActionDigAuto : AAction
    {
        public GroupData required_item;
        public float energy = 1f;

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            DigSpot spot = select.GetComponent<DigSpot>();
            if (spot != null)
            {
                string animation = character.Animation ? character.Animation.dig_anim : "";
                character.TriggerAnim(animation, spot.transform.position);
                character.TriggerProgressBusy(1.5f, () =>
                {
                    spot.Dig();

                    character.Attributes.AddAttribute(AttributeType.Energy, -energy);

                    InventoryItemData ivdata = character.EquipData.GetFirstItemInGroup(required_item);
                    if (ivdata != null)
                        ivdata.durability -= 1;
                });
            }
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            return character.EquipData.HasItemInGroup(required_item) && character.Attributes.GetAttributeValue(AttributeType.Energy) >= energy;
        }
    }

}