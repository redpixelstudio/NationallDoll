using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Allow to set the EquipSlot for the EquipPanel
    /// </summary>

    public class EquipSlotUI : ItemSlot
    {
        [Header("Equip Slot")]
        public EquipSlot equip_slot;


        protected override void Start()
        {
            base.Start();
            index = (int) equip_slot;
        }
    }

}
