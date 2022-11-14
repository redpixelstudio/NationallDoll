using UnityEditor;
using System.IO;
using System;
using UnityEngine;

namespace RhythmGameStarter
{
    [CustomEditor(typeof(Note))]
    public class NoteEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var note = (Note)target;

            serializedObject.Update();

            var scoreProp = serializedObject.FindProperty("score");
            var actionProp = serializedObject.FindProperty("action");

            var swipeDirectionProp = serializedObject.FindProperty("swipeDirection");
            var swipeThresholdProp = serializedObject.FindProperty("swipeThreshold");

            var applyNoteLenghtTargetProp = serializedObject.FindProperty("applyNoteLenghtTarget");
            var noteLengthSizeOffsetProp = serializedObject.FindProperty("noteLengthSizeOffset");

            var killAnimProp = serializedObject.FindProperty("killAnim");
            var noTapEffectProp = serializedObject.FindProperty("noTapEffect");
            var hitSoundProp = serializedObject.FindProperty("hitSound");
            var noHitSoundProp = serializedObject.FindProperty("noHitSound");

            EditorGUILayout.PropertyField(scoreProp);
            EditorGUILayout.PropertyField(actionProp);

            //Tap, LongTap, Swipe
            switch (actionProp.enumValueIndex)
            {
                case 0:
                    break;
                case 1:
                    using (new EditorGUILayout.VerticalScope("HelpBox"))
                    {
                        EditorGUILayout.LabelField("Long Tap", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(applyNoteLenghtTargetProp);
                        EditorGUILayout.PropertyField(noteLengthSizeOffsetProp);
                    }
                    EditorGUILayout.Space();
                    break;
                case 2:
                    using (new EditorGUILayout.VerticalScope("HelpBox"))
                    {
                        EditorGUILayout.LabelField("Swipe", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(swipeDirectionProp);
                        EditorGUILayout.PropertyField(swipeThresholdProp);
                    }
                    EditorGUILayout.Space();
                    break;
            }

            EditorGUILayout.PropertyField(killAnimProp);
            EditorGUILayout.PropertyField(noTapEffectProp);
            EditorGUILayout.PropertyField(noHitSoundProp);

            if (!noHitSoundProp.boolValue)
                EditorGUILayout.PropertyField(hitSoundProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}