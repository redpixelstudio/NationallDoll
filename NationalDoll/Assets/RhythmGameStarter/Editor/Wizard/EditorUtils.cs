
using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace RhythmGameStarter
{
    public static class EditorUtils
    {
        public readonly struct FoldoutScope : IDisposable
        {
            private readonly bool wasIndent;

            public FoldoutScope(AnimBool value, out bool shouldDraw, string label, bool indent = true, SerializedProperty toggle = null, bool isTop = false)
            {
                value.target = Foldout(value.target, label, toggle, isTop);
                shouldDraw = EditorGUILayout.BeginFadeGroup(value.faded);
                if (shouldDraw && indent)
                {
                    Indent();
                    wasIndent = true;
                }
                else
                {
                    wasIndent = false;
                }
            }

            public void Dispose()
            {
                if (wasIndent)
                    EndIndent();
                EditorGUILayout.EndFadeGroup();
            }
        }

        public readonly struct BackgroundColorScope : IDisposable
        {
            private readonly Color previousGUIColor;

            public BackgroundColorScope(Color color)
            {
                previousGUIColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose()
            {
                GUI.backgroundColor = previousGUIColor;
            }
        }

        public readonly struct GUIColorScope : IDisposable
        {
            private readonly Color previousGUIColor;

            public GUIColorScope(Color color)
            {
                previousGUIColor = GUI.color;
                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = previousGUIColor;
            }
        }

        public static void HorizontalLine(float height = 1, float width = -1, Vector2 margin = new Vector2())
        {
            GUILayout.Space(margin.x);

            var rect = EditorGUILayout.GetControlRect(false, height);
            if (width > -1)
            {
                var centerX = rect.width / 2;
                rect.width = width;
                rect.x += centerX - width / 2;
            }

            Color color = EditorStyles.label.normal.textColor;
            color.a = 0.5f;
            EditorGUI.DrawRect(rect, color);

            GUILayout.Space(margin.y);
        }

        public static GUIStyle foldoutStyleTop;
        public static GUIStyle foldoutStyle;

        public static bool Foldout(bool value, string label, SerializedProperty toggle = null, bool isTop = false)
        {
            if (foldoutStyle == null)
            {
                // foldoutStyle = EditorStyles.helpBox;
                foldoutStyle = new GUIStyle("ProjectBrowserHeaderBgMiddle");
                foldoutStyleTop = new GUIStyle("ProjectBrowserHeaderBgTop");
                // foldoutStyle.hover = EditorStyles.miniButton.hover;
            }

            bool _value;
            EditorGUILayout.BeginVertical(isTop ? foldoutStyleTop : foldoutStyle);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();

            {
                var endToggleRect = new Rect(rect);
                endToggleRect.x += endToggleRect.width - 22;
                endToggleRect.width = 20;
                if (toggle != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(endToggleRect, toggle, GUIContent.none);
                    if (EditorGUI.EndChangeCheck() && toggle.boolValue)
                    {
                        _value = true;
                        return _value;
                    }
                }
            }

            {
                var toggleRect = new Rect(rect);
                toggleRect.x += 4;
                toggleRect.width -= 4;
                if (toggle != null && !toggle.boolValue)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    _value = EditorGUI.Toggle(toggleRect, value, EditorStyles.foldout);
                    EditorGUI.EndDisabledGroup();

                    _value = false;
                }
                else
                {
                    _value = EditorGUI.Toggle(toggleRect, value, EditorStyles.foldout);
                }
            }

            rect.x += 20;
            rect.width -= 20;

            if (toggle != null && !toggle.boolValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
            }
            return _value;
        }

        public static void Indent()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            EditorGUILayout.BeginVertical();
        }

        public static void EndIndent()
        {
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

    }
}
