using UnityEngine;

namespace RhythmGameStarter
{
    public class ForceFPS : MonoBehaviour
    {
        [Comment("Simple script to make sure the framerate is high enough on mobile, can remove this you don't need it")]
        public int forcedFrameRate = 60;

        private void Awake()
        {
            // QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = forcedFrameRate;
        }
    }
}