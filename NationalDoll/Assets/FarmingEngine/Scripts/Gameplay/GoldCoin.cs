using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Speed up the gold coin taking action to avoid it appearing in inventory
    /// </summary>

    [RequireComponent(typeof(Item))]
    public class GoldCoin : MonoBehaviour
    {
        private Item item;

        void Awake()
        {
            item = GetComponent<Item>();
            item.onTake += OnTake;
        }

        void OnTake()
        {
            PlayerCharacter character = PlayerCharacter.GetNearest(transform.position);
            if (character != null)
            {
                //Remove from inventory and add to gold amount instead
                character.Inventory.RemoveItem(item.data, item.quantity);
                character.SaveData.gold += item.quantity;
                ItemTakeFX.DoCoinTakeFX(character.transform.position, item.data, character.player_id);
            }
        }
    }

}
