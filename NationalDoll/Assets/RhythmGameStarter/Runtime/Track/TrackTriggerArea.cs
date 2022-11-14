using UnityEngine;
using System.Linq;

namespace RhythmGameStarter
{
    public class TrackTriggerArea : MonoBehaviour
    {
        public TouchWrapperEvent OnNoteTrigger;

        public void TriggerNote(TouchWrapper touch)
        {
            OnNoteTrigger.Invoke(touch);
        }
    }
}