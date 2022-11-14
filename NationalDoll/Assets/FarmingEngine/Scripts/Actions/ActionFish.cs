using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Use your fishing rod to fish a fish!
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Fish", order = 50)]
    public class ActionFish : SAction
    {
        public GroupData fishing_rod;
        public float fish_time = 3f;

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            if (select != null)
            {
                character.FaceTorward(select.transform.position);

                ItemProvider pond = select.GetComponent<ItemProvider>();
                if (pond != null)
                {
                    if (pond.HasItem())
                    {
                        character.FishItem(pond, 1, fish_time);
                        character.Attributes.GainXP("fishing", 10); //Example of XP gain
                    }
                }
            }
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            ItemProvider pond = select.GetComponent<ItemProvider>();
            return pond != null && pond.HasItem() && character.EquipData.HasItemInGroup(fishing_rod) && !character.IsSwimming();
        }
    }

}