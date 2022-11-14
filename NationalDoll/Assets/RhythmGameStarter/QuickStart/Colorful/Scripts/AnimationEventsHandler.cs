using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RhythmGameStarter
{
    public class AnimationEventsHandler : MonoBehaviour
    {
        [Comment("Simple event handler for the animation event call back")]
        public bool debugLog;
        public EventList events;

        public void TriggerEvent(string name)
        {
            foreach (var ev in events.values)
            {
                if (ev.name == name)
                {
                    if (debugLog)
                    {
                        Debug.Log("Triggering " + ev.name);
                    }
                    ev.triggerEvent.Invoke();
                    break;
                }
            }
        }
    }

    [System.Serializable]
    public class EventList : ReorderableList<EventEntry> { }

    [System.Serializable]
    public class EventEntry
    {
        public string name;
        public UnityEvent triggerEvent;
    }
}