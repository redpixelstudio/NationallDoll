using UnityEditor;
using System.IO;
using System;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using UnityEditor.AnimatedValues;
using Melanchall.DryWetMidi.Tools;

namespace RhythmGameStarter
{
    [CustomEditor(typeof(SongItem))]
    public class SongItemEditor : Editor
    {
        private SerializedProperty onsetDataProperty;
        private SerializedProperty useGenetatorProperty;
        private SerializedProperty genetatorProperty;
        private SerializedProperty useCurrentBpmMidiImportProperty;

        private AnimBool songVisibleAnim, sequenceVisibleAnim, metadataVisibleAnim, generatorVisibleAnim;

        private Editor generatorEditor;

        private GUIStyle dropdownButtonStyles;

        private void OnEnable()
        {
            onsetDataProperty = serializedObject.FindProperty("onsetData");
            useGenetatorProperty = serializedObject.FindProperty("useGenetator");
            genetatorProperty = serializedObject.FindProperty("generator");
            useCurrentBpmMidiImportProperty = serializedObject.FindProperty("useCurrentBpmMidiImport");

            songVisibleAnim = new AnimBool(true, Repaint);
            songVisibleAnim.speed = 8f;

            sequenceVisibleAnim = new AnimBool(true, Repaint);
            sequenceVisibleAnim.speed = 8f;

            metadataVisibleAnim = new AnimBool(Repaint);
            metadataVisibleAnim.speed = 8f;

            generatorVisibleAnim = new AnimBool(Repaint);
            generatorVisibleAnim.speed = 8f;

            UpdateGeneratorEditor(false);
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        private void OnDestroy()
        {
            if (generatorEditor)
                DestroyImmediate(generatorEditor);
        }

        private void UpdateGeneratorEditor(bool changed)
        {
            if (changed && generatorEditor)
            {
                DestroyImmediate(generatorEditor);
            }

            if (genetatorProperty.objectReferenceValue && !generatorEditor)
            {
                generatorEditor = Editor.CreateEditor(genetatorProperty.objectReferenceValue);
            }
        }

        public override void OnInspectorGUI()
        {
            var songItem = (SongItem)target;

            // EditorGUI.indentLevel--;

            if (dropdownButtonStyles == null)
            {
                dropdownButtonStyles = new GUIStyle("DropDownButton");
                dropdownButtonStyles.alignment = TextAnchor.MiddleCenter;
                dropdownButtonStyles.margin = new RectOffset(3, 3, 0, 0);
            }

            serializedObject.Update();

            songVisibleAnim.target = EditorUtils.Foldout(songVisibleAnim.target, "Song", null, false);
            if (EditorGUILayout.BeginFadeGroup(songVisibleAnim.faded))
            {
                EditorUtils.Indent();
                // TitleAttributeDrawer.OnGUILayout("Song");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("author"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("clip"));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.DelayedIntField(serializedObject.FindProperty("bpm"));
                EditorGUILayout.HelpBox("The time of each note will be recalculated base on the bpm when you changed it.", MessageType.Info);
                var changed = EditorGUI.EndChangeCheck();
                if (changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    UpdateNotesTime(songItem);
                    serializedObject.Update();
                }
                EditorUtils.EndIndent();
            }
            EditorGUILayout.EndFadeGroup();

            sequenceVisibleAnim.target = EditorUtils.Foldout(sequenceVisibleAnim.target, "Sequence");
            if (EditorGUILayout.BeginFadeGroup(sequenceVisibleAnim.faded))
            {
                EditorUtils.Indent();
                EditorGUILayout.PropertyField(useCurrentBpmMidiImportProperty, new GUIContent("Import With Current Bpm",
                 "Some DAW export midi (e.g. Ableton) clip without bpm info, so enable this will force to import the midi with the current bpm while with disabled this, the bpm will be detected from midi file and update accordinly."));
                if (EditorGUILayout.DropdownButton(new GUIContent("Import notes"), FocusType.Keyboard, dropdownButtonStyles))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("MIDI"), false, () =>
                    {
                        var midiFile = FindMidiFile(songItem);
                        if (midiFile != null && EditorUtility.DisplayDialog("Import notes from midi", "This will erased all your current notes, and import from the midi clip if found", "Import", "Cancel"))
                        {
                            serializedObject.ApplyModifiedProperties();
                            Undo.RecordObject(songItem, "Import from Midi");
                            UpdateBpm(midiFile, songItem);
                            serializedObject.Update();
                        }
                    });
                    menu.AddItem(new GUIContent("JSON"), false, () =>
                    {
                        var importedLiteSongItem = FindJsonFile(songItem);
                        if (importedLiteSongItem != null && EditorUtility.DisplayDialog("Import notes from json", "This will erased all your current notes, and import notes from the json file", "Import", "Cancel"))
                        {
                            serializedObject.ApplyModifiedProperties();
                            if (importedLiteSongItem.bpm != songItem.bpm)
                            {
                                var index = EditorUtility.DisplayDialogComplex("Bpm doesn't match", "Bpm doesn't match! You have few options", "Use SongItem's", "Use Json file's", "Cancel");
                                if (index == 0)
                                    importedLiteSongItem.bpm = songItem.bpm;
                                else if (index == 1)
                                    songItem.bpm = importedLiteSongItem.bpm;
                                else if (index == 2)
                                {
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                            }
                            Undo.RecordObject(songItem, "Import from Json");
                            importedLiteSongItem.RecalculateBpmTo(importedLiteSongItem.bpm);
                            songItem.LoadNotesFrom(importedLiteSongItem);
                            EditorUtility.SetDirty(songItem);
                            serializedObject.Update();
                        }
                    });
                    menu.ShowAsContext();

                    // GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("Edit notes"))
                {
                    SequenceEditor.ShowWindow(songItem);
                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("Number of notes : " + serializedObject.FindProperty("notes").arraySize.ToString());
                EditorGUI.EndDisabledGroup();
                EditorUtils.EndIndent();
            }
            EditorGUILayout.EndFadeGroup();

            metadataVisibleAnim.target = EditorUtils.Foldout(metadataVisibleAnim.target, "Metadata", null, false);
            if (EditorGUILayout.BeginFadeGroup(metadataVisibleAnim.faded))
            {
                EditorUtils.Indent();
                EditorGUILayout.HelpBox("Extra metadata for this SongItem, which can be retrieve via the TryGetMetadata method", MessageType.Info);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("metadata"));
                EditorUtils.EndIndent();
            }
            EditorGUILayout.EndFadeGroup();

            generatorVisibleAnim.target = EditorUtils.Foldout(generatorVisibleAnim.target, "Generator [Experimental]", useGenetatorProperty);
            if (EditorGUILayout.BeginFadeGroup(generatorVisibleAnim.faded))
            {
                EditorUtils.Indent();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Onset", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onsetSensitivity"));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(onsetDataProperty.arraySize == 0 ? "Analyse Onset" : "Reanalyse"))
                {
                    serializedObject.ApplyModifiedProperties();
                    Undo.RecordObject(songItem, "Analyse Onset");
                    songItem.onsetData = songItem.GenerateOnset();

                    float max = 0;
                    float min = 0;

                    // Find strongest/weakest onset
                    for (int i = 0; i < songItem.onsetData.Length; i++)
                    {
                        max = Math.Max(max, songItem.onsetData[i]);
                        min = Math.Min(min, songItem.onsetData[i]);
                    }

                    songItem.onsetMax = max;
                    songItem.onsetMin = min;
                    EditorUtility.SetDirty(songItem);
                    serializedObject.Update();
                }
                if (onsetDataProperty.arraySize != 0)
                {
                    if (GUILayout.Button("Clear"))
                    {
                        onsetDataProperty.ClearArray();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox("Onset data length : " + onsetDataProperty.arraySize + "\n Min : " + songItem.onsetMin + "\n Max : " + songItem.onsetMax, MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("runtimeGenerate"));

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(genetatorProperty);
                UpdateGeneratorEditor(EditorGUI.EndChangeCheck());
                if (generatorEditor)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    generatorEditor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                    EditorGUILayout.EndVertical();
                }
                if (genetatorProperty.objectReferenceValue && GUILayout.Button("Generate"))
                {
                    var notes = ((SequenceGeneratorBase)genetatorProperty.objectReferenceValue).OnGenerateSequence(songItem);
                    if (notes != null)
                    {
                        serializedObject.ApplyModifiedProperties();
                        Undo.RecordObject(songItem, "Generate Notes");
                        songItem.notes = notes;
                        EditorUtility.SetDirty(songItem);
                        serializedObject.Update();

                        SequenceEditor.ShowWindow(songItem);
                    }
                }
                EditorUtils.EndIndent();
            }
            EditorGUILayout.EndFadeGroup();

