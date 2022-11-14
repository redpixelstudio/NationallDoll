﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    [CreateAssetMenu(fileName ="LootData", menuName ="FarmingEngine/LootData", order =20)]
    public class LootData : SData
    {
        public ItemData item;
        public int quantity = 1;
        public float probability = 1f; //1f = 100%, 0.5f = 50%, 0f = 0%
    }

}