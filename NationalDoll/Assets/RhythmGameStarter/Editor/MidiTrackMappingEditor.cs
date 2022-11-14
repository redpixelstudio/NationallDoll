using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RhythmGameStarter
{
    [CustomEditor(typeof(MidiTrackMapping))]
    public class MidiTrackMappingEditor : Editor
    {
        ReorderableList mappingList;

        SerializedProperty ignoreOctaveProp;
        SerializedProperty referenceRootOctaveProp;

        void OnEnable()
        {
            referenceRootOctaveProp = serializedObject.FindProperty("referenceRootOctave");
            ignoreOctaveProp = serializedObject.FindProperty("ignoreOctave");

            mappingList = new ReorderableList(serializedObject, serializedObject.FindProperty("mapping"), false, true, true, true);
            mappingList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 5;

                var element = mappingList.serializedProperty.GetArrayElementAtIndex(index);
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, "Track: " + (index + 1));

                rect.y += rect.height;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative("noteTarget"));

                if (!ignoreOctaveProp.boolValue)
                {
                    rect.y += rect.height;
                    EditorGUI.PropertyField(rect, element.FindPropertyRelative("noteOctave"));
                }
            };
            mappingList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Mapping");
            };
            mappingList.elementHeightCallback = (index) =>
            {
                if (ignoreOctaveProp.boolValue)
                    return EditorGUIUtility.singleLineHeight * 2 + 10;

                return EditorGUIUtility.singleLineHeight * 3 + 10;
            };
        }

        public override void OnInspectorGUI()
        {
            var mapping = (MidiTrackMapping)target;

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("For different DAW, there might be different root octave, e.g. Ableton Live & built-in sequencer is 3, FL studio is 5, etc.. It depends on which tool you use to edit the midi clip.", MessageType.Info);
            EditorGUILayout.PropertyField(referenceRootOctaveProp);
            EditorGUILayout.PropertyField(ignoreOctaveProp);

            EditorGUILayout.Space();

            mappingList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}