            serializedObject.ApplyModifiedProperties();
        }

        public static LiteSongItem FindJsonFile(SongItem target)
        {
            var path = AssetDatabase.GetAssetPath((SongItem)target);
            var selectedMidFilePath = EditorUtility.OpenFilePanelWithFilters("Import sequence from json", Directory.GetParent(path).ToString(), new string[] { "Json File", "json" });

            if (string.IsNullOrEmpty(selectedMidFilePath)) return null;

            var json = File.ReadAllText(selectedMidFilePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("Json file empty!");
                return null;
            }
            return LiteSongItem.FromJson(json);
        }

        public static MidiFile FindMidiFile(SongItem target)
        {
            var path = AssetDatabase.GetAssetPath((SongItem)target);

            var selectedMidFilePath = EditorUtility.OpenFilePanelWithFilters("Import sequence from midi", Directory.GetParent(path).ToString(), new string[] { "Midi File", "mid" });

            if (!string.IsNullOrEmpty(selectedMidFilePath))
            {
                return MidiFile.Read(selectedMidFilePath);
            }

            // string filePath = path.Substring(0, path.Length - Path.GetFileName(path).Length);

            // string[] files = Directory.GetFiles(filePath, "*.mid");
            // if (files.Length > 0)
            // {
            //     // string midiFileName = Path.Combine(filePath, );
            //     return MidiFile.Read(files[0]);
            // }
            return null;
        }

