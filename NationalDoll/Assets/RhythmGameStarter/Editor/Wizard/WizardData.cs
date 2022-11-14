using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;
#endif

namespace RhythmGameStarter
{
    [CreateAssetMenu(fileName = "Wizard Data", menuName = "Rhythm Game/Extra/Wizard Data", order = 1)]
    public class WizardData : ScriptableObject
    {
        public int wizardVersionId;

        public string header;

        public TextAsset packageJson;

        [Multiline]
        public string message;

        public WAction[] buttonLinks;

        public WizardActionGroup[] wizardActionGroups;

        [Serializable]
        public class WizardActionGroup
        {
            [HideInInspector]
            public string groupName;

            public WizardAction[] actions;
#if UNITY_EDITOR
            [NonSerialized] public AnimBool visible;
#endif
        }

        [Serializable]
        public class WizardAction
        {
            [HideInInspector]
            public string question;

            [Multiline]
            public string answer;
            public WAction[] wActions;
        }

        [Serializable]
        public class WAction
        {
            [HideInInspector] public string referenceSceneObjectName;
            [HideInInspector] public string referenceAssetPath;
            [HideInInspector] public string actionText = "Go";
            public string url;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WizardData))]
    public class WizardDataEditor : Editor
    {
        private SerializedProperty _property;
        private ReorderableList _list;
        private ReorderableList _button_list;

        private Dictionary<string, ReorderableState> stateCache = new Dictionary<string, ReorderableState>();

        private GUIStyle centerLabel;

        public class ReorderableState
        {
            public ReorderableList list;
            public AnimBool visible;

            public Func<float> onGetExtraHeight;
            public Action<Rect> onDrawExtraGUI;

            public bool hasLabelField;

            public void OnGUI(Rect rect)
            {
                var toggleRect = new Rect(rect);
                toggleRect.x -= 14;
                toggleRect.height = EditorGUIUtility.singleLineHeight;
                visible.target = EditorGUI.Toggle(toggleRect, visible.target, EditorStyles.foldout);
                rect.y += EditorGUIUtility.singleLineHeight + 2.0f;

                if (visible.target)
                {
                    if (list != null && hasLabelField)
                        rect.y += EditorGUIUtility.singleLineHeight + 2.0f;

                    if (onDrawExtraGUI != null)
                    {
                        onDrawExtraGUI(rect);
                        rect.y += onGetExtraHeight();
                    }

                    if (list != null)
                        list.DoList(rect);
                }
            }

            public float GetHeight()
            {
                var h = 0f;

                h += EditorGUIUtility.singleLineHeight;

                if (list != null)
                {
                    h += visible.target ? list.GetHeight() : 0;
                }

                if (visible.target && onGetExtraHeight != null)
                    h += onGetExtraHeight();


                if (list != null && hasLabelField)
                    h += EditorGUIUtility.singleLineHeight + 4;

                return h;
            }

        }

        private void OnEnable()
        {
            _property = serializedObject.FindProperty("wizardActionGroups");
            var _button_links_property = serializedObject.FindProperty("buttonLinks");
            _button_list = GetState(_button_links_property, "Links", "actionText", "Label").list;
            _list = GetState(_property, "Wizard Action Groups", "groupName", "Group", (item, index) =>
            {
                return GetState(item.FindPropertyRelative("actions"), "Items", "question", "Q", (_item, _index) =>
                {
                    var answerProperty = _item.FindPropertyRelative("answer");
                    var wActionsProperty = _item.FindPropertyRelative("wActions");
                    var clearIcon = EditorGUIUtility.IconContent("clear");
                    return GetState(wActionsProperty, null, "actionText", "Action", (__item, __index) =>
                    {
                        var referenceSceneObjectNameProperty = __item.FindPropertyRelative("referenceSceneObjectName");
                        var referenceAssetPathProperty = __item.FindPropertyRelative("referenceAssetPath");

                        var urlProperty = __item.FindPropertyRelative("url");

                        var displayURL = !string.IsNullOrWhiteSpace(urlProperty.stringValue);

                        return GetState(__item, null, null, null, null, (rect) =>
                        {
                            if (centerLabel == null)
                            {
                                centerLabel = new GUIStyle("box");
                                centerLabel.font = EditorStyles.boldFont;
                                centerLabel.alignment = TextAnchor.MiddleCenter;
                            }

                            rect.y += 2;
                            rect.height = EditorGUIUtility.singleLineHeight;


                            void DrawLabelClearButton(SerializedProperty property)
                            {
                                var bRect = new Rect(rect);
                                bRect.x += bRect.width - 30;
                                bRect.width = 30;
                                if (GUI.Button(bRect, clearIcon))
                                {
                                    property.stringValue = null;
                                }
                            }

                            if (displayURL)
                            {
                                EditorGUI.PropertyField(rect, urlProperty);
                                rect.y += EditorGUI.GetPropertyHeight(urlProperty) + 2;
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(referenceSceneObjectNameProperty.stringValue))
                                {
                                    EditorGUI.LabelField(rect, referenceSceneObjectNameProperty.stringValue);
                                    DrawLabelClearButton(referenceSceneObjectNameProperty);

                                    rect.y += rect.height;
                                }
                                if (!string.IsNullOrWhiteSpace(referenceAssetPathProperty.stringValue))
                                {
                                    var assetPath = referenceAssetPathProperty.stringValue;
                                    var path = Directory.GetParent(assetPath).Name + "/" + Path.GetFileName(assetPath);
                                    EditorGUI.LabelField(rect, new GUIContent(path, assetPath));

                                    DrawLabelClearButton(referenceAssetPathProperty);

                                    rect.y += rect.height + 2;
                                }
                            }

                            var pRect = new Rect(rect);
                            pRect.height = 24;

                            var _bRect = new Rect(pRect);
                            _bRect.x += _bRect.width - 40;
                            _bRect.width = 40;

                            if (!displayURL)
                            {
                                Event evt = Event.current;
                                GUI.Box(pRect, "Drop here", centerLabel);

                                switch (evt.type)
                                {
                                    case EventType.DragUpdated:
                                    case EventType.DragPerform:
                                        if (!pRect.Contains(evt.mousePosition))
                                            return;

                                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                        if (evt.type == EventType.DragPerform)
                                        {
                                            DragAndDrop.AcceptDrag();

                                            foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                            {
                                                if (dragged_object is GameObject && !PrefabUtility.IsPartOfPrefabAsset(dragged_object))
                                                {
                                                    referenceSceneObjectNameProperty.stringValue = dragged_object.name;
                                                    referenceAssetPathProperty.stringValue = (dragged_object as GameObject).scene.path;
                                                }
                                                if (dragged_object is UnityEngine.Object)
                                                {
                                                    if (AssetDatabase.IsMainAsset(dragged_object))
                                                    {
                                                        referenceAssetPathProperty.stringValue = AssetDatabase.GetAssetPath(dragged_object);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }

                            if (GUI.Button(_bRect, displayURL ? "Url" : "Ref"))
                            {
                                displayURL = !displayURL;
                            }

                        }, () =>
                        {
                            var h = 24f + 4f + 2;

                            if (displayURL)
                            {
                                h += EditorGUI.GetPropertyHeight(urlProperty);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(referenceSceneObjectNameProperty.stringValue))
                                    h += EditorGUIUtility.singleLineHeight;
                                if (!string.IsNullOrWhiteSpace(referenceAssetPathProperty.stringValue))
                                    h += EditorGUIUtility.singleLineHeight;
                            }

                            return h;
                        }, true);
                    }, (rect) =>
                    {
                        var pRect = new Rect(rect);
                        pRect.y += 2;
                        pRect.height = EditorGUI.GetPropertyHeight(answerProperty);
                        EditorGUI.PropertyField(pRect, answerProperty);
                    }, () =>
                    {
                        return EditorGUI.GetPropertyHeight(answerProperty) + 4;
                    });
                });
            }).list;
        }

        public ReorderableState GetState(SerializedProperty item, string headerName, string itemLabelProperty, string itemLabelPropertyLabel, Func<SerializedProperty, int, ReorderableState> getSubState = null, Action<Rect> onDrawExtraGUI = null, Func<float> onGetExtraHeight = null, bool noList = false)
        {
            ReorderableState state;
            stateCache.TryGetValue(item.propertyPath, out state);
            if (state == null)
            {
                state = new ReorderableState();
                state.visible = new AnimBool(Repaint);
                state.visible.speed = 8;

                state.hasLabelField = itemLabelProperty == null;

                if (!noList)
                {
                    var _property = item;
                    state.list = new ReorderableList(serializedObject, _property, true, headerName != null, true, true)
                    {
                        drawElementCallback = (rect, index, isActive, isFocused) =>
                        {
                            var _item = _property.GetArrayElementAtIndex(index);
                            ReorderableState subState = null;
                            if (getSubState != null)
                                subState = getSubState(_item, index);

                            rect.y += 2;
                            rect.x += 10.0f;
                            rect.width -= 10.0f;

                            if (itemLabelProperty != null)
                            {
                                var nameRect = new Rect(rect);
                                nameRect.height = EditorGUIUtility.singleLineHeight;
                                EditorGUI.PropertyField(nameRect, _item.FindPropertyRelative(itemLabelProperty), new GUIContent(itemLabelPropertyLabel));
                            }

                            if (subState != null)
                            {
                                subState.OnGUI(rect);
                            }
                            else
                            {
                                EditorGUI.PropertyField(rect, _item, GUIContent.none, true);
                            }

                        },
                        elementHeightCallback = (index) =>
                        {
                            var _item = _property.GetArrayElementAtIndex(index);
                            var h = 4.0f;
                            ReorderableState subState;
                            if (getSubState != null)
                            {
                                subState = getSubState(_item, index);
                                h += subState.GetHeight();
                            }
                            else
                            {
                                h += EditorGUI.GetPropertyHeight(_item);
                            }

                            return h;
                        }
                    };

                    if (headerName == null)
                    {
                        state.list.headerHeight = 1;
                    }
                    else
                    {
                        state.list.drawHeaderCallback = (rect) =>
                        {
                            EditorGUI.LabelField(rect, string.Format("{0}: {1}", headerName, _property.arraySize), EditorStyles.boldLabel);
                        };
                    }
                }

                state.onGetExtraHeight = onGetExtraHeight;
                state.onDrawExtraGUI = onDrawExtraGUI;

                stateCache.Add(item.propertyPath, state);
            }
            return state;
        }

        private int selectedTab;

        private static readonly string[] tabLabels = new string[] { "General", "Data", "Links" };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            selectedTab = GUILayout.Toolbar(selectedTab, tabLabels);
            EditorGUILayout.Space();

            switch (selectedTab)
            {
                case 0:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wizardVersionId"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("packageJson"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("header"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("message"));
                    break;
                case 1:
                    _list.DoLayoutList();
                    break;
                case 2:
                    _button_list.DoLayoutList();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}