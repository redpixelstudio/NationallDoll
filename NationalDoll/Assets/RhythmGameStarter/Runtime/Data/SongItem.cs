using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RhythmGameStarter
{
    /// <summary>
    /// A lite data class similar to <see cref="SongItem"/> for data storing with Json
    /// </summary>
    [Serializable]
    public class LiteSongItem
    {
        public string name;
        public int bpm;
        public List<SongItem.MidiNote> notes;

        public LiteSongItem(string name, SongItem songItem)
        {
            this.bpm = songItem.bpm;
            this.notes = songItem.notes;
        }

        public static LiteSongItem FromJson(string json)
        {
            return JsonUtility.FromJson(json, typeof(LiteSongItem)) as LiteSongItem;
        }

        public void RecalculateBpmTo(int newBpm)
        {
            //Recalculate the time of each note to the new bpm
            foreach (var note in notes)
            {
                note.time = 60f / newBpm * note.beatIndex;
                note.noteLength = 60f / newBpm * note.beatLengthIndex;
            }

            bpm = newBpm;
        }
    }

    [CreateAssetMenu(fileName = "SongItem", menuName = "Rhythm Game/Song Item", order = 1)]
    public class SongItem : ScriptableObject
    {
        public const int maxTrackNumberForMapping = 6;

        public static NoteName[] noteNameMapping = new NoteName[] { NoteName.C, NoteName.D, NoteName.E, NoteName.F, NoteName.G, NoteName.A };

        public AudioClip clip;

        public string author;

        public int bpm;

        public MetadataList metadata;

        [Serializable]
        public class MetadataList : ReorderableList<Metadata> { }

        [Serializable]
        public class Metadata
        {
            public string id;
            public int intValue;
            public string stringValue;
        }

        public bool TryGetMetadata(string id, out Metadata value)
        {
            var v = metadata.values.Find(x => x.id == id);
            value = v;
            return v != null;
        }

        public bool useCurrentBpmMidiImport;

        [HideInInspector]
        public List<MidiNote> notes = new List<MidiNote>();

        public bool useGenetator;
        public bool runtimeGenerate;

        public SequenceGeneratorBase generator;

        public float onsetSensitivity = 1;
        public float[] onsetData;
        public float onsetMin, onsetMax;

        /// <summary>
        /// Returns a copied version of the notes
        /// </summary>
        /// <returns></returns>
        public List<MidiNote> GetNotes()
        {
            if (useGenetator && runtimeGenerate && generator)
            {
                var temp = generator.OnGenerateSequence(this);
                if (temp != null)
                    return temp;
            }

            return notes.ConvertAll(x => new MidiNote(x));
        }

        [Serializable]
        public class MidiNote
        {
            public NoteName noteName;
            public int noteOctave;
            public float time;
            public float noteLength;
            public float beatIndex;
            public float beatLengthIndex;

            public MidiNote() { }

            public MidiNote(MidiNote other)
            {
                noteName = other.noteName;
                noteOctave = other.noteOctave;
                time = other.time;
                noteLength = other.noteLength;
                beatIndex = other.beatIndex;
                beatLengthIndex = other.beatLengthIndex;
            }

            [NonSerialized]
            public bool created;
        }

        // public TempoChange[] tempoChanges;

        // [Serializable]
        // public class TempoChange
        // {
        //     public float timeStart;
        //     public float timeEnd;
        //     public int bpm;
        // }

        //Rounding our beat to 0.25
        public static float RoundToNearestBeat(float value)
        {
            return (float)Math.Round(value * 4, MidpointRounding.ToEven) / 4;
        }

        // public void ResetNotesState()
        // {
        //     notes.ForEach(x => x.created = false);
        // }

        //Expensive method
        public float[] GenerateOnset()
        {
            var detector = new AudioAnalysis(clip);

            detector.DetectOnsets(onsetSensitivity);
            // detector.NormalizeOnsets(0);
            return detector.GetOnsets();
        }

        public void LoadNotesFrom(LiteSongItem liteSongItem)
        {
            this.notes = liteSongItem.notes;
        }

        public enum NoteName
        {
            C = 0,

            CSharp = 1,

            D = 2,

            DSharp = 3,

            E = 4,

            F = 5,
            FSharp = 6,

            G = 7,

            GSharp = 8,

            A = 9,

            ASharp = 10,

            B = 11
        }
    }
}