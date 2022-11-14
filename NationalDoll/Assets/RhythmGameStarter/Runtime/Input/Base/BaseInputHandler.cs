using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    [DisallowMultipleComponent]
    public abstract class BaseInputHandler : MonoBehaviour
    {
        private NoteArea[] noteAreas;

        protected virtual void Start()
        {
            noteAreas = GetComponentsInChildren<NoteArea>();
            for (int i = 0; i < noteAreas.Length; i++)
            {
                var noteArea = noteAreas[i];
                noteArea.keyboardInputHandler = this;
            }
        }

        public virtual bool GetTrackActionKeyDown(Track track, int index) => false;
        public virtual bool GetTrackActionKeyUp(Track track, int index) => false;
        public virtual bool GetTrackActionKey(Track track, int index) => false;
        public virtual bool GetTrackDirectionKey(Note.SwipeDirection swipeDirection) => false;
    }
}