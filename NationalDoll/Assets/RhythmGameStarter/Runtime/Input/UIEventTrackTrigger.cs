using UnityEngine;
using UnityEngine.EventSystems;

namespace RhythmGameStarter
{
    public class UIEventTrackTrigger : MonoBehaviour, IPointerDownHandler
    {
        public TrackTriggerArea triggerArea;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerId != -1)
                triggerArea.TriggerNote(new TouchWrapper(Input.GetTouch(eventData.pointerId)));
        }
    }
}