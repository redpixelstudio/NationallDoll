using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    public class Track : MonoBehaviour
    {
        public Transform lineArea;
        public NoteArea noteArea;

        [HideInInspector]
        public Transform notesParent;

        [HideInInspector]
        public IEnumerable<SongItem.MidiNote> allNotes;

        [HideInInspector]
        public AudioSource trackHitAudio;

        [HideInInspector]
        public List<Note> runtimeNote;

        void Awake()
        {
            trackHitAudio = GetComponent<AudioSource>();
            notesParent = new GameObject("Notes").transform;
            notesParent.SetParent(transform);

            notesParent.localScale = Vector3.one;
            
            ResetTrackPosition();
        }

        private void ResetTrackPosition()
        {
            notesParent.transform.position = lineArea.position;
            notesParent.transform.localEulerAngles = Vector3.zero;
        }

        public GameObject CreateNote(GameObject prefab)
        {
            var ordinalScale = prefab.transform.localScale;

            var note = Instantiate(prefab);
            note.transform.SetParent(notesParent);
            note.transform.localEulerAngles = Vector3.zero;

            note.transform.localScale = ordinalScale;

            var noteScript = note.GetComponent<Note>();
            noteScript.inUse = true;
            runtimeNote.Add(noteScript);
            return note;
        }

        public void AttachNote(GameObject noteInstance)
        {
            var ordinalScale = noteInstance.transform.localScale;

            noteInstance.transform.SetParent(notesParent);
            noteInstance.transform.localEulerAngles = Vector3.zero;

            noteInstance.transform.localScale = ordinalScale;

            var note = noteInstance.GetComponent<Note>();
            note.parentTrack = this;
            runtimeNote.Add(note);
        }

        public void DestoryAllNotes()
        {
            runtimeNote.Clear();
            foreach (Transform child in notesParent)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        public void RecycleAllNotes(TrackManager manager)
        {
            runtimeNote.Clear();

            var currentNotes = new List<Transform>();
            foreach (Transform child in notesParent)
            {
                currentNotes.Add(child);
            }
            currentNotes.ForEach(x =>
            {
                manager.ResetNoteToPool(x.gameObject);
            });
        }

        public void ResetTrack()
        {
            ResetTrackPosition();
            
            runtimeNote.Clear();

            noteArea.ResetNoteArea();
        }
    }
}