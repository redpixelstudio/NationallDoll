using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Read a note on an item
    /// </summary>
    

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Read", order = 50)]
    public class ActionRead : SAction
    {

        public override void DoAction(PlayerCharacter character, ItemSlot slot)
        {
            ItemData item = slot.GetItem();
            if (item != null)
            {
                ReadPanel.Get().ShowPanel(item.title, item.desc);
            }

        }

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            ReadObject read = select.GetComponent<ReadObject>();
            if (read != null)
            {
                ReadPanel.Get().ShowPanel(read.title, read.text);
            }

        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
        {
            return true;
        }
    }

}