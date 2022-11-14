using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RhythmGameStarter
{
    [CustomEditor(typeof(NotePrefabMapping))]
    public class NotePrefabMappingEditor : Editor
    {
        ReorderableList notesPrefabList;
        ReorderableList mappingList;

        SerializedProperty referenceRootOctaveProp;
        SerializedProperty ignoreOctaveProp;
        SerializedProperty ignoreNameProp;

        void OnEnable()
        {
            referenceRootOctaveProp = serializedObject.FindProperty("referenceRootOctave");
            ignoreOctaveProp = serializedObject.FindProperty("ignoreOctave");
            ignoreNameProp = serializedObject.FindProperty("ignoreName");

            notesPrefabList = new ReorderableList(serializedObject, serializedObject.FindProperty("notesPrefab"), false, true, true, true);
            notesPrefabList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 5;

                var element = notesPrefabList.serializedProperty.GetArrayElementAtIndex(index);
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative("prefab"));
                rect.y += rect.height;
                EditorGUI.PropertyField(rect, element.FindPropertyRelative("poolSize"));
            };
            notesPrefabList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Note Prefab");
            };
            notesPrefabList.elementHeightCallback = (index) =>
            {
                return EditorGUIUtility.singleLineHeight * 2 + 10;
            };

            mappingList = new ReorderableList(serializedObject, serializedObject.FindProperty("mapping"), false, true, true, true);
            mappingList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 5;

                var element = mappingList.serializedProperty.GetArrayElementAtIndex(index);
                rect.height = EditorGUIUtility.singleLineHeight;

                if (!ignoreNameProp.boolValue)
                {
                    EditorGUI.PropertyField(rect, element.FindPropertyRelative("noteName"));
                    rect.y += rect.height;
                }
                if (!ignoreOctaveProp.boolValue)
                {
                    EditorGUI.PropertyField(rect, element.FindPropertyRelative("noteOctave"));
                    rect.y += rect.height;
                }

                var selectedIndexProp = element.FindPropertyRelative("notePrefabIndex");
                var displayOptions = new string[notesPrefabList.serializedProperty.arraySize];
                for (int i = 0; i < displayOptions.Length; i++)
                {
                    var obj = notesPrefabList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("prefab").objectReferenceValue;
                    if (obj)
                    {
                        displayOptions[i] = obj.name;
                    }
                }
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    var m_index = EditorGUI.Popup(rect, "Prefab", selectedIndexProp.intValue, displayOptions);
                    if (change.changed)
                    {
                        selectedIndexProp.intValue = m_index;
                    }
                }

            };
            mappingList.drawHeaderCallback = (rect) =>
            {
                EditorGUI.LabelField(rect, "Mapping");
            };
            mappingList.elementHeightCallback = (index) =>
            {
                if (ignoreOctaveProp.boolValue && ignoreNameProp.boolValue)
                    return EditorGUIUtility.singleLineHeight + 10;

                if (ignoreOctaveProp.boolValue || ignoreNameProp.boolValue)
                    return EditorGUIUtility.singleLineHeight * 2 + 10;

                return EditorGUIUtility.singleLineHeight * 3 + 10;
            };
        }

        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();
            // return;

            var mapping = (NotePrefabMapping)target;

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("The first item will be the default note prefab", MessageType.Info);
            notesPrefabList.DoLayoutList();

            EditorGUILayout.HelpBox("For different DAW, there might be different root octave, e.g. Ableton Live & built-in sequencer is 3, FL studio is 5, etc.. It depends on which tool you use to edit the midi clip.", MessageType.Info);
            EditorGUILayout.PropertyField(referenceRootOctaveProp);
            EditorGUILayout.PropertyField(ignoreOctaveProp);
            EditorGUILayout.PropertyField(ignoreNameProp);

            EditorGUILayout.Space();

            mappingList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}