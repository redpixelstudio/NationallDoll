using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    //Base class for note sequence Generator, can implement your own generation method and use with existing RhythmCore features
    public abstract class SequenceGeneratorBase : ScriptableObject
    {
        public abstract List<SongItem.MidiNote> OnGenerateSequence(SongItem songItem);
    }
}