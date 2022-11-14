using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// FX that show the item going to the inventory when picked
    /// </summary>

    public class ItemTakeFX : MonoBehaviour
    {
        public SpriteRenderer icon;
        public float fx_speed = 10f;

        private Vector3 start_pos;
        private Vector3 start_scale;
        private InventoryType inventory_target;
        private int slot_target = -1;
        private int target_player = 0;
        private float timer = 0f;
        private bool is_coin = false;

        private void Awake()
        {
            start_pos = transform.position;
            start_scale = transform.localScale;
        }

        void Start()
        {
            if (!is_coin && slot_target < 0)
                Destroy(gameObject);
        }

        void Update()
        {
            ItemSlotPanel panel = ItemSlotPanel.Get(inventory_target);
            PlayerUI player_ui = PlayerUI.Get(target_player);

            //Items
            if (!is_coin && panel != null)
            {
                Vector3 wPos = panel.GetSlotWorldPosition(slot_target);
                DoMoveToward(wPos);

                InventoryData inventory = panel.GetInventory();
                InventoryItemData islot = inventory?.GetItem(slot_target);
                if (islot == null || islot.GetItem() == null)
                    Destroy(gameObject);
            }

            //Coins
            if (is_coin && player_ui != null && player_ui.gold_value != null)
            {
                Vector3 wPos = player_ui.gold_value.transform.position;
                DoMoveToward(wPos);
            }

            timer += Time.deltaTime;
            if (timer > 2f)
                Destroy(gameObject);
        }

        private void DoMoveToward(Vector3 target_pos)
        {
            Vector3 dir = target_pos - transform.position;
            Vector3 tDir = target_pos - start_pos;
            float mdist = Mathf.Min(fx_speed * Time.deltaTime, dir.magnitude);
            float scale = dir.magnitude / tDir.magnitude;
            transform.position += dir.normalized * mdist;
            transform.localScale = start_scale * scale;
            transform.rotation = Quaternion.LookRotation(TheCamera.Get().transform.forward, Vector3.up);

            if (dir.magnitude < 0.1f)
                Destroy(gameObject);
        }

        public void SetItem(ItemData item, InventoryType inventory, int slot)
        {
            inventory_target = inventory;
            slot_target = slot;
            icon.sprite = item.icon;
            is_coin = false;
        }

        public void SetCoin(ItemData item, int player_id)
        {
            icon.sprite = item.icon;
            target_player = player_id;
            is_coin = true;
        }

        public static void DoTakeFX(Vector3 pos, ItemData item, InventoryType inventory, int target_slot)
        {
            if (AssetData.Get().item_take_fx != null && item != null)
            {
                GameObject fx = Instantiate(AssetData.Get().item_take_fx, pos, Quaternion.identity);
                fx.GetComponent<ItemTakeFX>().SetItem(item, inventory, target_slot);
            }
        }

        public static void DoCoinTakeFX(Vector3 pos, ItemData item, int player_id)
        {
            if (AssetData.Get().item_take_fx != null && item != null)
            {
                GameObject fx = Instantiate(AssetData.Get().item_take_fx, pos, Quaternion.identity);
                fx.GetComponent<ItemTakeFX>().SetCoin(item, player_id);
            }
        }
    }

}