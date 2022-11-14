using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine {

    [RequireComponent(typeof(Selectable))]
    public class ShopNPC : MonoBehaviour
    {
        public string title;

        [Header("Buy")]
        public ItemData[] items; //Buy Items

        [Header("Sell")]
        public GroupData sell_group; //Sell Items, if null, can sell anything

        private Selectable selectable;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        public void OpenShop()
        {
            PlayerCharacter character = PlayerCharacter.GetNearest(transform.position);
            if (character != null)
                OpenShop(character);
        }

        public void OpenShop(PlayerCharacter player)
        {
            List<ItemData> buy_items = new List<ItemData>(items);
            ShopPanel.Get().ShowShop(player, title, buy_items, sell_group);
        }
    }

}
