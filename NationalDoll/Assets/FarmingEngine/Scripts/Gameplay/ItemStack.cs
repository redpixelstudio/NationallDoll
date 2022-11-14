using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    //Can stack many of only 1 type of item, (Not for inventory containers like Chest)

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(UniqueID))]
    public class ItemStack : MonoBehaviour
    {
        public ItemData item;
        public int item_start = 0;
        public int item_max = 20;

        public GameObject item_mesh;

        private Selectable selectable;
        private UniqueID unique_id;

        private static List<ItemStack> stack_list = new List<ItemStack>();

        void Awake()
        {
            stack_list.Add(this);
            selectable = GetComponent<Selectable>();
            unique_id = GetComponent<UniqueID>();

        }

        private void OnDestroy()
        {
            stack_list.Remove(this);
        }

        private void Start()
        {
            if(!PlayerData.Get().HasCustomInt(GetCountUID()))
                 PlayerData.Get().SetCustomInt(GetCountUID(), item_start);
        }

        void Update()
        {
            if (item_mesh != null)
            {
                bool active = GetItemCount() > 0;
                if (active != item_mesh.activeSelf)
                    item_mesh.SetActive(active);
            }
        }

        public void AddItem(int value)
        {
            int val = GetItemCount();
            PlayerData.Get().SetCustomInt(GetCountUID(), val + value);
        }

        public void RemoveItem(int value)
        {
            int val = GetItemCount();
            val -= value;
            val = Mathf.Max(val, 0);
            PlayerData.Get().SetCustomInt(GetCountUID(), val);
        }

        public int GetItemCount()
        {
            return PlayerData.Get().GetCustomInt(GetCountUID());
        }

        public string GetUID()
        {
            return unique_id.unique_id;
        }

        public string GetCountUID()
        {
            return unique_id.unique_id + "_count";
        }

        public static ItemStack GetNearest(Vector3 pos, float range = 999f)
        {
            float min_dist = range;
            ItemStack nearest = null;
            foreach (ItemStack item in stack_list)
            {
                float dist = (item.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = item;
                }
            }
            return nearest;
        }

        public static List<ItemStack> GetAll()
        {
            return stack_list;
        }
    }

}
