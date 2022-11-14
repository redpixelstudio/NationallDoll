using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    [DisallowMultipleComponent]
    public abstract class BaseTouchHandler : MonoBehaviour
    {
        private NoteArea[] noteAreas;

        protected virtual void Start()
        {
            noteAreas = GetComponentsInChildren<NoteArea>();
            for (int i = 0; i < noteAreas.Length; i++)
            {
                var noteArea = noteAreas[i];
                noteArea.touchInputHandler = this;
            }
        }

        public virtual TouchWrapper GetTouchById(int id) => default(TouchWrapper);
    }
}