        public static void UpdateNotesTime(SongItem songItem)
        {
            foreach (var note in songItem.notes)
            {
                note.time = 60f / songItem.bpm * note.beatIndex;
                note.noteLength = 60f / songItem.bpm * note.beatLengthIndex;
            }
        }

        public static void UpdateBpm(MidiFile rawMidi, SongItem songItem)
        {
            if (rawMidi == null)
            {
                Debug.LogError("Cannot find the midi file");
                return;
            }
            songItem.notes.Clear();
            var detectedTempoMap = rawMidi.GetTempoMap();
            var tempoMap = songItem.useCurrentBpmMidiImport ? TempoMap.Create(detectedTempoMap.TimeDivision, Tempo.FromBeatsPerMinute(songItem.bpm), detectedTempoMap.TimeSignature.AtTime(0)) : detectedTempoMap;

            if (!songItem.useCurrentBpmMidiImport)
                songItem.bpm = (int)rawMidi.GetTempoMap().Tempo.AtTime(0).BeatsPerMinute;

            Debug.Log($"Updating Midi Data {tempoMap.TimeDivision}, {songItem.bpm}bpm");

            int count = 0;
            foreach (var note in rawMidi.GetNotes())
            {
                count++;
                var beat = GetMetricTimeSpanTotal(note.TimeAs<MetricTimeSpan>(tempoMap)) * songItem.bpm / 60;
                var beatLength = GetMetricTimeSpanTotal(note.LengthAs<MetricTimeSpan>(tempoMap)) * songItem.bpm / 60;
                // Debug.Log(RoundToNearestBeat(beat));

                songItem.notes.Add(new SongItem.MidiNote()
                {
                    noteName = ParseEnum<SongItem.NoteName>(note.NoteName.ToString()),
                    noteOctave = note.Octave,
                    time = GetMetricTimeSpanTotal(note.TimeAs<MetricTimeSpan>(tempoMap)),
                    noteLength = GetMetricTimeSpanTotal(note.LengthAs<MetricTimeSpan>(tempoMap)),

                    //This two is for recalculating the currect time when the bpm is changed
                    beatIndex = SongItem.RoundToNearestBeat(beat),
                    beatLengthIndex = SongItem.RoundToNearestBeat(beatLength),
                });
            }
            Debug.Log(count + " Note(s) detected from Midi file");
            EditorUtility.SetDirty(songItem);
        }

        private static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private static float GetMetricTimeSpanTotal(MetricTimeSpan ts)
        {
            return (float)ts.TotalMicroseconds / 1000000f;
        }
    }
}