using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Sleeeep! Until the next day
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/SleepDay", order = 50)]
    public class ActionSleepDay : SAction
    {

        public override void DoAction(PlayerCharacter character, Selectable select)
        {
            TheGame.Get().TransitionToNextDay();
        }
    }

}
