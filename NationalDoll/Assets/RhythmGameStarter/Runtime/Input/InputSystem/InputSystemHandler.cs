using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/input")]
    public class InputSystemHandler : BaseInputHandler
    {
#if ENABLE_INPUT_SYSTEM
        [Title("Input System", 3)]
        public PlayerInput playerInput;
        
        [Title("Track Action Key", 3)]
        [ReorderableDisplay("Track")]
        public InputActionList keyMapping;

        [Title("Swipe Action Key", 3)]
        public InputActionReference upRef;
        public InputActionReference downRef;
        public InputActionReference leftRef;
        public InputActionReference rightRef;

        private InputAction[] actions;
        private InputAction up;
        private InputAction down;
        private InputAction left;
        private InputAction right;


        [System.Serializable]
        public class InputActionList : ReorderableList<InputActionReference> { }

        protected override void Start()
        {
            base.Start();

            actions = new InputAction[keyMapping.Count];
            for (int i = 0; i < keyMapping.Count; i++)
                actions[i] = playerInput.actions[keyMapping[i].name];

            up = playerInput.actions[upRef.name];
            down = playerInput.actions[downRef.name];
            left = playerInput.actions[leftRef.name];
            right = playerInput.actions[rightRef.name];
        }

        public override bool GetTrackActionKeyDown(Track track, int index)
        {
            var key = actions[index];
            return key.WasPressedThisFrame();
        }

        public override bool GetTrackActionKeyUp(Track track, int index)
        {
            var key = actions[index];
            return key.WasReleasedThisFrame();
        }

        public override bool GetTrackActionKey(Track track, int index)
        {
            var key = actions[index];
            return key.IsPressed();
        }

        public override bool GetTrackDirectionKey(Note.SwipeDirection swipeDirection)
        {
            InputAction key = null;
            switch (swipeDirection)
            {
                case Note.SwipeDirection.Up:
                    key = up;
                    break;
                case Note.SwipeDirection.Down:
                    key = down;
                    break;
                case Note.SwipeDirection.Left:
                    key = left;
                    break;
                case Note.SwipeDirection.Right:
                    key = right;
                    break;
            }
            return key != null ? key.IsPressed() : false;
        }

#if UNITY_EDITOR
        class InputActionReferenceComparer : IEqualityComparer<InputActionReference>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(InputActionReference x, InputActionReference y)
            {
                //Check whether the compared objects reference the same data.
                if (System.Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (System.Object.ReferenceEquals(x, null) || System.Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.name == y.name;
            }

            // If Equals() returns true for a pair of objects
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(InputActionReference x)
            {
                //Check whether the object is null
                if (System.Object.ReferenceEquals(x, null)) return 0;
                return x.name.GetHashCode();
            }
        }


        [UnityEditor.CustomEditor(typeof(InputSystemHandler))]
        public class InputSystemHandlerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                serializedObject.Update();
                EditorGUILayout.Space();
                if (GUILayout.Button("Update Input Reference"))
                {
                    var playerInput = serializedObject.FindProperty("playerInput").objectReferenceValue as PlayerInput;
                    // var actionsAsset = serializedObject.FindProperty("actionsAsset").objectReferenceValue as InputActionAsset;
                    var assets = GetAllActionsFromAsset(playerInput.actions).Distinct(new InputActionReferenceComparer()).ToArray();

                    if (assets != null)
                    {
                        var tracks = assets.Where(x =>
                        {
                            return x.action.name.StartsWith("Track");
                        }).ToArray();

                        var keyValues = serializedObject.FindProperty("keyMapping").FindPropertyRelative("values");

                        keyValues.arraySize = tracks.Length;

                        for (int i = 0; i < tracks.Count(); i++)
                        {
                            keyValues.GetArrayElementAtIndex(i).objectReferenceValue = tracks[i];
                        }

                        serializedObject.FindProperty("upRef").objectReferenceValue = GetActionReferenceFromAssets(assets, "Up");
                        serializedObject.FindProperty("downRef").objectReferenceValue = GetActionReferenceFromAssets(assets, "Down");
                        serializedObject.FindProperty("leftRef").objectReferenceValue = GetActionReferenceFromAssets(assets, "Left");
                        serializedObject.FindProperty("rightRef").objectReferenceValue = GetActionReferenceFromAssets(assets, "Right");
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }

            // From InputSystemUIInputModuleEditor
            private static InputActionReference GetActionReferenceFromAssets(InputActionReference[] actions, params string[] actionNames)
            {
                foreach (var actionName in actionNames)
                {
                    foreach (var action in actions)
                    {
                        if (string.Compare(action.action.name, actionName, StringComparison.InvariantCultureIgnoreCase) == 0)
                            return action;
                    }
                }
                return null;
            }

            private static InputActionReference[] GetAllActionsFromAsset(InputActionAsset actions)
            {
                if (actions != null)
                {
                    var path = AssetDatabase.GetAssetPath(actions);
                    // Debug.Log(path);
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    // Debug.Log(assets.Length);
                    return assets.Where(asset => asset is InputActionReference).Cast<InputActionReference>().OrderBy(x => x.name).ToArray();
                }
                return null;
            }
        }
#endif
#endif
    }
}