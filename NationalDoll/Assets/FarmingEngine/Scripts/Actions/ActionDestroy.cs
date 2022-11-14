using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Just destroy the destructible
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/Destroy", order = 50)]
    public class ActionDestroy : AAction
    {
        public string animation;

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            select.GetDestructible().KillIn(0.5f);
            character.TriggerAnim(animation, select.transform.position);
            character.TriggerBusy(0.5f);
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable select)
        {
            return select.GetDestructible() && !select.GetDestructible().IsDead();
        }
    }

}