using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AnimatedValues;

namespace RhythmGameStarter
{
    public class WizardLoader : AssetPostprocessor
    {
        //Path to specifc wizard data we want to display
        public static string WIZARD_DATA_PATH = "Assets/RhythmGameStarter/Editor/Wizard/Wizard Data.asset";

        //Key for editor for only display once
        private static string wizardKey = "RhythmGameStarterWizard";

        static WizardLoader() => EditorApplication.update += CheckForWizard;

        static void CheckForWizard()
        {
            if (!EditorApplication.isUpdating)
            {
                EditorApplication.update -= CheckForWizard;

                var wizardData = AssetDatabase.LoadAssetAtPath<WizardData>(WIZARD_DATA_PATH);
                if (wizardData)
                {
                    var wizardVersion = wizardData.wizardVersionId;
                    if (EditorPrefs.GetInt(wizardKey, -1) == wizardVersion)
                    {
                        return;
                    }
                    EditorPrefs.SetInt(wizardKey, wizardVersion);
                    Wizard.ShowWindow();
                }
            }
        }
    }

    public class Wizard : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Tools/Rhythm Game/Wizard", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<Wizard>();
            window.titleContent = new GUIContent("Wizard");
            window.Show();
        }

        public WizardData wizardData;

        private GUIStyle biggerButton, message, header, questionContainer, question, answer;

        private GUIContent urlIcon, eyeIcon;

        private Vector2 scroll;

        private bool inited;

        private PackageJson parsedPackage;

        [Serializable]
        public class PackageJson
        {
            public string version;
        }

        private void OnEnable()
        {
            minSize = new Vector2(400, 200);

            wizardData = AssetDatabase.LoadAssetAtPath<WizardData>(WizardLoader.WIZARD_DATA_PATH);

            if (wizardData == null)
            {
                Debug.Log("Wizard data not found");
                return;
            }

            foreach (var group in wizardData.wizardActionGroups)
            {
                group.visible = new AnimBool(Repaint);
                group.visible.speed = 8;
            }
        
            if (wizardData.packageJson)
                parsedPackage = JsonUtility.FromJson<PackageJson>(wizardData.packageJson.text);
            else 
                parsedPackage = null;

            if (wizardData.wizardActionGroups.Length >= 1)
                wizardData.wizardActionGroups[0].visible.target = true;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Refresh"), false, Refresh);
        }

        public void Refresh()
        {
            inited = false;
            OnEnable();
        }

        private void OnGUI()
        {
            if (wizardData == null)
            {
                return;
            }

            if (!inited)
            {
                inited = true;

                biggerButton = new GUIStyle("button");
                biggerButton.padding = new RectOffset(12, 12, 6, 6);

                urlIcon = EditorGUIUtility.IconContent("_Help");

                eyeIcon = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "ViewToolOrbit On" : "ViewToolOrbit");



                header = new GUIStyle(EditorStyles.boldLabel);
                header.fontSize = 14;
                header.alignment = TextAnchor.MiddleCenter;

                questionContainer = new GUIStyle();

                answer = new GUIStyle(EditorStyles.label);
                answer.normal.textColor = EditorStyles.label.normal.textColor;
                answer.wordWrap = true;
                answer.alignment = TextAnchor.MiddleLeft;
                answer.stretchWidth = false;
                // var c = answer.normal.textColor;
                // c.a = 0.8f;
                // answer.normal.textColor = c;
                answer.padding = new RectOffset(4, 4, 4, 4);

                message = new GUIStyle(answer);
                // message.normal.textColor = EditorStyles.label.normal.textColor;
                // message.fontSize = 12;
                // message.stretchWidth = true;
                message.alignment = TextAnchor.MiddleCenter;
                message.stretchWidth = true;


                question = new GUIStyle("box");
                question.fontSize = 14;
                question.normal.textColor = EditorStyles.label.normal.textColor;
                question.font = EditorStyles.boldFont;
                question.wordWrap = false;
                question.alignment = TextAnchor.MiddleLeft;
                var c = question.normal.textColor;
                c.a = 0.9f;
                question.normal.textColor = c;
            }

            void DrawButtonLinks()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                foreach (var wAction in wizardData.buttonLinks)
                {
                    urlIcon.text = wAction.actionText;
                    if (GUILayout.Button(urlIcon, biggerButton))
                        Application.OpenURL(wAction.url);
                }
                EditorGUILayout.EndHorizontal();
                // EditorGUILayout.Space();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(position.width));
            EditorGUILayout.Space();
            var h = wizardData.header + (parsedPackage != null ? $" v{parsedPackage.version}" : "");
            EditorGUILayout.LabelField(h, header);
            EditorUtils.HorizontalLine(1, header.CalcSize(new GUIContent(h)).x);
            EditorGUILayout.Space();

            var backgroundColor = EditorGUIUtility.isProSkin ? new Color(30 / 255f, 30 / 255f, 30 / 255f) : new Color(1f, 1f, 1f);
            backgroundColor.a = 0.3f;

            var messageRect = GUILayoutUtility.GetRect(new GUIContent(wizardData.message), message);
            // EditorGUILayout.LabelField(, message);
            EditorGUI.DrawRect(messageRect, backgroundColor);
            EditorGUI.LabelField(messageRect, wizardData.message, message);

            EditorGUILayout.Separator();

            void CloseOtherGroup(WizardData.WizardActionGroup onGroup)
            {
                foreach (var group in wizardData.wizardActionGroups)
                    if (group != onGroup)
                        group.visible.target = false;
            }

            foreach (var group in wizardData.wizardActionGroups)
            {
                if (group.visible == null)
                {
                    Refresh();
                }

                EditorGUI.BeginChangeCheck();
                group.visible.target = EditorUtils.Foldout(group.visible.target, group.groupName);
                if (EditorGUI.EndChangeCheck() && group.visible.target)
                    CloseOtherGroup(group);

                if (EditorGUILayout.BeginFadeGroup(group.visible.faded))
                {
                    EditorUtils.Indent();
                    foreach (var action in group.actions)
                    {
                        EditorGUILayout.BeginVertical(questionContainer);
                        GUILayout.Space(2);

                        using (new EditorUtils.GUIColorScope(Color.yellow))
                        using (new EditorUtils.BackgroundColorScope(EditorGUIUtility.isProSkin ? Color.yellow : new Color(1, 1, 171 / 255f, 0.34f)))
                            EditorGUILayout.LabelField($"Q: {action.question}", question, GUILayout.Height(30));
                        GUILayout.Space(2);

                        var offset = question.CalcSize(new GUIContent("Q: "));

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(offset.x);
                        // using (new EditorUtils.BackgroundColorScope(EditorGUIUtility.isProSkin ? Color.black : Color.white))
                        var answerSize = GUILayoutUtility.GetRect(new GUIContent(action.answer), answer);

                        EditorGUI.DrawRect(answerSize, backgroundColor);
                        // GUILayout.Label(, answer);
                        EditorGUI.LabelField(answerSize, action.answer, answer);
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(2);
                        if (action.wActions != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            foreach (var wAction in action.wActions)
                            {
                                var isUrl = !string.IsNullOrWhiteSpace(wAction.url);

                                if (isUrl)
                                {
                                    urlIcon.text = wAction.actionText;
                                    if (GUILayout.Button(urlIcon, biggerButton))
                                        Application.OpenURL(wAction.url);
                                }
                                else
                                {
                                    eyeIcon.text = wAction.actionText;
                                    if (GUILayout.Button(eyeIcon, biggerButton))
                                    {
                                        if (!string.IsNullOrWhiteSpace(wAction.referenceAssetPath))
                                        {
                                            if (wAction.referenceAssetPath.EndsWith(".unity"))
                                                LoadScene(wAction.referenceAssetPath);
                                            else
                                                PingAsset(LoadAsset<UnityEngine.Object>(wAction.referenceAssetPath));
                                        }
                                        if (!string.IsNullOrWhiteSpace(wAction.referenceSceneObjectName))
                                        {
                                            LocateGameObject(GameObject.Find(wAction.referenceSceneObjectName));
                                        }
                                        GUIUtility.ExitGUI();
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        //Draw vertical line
                        var rect = GUILayoutUtility.GetLastRect();
                        rect.x += 10;
                        rect.y += offset.y + 6;
                        rect.height -= offset.y + 6;
                        rect.width = 2;
                        var color = answer.normal.textColor;
                        color.a = 0.5f;
                        EditorGUI.DrawRect(rect, color);
                        // EditorGUILayout.Separator();
                    }
                    EditorUtils.EndIndent();
                }
                EditorGUILayout.EndFadeGroup();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndScrollView();

            DrawButtonLinks();
        }

        public static void LocateGameObject(GameObject obj)
        {
            Selection.activeGameObject = obj;
            SceneView.FrameLastActiveSceneView();
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        public static void LoadScene(string path)
        {
            if (EditorSceneManager.GetActiveScene().path == path) return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
        }

        public static T LoadAsset<T>(string path) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(path);

        public static void PingAsset(UnityEngine.Object target)
        {
            if (target)
            {
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }

    }
}