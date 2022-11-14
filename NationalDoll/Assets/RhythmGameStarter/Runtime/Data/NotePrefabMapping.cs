using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RhythmGameStarter
{
    [CreateAssetMenu(fileName = "NotePrefabMapping", menuName = "Rhythm Game/Mapping/Note Prefab Mapping", order = 1)]
    public class NotePrefabMapping : ScriptableObject
    {
        //The root octave for the wetdrymidi library is 4
        //For differnt DAW, there might be different root octave
        public int referenceRootOctave = 4;
        public bool ignoreOctave;
        public bool ignoreName;

        public List<PrefabPoolEntry> notesPrefab;

        public List<NoteMap> mapping = new List<NoteMap>();

        [Serializable]
        public class PrefabPoolEntry
        {
            public GameObject prefab;
            public int poolSize = 10;
        }

        [Serializable]
        public class NoteMap
        {
            public int notePrefabIndex;
            public SongItem.NoteName noteName;
            public int noteOctave;
        }

        public GameObject GetNotePrefab(SongItem.MidiNote note)
        {
            GameObject prefab = null;

            if (mapping != null && mapping.Count > 0)
            {
                var o = mapping.Find(target =>
                    {
                        return CompareNoteMapping(target, note);
                    });
                if (o != null)
                    prefab = notesPrefab[o.notePrefabIndex].prefab;
            }
            return prefab ? prefab : notesPrefab.FirstOrDefault().prefab;
        }

        public int GetNoteType(SongItem.MidiNote note)
        {
            if (mapping != null && mapping.Count > 0)
            {
                for (int i = 0; i < mapping.Count; i++)
                {
                    var target = mapping[i];
                    if (CompareNoteMapping(target, note))
                    {
                        return target.notePrefabIndex;
                    }
                }
            }
            return 0;
        }

        private bool CompareNoteMapping(NoteMap target, SongItem.MidiNote note)
        {
            //The root octave for the wetdrymidi library is 4
            var rootOffset = 4 - referenceRootOctave;

            if (ignoreOctave)
                return target.noteName == note.noteName;

            if (ignoreName)
                return target.noteOctave == note.noteOctave - rootOffset;

            return target.noteName == note.noteName && target.noteOctave == note.noteOctave - rootOffset;
        }
    }
}