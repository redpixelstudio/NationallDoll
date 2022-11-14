using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RhythmGameStarter
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SequenceGenerator))]
    public class SequenceGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var algorithmProperty = serializedObject.FindProperty("algorithm");
            var reseedProperty = serializedObject.FindProperty("reseed");

            EditorGUILayout.PropertyField(algorithmProperty);
            EditorGUILayout.PropertyField(reseedProperty);
            if (!reseedProperty.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetTrackCount"));

            if (algorithmProperty.enumValueIndex == 1)
            {
                var usePercentageProperty = serializedObject.FindProperty("usePercentage");

                EditorGUILayout.PropertyField(usePercentageProperty);
                if (!usePercentageProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onsetMinThreshold"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onsetMaxThreshold"));
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onsetMinThresholdPercentage"), new GUIContent("Onset Min %"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onsetMaxThresholdPercentage"), new GUIContent("Onset Max %"));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowHalfStep"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowQuarterStep"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("closeBeatDistance"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    [CreateAssetMenu(fileName = "SequenceGenerator", menuName = "Rhythm Game/Experiemental/Sequence Generator", order = 0)]
    public class SequenceGenerator : SequenceGeneratorBase
    {
        public enum Algorithm
        {
            Random, OnsetRandom
        }

        public Algorithm algorithm;

        [Title("Seed")]
        public bool reseed = true;
        public int seed;

        [Title("Track")]
        [Range(0, SongItem.maxTrackNumberForMapping)]
        public int targetTrackCount = 4;

        [Title("Onset Threshold")]
        [Tooltip("The the actual value will be compared with min/max threshold in percentage of max onset value")]
        public bool usePercentage;
        public float onsetMinThreshold;
        public float onsetMaxThreshold;

        [Range(0, 1)]
        public float onsetMinThresholdPercentage;
        [Range(0, 1)]
        public float onsetMaxThresholdPercentage;

        [Title("Options")]
        public bool allowHalfStep = true;
        public bool allowQuarterStep = true;

        [Range(0, 1)]
        public float closeBeatDistance;

        private void OnValidate()
        {
            closeBeatDistance = Mathf.Max(0, SongItem.RoundToNearestBeat(closeBeatDistance));
        }

        public override List<SongItem.MidiNote> OnGenerateSequence(SongItem songItem)
        {
            //Safety checks
            if (!songItem || !songItem.clip)
            {
                Debug.LogError("Failed to generate, the clip or the songItem is null");
                return null;
            }

            //Seed handling
            if (reseed)
            {
                Random.InitState(System.Environment.TickCount);
            }
            else
            {
                Random.InitState(seed);
            }

            //Calculating the total beat length with quantize to 0.25
            var totalBeat = SongItem.RoundToNearestBeat(songItem.bpm * songItem.clip.length / 60f);
            // Debug.Log(totalBeat);

            var notes = new List<SongItem.MidiNote>();

            ///Begin handling onset notes generations 
            var lastBeatIndex = -1f;
            if (algorithm == Algorithm.OnsetRandom)
            {
                if (songItem.onsetData != null && songItem.onsetData.Length > 0)
                {
                    for (int i = 0; i < songItem.onsetData.Length; i++)
                    {
                        var onsetTime = AudioAnalysis.getTimeFromIndex(songItem.clip, i);
                        var beatIndex = SongItem.RoundToNearestBeat(onsetTime * songItem.bpm / 60);

                        //Check for close beat distance
                        if (lastBeatIndex == -1 || (beatIndex - lastBeatIndex) > closeBeatDistance)
                        {
                            var fallInRange = false;
                            if (usePercentage)
                            {
                                var onsetPrecentage = (songItem.onsetData[i] / songItem.onsetMax);
                                fallInRange = onsetPrecentage > onsetMinThresholdPercentage && onsetPrecentage <= onsetMaxThresholdPercentage;
                            }
                            else
                            {
                                fallInRange = songItem.onsetData[i] > onsetMinThreshold && songItem.onsetData[i] <= onsetMaxThreshold;
                            }
                            if (fallInRange)
                            {
                                if (!allowHalfStep && (beatIndex % 1) == 0.5f)
                                    continue;
                                if (!allowQuarterStep && ((beatIndex % 1) == 0.25f || (beatIndex % 1) == 0.75f))
                                    continue;

                                notes.Add(new SongItem.MidiNote()
                                {
                                    beatIndex = beatIndex,
                                    beatLengthIndex = 1f,

                                    noteName = SongItem.noteNameMapping[Random.Range(0, targetTrackCount)],
                                    noteOctave = 3,
                                    //in seconds
                                    time = onsetTime,
                                    noteLength = 60f / songItem.bpm * 1f,
                                });

                                lastBeatIndex = beatIndex;
                            }
                        }
                    }
                    return notes;
                }
                else
                {
                    Debug.LogWarning("Onset data is empty, nothing generated.");
                    return null;
                }
            }

            //Begin simple random note geneation
            var maxCountX = 1;
            var preferredX = -1;
            var previousX = -1;

            //First to loop through all beat
            for (float y = 0; y < totalBeat; y += 1f)
            {
                var currentCountX = 0;

                maxCountX = 1;
                //Randomize max count x
                if (UnityEngine.Random.value > 0.9f) maxCountX = 2;
                else
                if (UnityEngine.Random.value < 0.2f) maxCountX = 0;

                //Randomize preferred X
                if (maxCountX == 1)
                    while (previousX == preferredX)
                    {
                        preferredX = UnityEngine.Random.Range(0, 4);
                    }

                previousX = preferredX;

                //Seconds to loop through all track
                for (int x = 0; x < targetTrackCount; x++)
                {
                    //Not our targeted track0
                    if (maxCountX == 1 && preferredX != x)
                        continue;

                    //We have more than on quota, try some randomness
                    if (maxCountX > 1)
                    {
                        //Unlucky -> skip
                        if (UnityEngine.Random.value > 0.5f) continue;

                    }
                    //We have reached out quota, skip
                    if (currentCountX >= maxCountX) continue;

                    //Going to create our note at this beat

                    notes.Add(new SongItem.MidiNote()
                    {
                        beatIndex = y,
                        beatLengthIndex = 1f,

                        noteName = SongItem.noteNameMapping[x],
                        noteOctave = 3,
                        //in seconds
                        time = 60f / songItem.bpm * y,
                        noteLength = 60f / songItem.bpm * 1f,
                    });

                    currentCountX++;
                }
            }
            return notes;
        }
    }
}