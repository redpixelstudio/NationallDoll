using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{

    public class AnimalFood : MonoBehaviour
    {
        public GroupData food_group;

        private Item item;
        private ItemStack stack;
        private Plant plant;

        private static List<AnimalFood> food_list = new List<AnimalFood>();

        void Awake()
        {
            food_list.Add(this);

            //These can be null, usually only one will be there
            item = GetComponent<Item>();
            stack = GetComponent<ItemStack>();
            plant = GetComponent<Plant>();
        }

        private void OnDestroy()
        {
            food_list.Remove(this);
        }

        public void EatFood()
        {
            if (item != null)
                item.EatItem();
            if (stack != null)
                stack.RemoveItem(1);
            if (plant != null)
                plant.KillNoLoot();
        }

        public bool CanBeEaten()
        {
            if (stack != null)
                return stack.GetItemCount() > 0;
            return true;
        }

        public static AnimalFood GetNearest(GroupData group, Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            AnimalFood nearest = null;
            foreach (AnimalFood item in food_list)
            {
                if (item.food_group == group || group == null || item.food_group == null)
                {
                    float dist = (item.transform.position - pos).magnitude;
                    if (dist < min_dist && item.CanBeEaten())
                    {
                        min_dist = dist;
                        nearest = item;
                    }
                }
            }
            return nearest;
        }

        public static AnimalFood GetNearest(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            AnimalFood nearest = null;
            foreach (AnimalFood item in food_list)
            {
                float dist = (item.transform.position - pos).magnitude;
                if (dist < min_dist && item.CanBeEaten())
                {
                    min_dist = dist;
                    nearest = item;
                }
            }
            return nearest;
        }

        public static List<AnimalFood> GetAll()
        {
            return food_list;
        }
    }

}
