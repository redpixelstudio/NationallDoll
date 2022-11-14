using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Automatically water plants in range
    /// </summary>

    public class Sprinkler : MonoBehaviour
    {
        public float range = 1f;

        private static List<Sprinkler> sprinkler_list = new List<Sprinkler>();

        private void Awake()
        {
            sprinkler_list.Add(this);
        }

        private void OnDestroy()
        {
            sprinkler_list.Remove(this);
        }

        public static Sprinkler GetNearestInRange(Vector3 pos)
        {
            float min_dist = 999f;
            Sprinkler nearest = null;
            foreach (Sprinkler sprinkler in sprinkler_list)
            {
                float dist = (sprinkler.transform.position - pos).magnitude;
                if (dist < min_dist && dist < sprinkler.range)
                {
                    nearest = sprinkler;
                    min_dist = dist;
                }
            }
            return nearest;
        }

        public static List<Sprinkler> GetAll()
        {
            return sprinkler_list;
        }
    }
}
