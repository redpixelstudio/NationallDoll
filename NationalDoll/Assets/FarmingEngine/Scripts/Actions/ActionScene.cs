using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    /// <summary>
    /// Change scene when using
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "FarmingEngine/Actions/ChangeScene", order = 50)]
    public class ActionScene : AAction
    {
        public string scene;
        public int entry_index;

        public override void DoAction(PlayerCharacter character, Selectable selectable)
        {
            TheGame.Get().TransitionToScene(scene, entry_index);
        }

        public override bool CanDoAction(PlayerCharacter character, Selectable selectable)
        {
            return true;
        }
    }

}