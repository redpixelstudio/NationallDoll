using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    [CreateAssetMenu(fileName = "MidiTrackMapping", menuName = "Rhythm Game/Mapping/Midi Track Mapping", order = 1)]
    public class MidiTrackMapping : ScriptableObject
    {
        //The root octave for the wetdrymidi library is 4
        //For different DAW, there might be different root octave
        public int referenceRootOctave = 4;
        public bool ignoreOctave;
        public List<Track> mapping = new List<Track>();

        [Serializable]
        public class Track
        {
            public SongItem.NoteName noteTarget;
            public int noteOctave;
        }

        public bool CompareMidiMapping(Track target, SongItem.MidiNote note)
        {
            //The root octave for the wetdrymidi library is 4
            var rootOffset = 4 - referenceRootOctave;

            if (ignoreOctave)
                return target.noteTarget == note.noteName;

            return target.noteTarget == note.noteName && target.noteOctave == note.noteOctave - rootOffset;
        }
    }
}