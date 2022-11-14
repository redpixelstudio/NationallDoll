using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FarmingEngine
{

    /// <summary>
    /// Shop slot that shows a single item, along with its price
    /// </summary>

    public class ShopSlot : UISlot
    {
        [Header("Item Slot")]
        public Image icon;
        public Text quantity;
        public Text title;
        public Text cost;
        public Image highlight;

        private Animator animator;

        private ItemData item;
        private bool is_sell;

        protected override void Start()
        {
            base.Start();

            animator = GetComponent<Animator>();

            if (highlight)
                highlight.enabled = false;
            if (title)
                title.enabled = false;
        }

        protected override void Update()
        {
            base.Update();

            if (highlight != null)
                highlight.enabled = selected || key_hover;
            if (title != null)
                title.enabled = selected;
        }

        public void SetBuySlot(ItemData item, int cost)
        {
            SetSlot(item, cost, 1, false);
        }

        public void SetSellSlot(ItemData item, int cost, int quantity, bool active)
        {
            SetSlot(item, cost, quantity, true, active);
        }

        private void SetSlot(ItemData item, int cost, int quantity, bool sell, bool active=true)
        {
            if (item != null)
            {
                CraftData prev = this.item;
                icon.sprite = item.icon;
                icon.enabled = true;
                icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, active ? 1f : 0.5f);
                this.quantity.text = quantity.ToString();
                this.quantity.enabled = quantity > 1;
                this.cost.text = cost.ToString();
                this.cost.enabled = active;
                this.item = item;
                this.is_sell = sell;

                if (title != null)
                {
                    title.enabled = selected;
                    title.text = item.title;
                }

                if (prev != item)
                    AnimateGain();
            }
            else
            {
                this.item = null;
                this.quantity.enabled = false;
                this.cost.enabled = false;
                icon.enabled = false;
                this.selected = false;

                if (highlight != null)
                    highlight.enabled = false;

                if (title != null)
                    title.enabled = false;
            }

            Show();
        }

        public void AnimateGain()
        {
            if (animator != null)
                animator.SetTrigger("Gain");
        }

        public CraftData GetCraftable()
        {
            return item;
        }

        public ItemData GetItem()
        {
            if (item != null)
                return item.GetItem();
            return null;
        }

        public bool IsSell()
        {
            return is_sell;
        }

    }

}