using System.Collections;
using UnityEngine;

namespace RhythmGameStarter
{
    public class OrientationHelper : MonoBehaviour
    {
        [Comment("Simple helper for setting the orientation on start")]
        public ScreenOrientation targetRotation;

        private void Start()
        {
            Screen.orientation = targetRotation;
        }
    }
}