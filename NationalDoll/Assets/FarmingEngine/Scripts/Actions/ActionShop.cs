using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Shop", order = 50)]
    public class ActionShop : AAction
    {
        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            ShopNPC shop = select.GetComponent<ShopNPC>();
            if (shop != null)
                shop.OpenShop(character);
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            ShopNPC shop = select.GetComponent<ShopNPC>();
            return shop != null;
        }
    }
}
