using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Action to attack a destructible (if the destructible cant be attack automatically)
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Attack", order = 50)]
    public class ActionAttack : SAction
    {
        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            if (select.GetDestructible())
            {
                character.Attack(select.GetDestructible());
            }
        }
    }

}