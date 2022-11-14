using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    public class TriggerInputHandler : BaseInputHandler
    {
        [Title("Track Action Key", 3)]
        [ReorderableDisplay("Track")]
        public TriggerList keyMapping;

        [System.Serializable]
        public class TriggerList : ReorderableList<TriggerState> { }

        public override bool GetTrackActionKeyDown(Track track, int index)
        {
            var key = keyMapping[index];
            return key.justEnter;
        }

        public override bool GetTrackActionKeyUp(Track track, int index)
        {
            var key = keyMapping[index];
            return key.justExit;
        }

        public override bool GetTrackActionKey(Track track, int index)
        {
            var key = keyMapping[index];
            return key.isTriggered;
        }

        public override bool GetTrackDirectionKey(Note.SwipeDirection swipeDirection)
        {
            return false;
        }

    }
}