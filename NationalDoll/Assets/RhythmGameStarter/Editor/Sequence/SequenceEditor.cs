using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static RhythmGameStarter.SongItem;
using UnityEditor.Callbacks;
using System;
using UnityEditor.AnimatedValues;

namespace RhythmGameStarter
{
    public class SequenceEditor : EditorWindow, IHasCustomMenu
    {
        [OnOpenAsset(2)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var target = Selection.activeObject as SongItem;
            if (target != null)
            {
                ShowWindow(Selection.activeObject as SongItem);
                return true; //catch open file
            }

            return false; // let unity open the file
        }

        [MenuItem("Tools/Rhythm Game/Sequence Editor", false, 1)]
        private static void ShowWindow()
        {
            var window = GetWindow<SequenceEditor>();
            window.titleContent = new GUIContent("Sequencer");
            window.Show();
        }

        public static void ShowWindow(SongItem item)
        {
            var window = GetWindow<SequenceEditor>();
            window.titleContent = new GUIContent("Sequencer");
            window.Show();

            window.songItem = item;
        }

        #region EDITOR_VARIABLE
        private Vector2 scroll;

        private float viewCount;
        private float _itemLimit;

        private int itemLimit
        {
            get => EditorPrefs.GetInt("RGSItemLimit", 500);
            set
            {
                EditorPrefs.SetInt("RGSItemLimit", value);
                _itemLimit = value;
            }
        }

        private int _numberOfTracksSelected = 0;

        private int numberOfTracksSelected
        {
            get => EditorPrefs.GetInt("RGSNumberOfTracksSelected", 0);
            set
            {
                EditorPrefs.SetInt("RGSNumberOfTracksSelected", value);
                _numberOfTracksSelected = value;
            }
        }

        private int _stepSizeSelected = 2;

        private int stepSizeSelected
        {
            get => EditorPrefs.GetInt("RGSStepSizeSelected", 2);
            set
            {
                EditorPrefs.SetInt("RGSStepSizeSelected", value);
                _stepSizeSelected = value;
            }
        }

        private int _colorPresetSelected = 0;

        private int colorPresetSelected
        {
            get => EditorPrefs.GetInt("RGSColorPresetSelected", 0);
            set
            {
                EditorPrefs.SetInt("RGSColorPresetSelected", value);
                _colorPresetSelected = value;
            }
        }

        private static string[] colorPresetDisplay = new string[] { "Default", "Sharper" };

        private bool _displayNoteDot = true;

        private bool displayNoteDot
        {
            get => EditorPrefs.GetBool("RGSdisplayNoteDot", true);
            set
            {
                EditorPrefs.SetBool("RGSdisplayNoteDot", value);
                _displayNoteDot = value;
            }
        }

        private bool _displayHelp = true;

        private bool displayHelp
        {
            get => EditorPrefs.GetBool("RGSdisplayHelp", true);
            set
            {
                EditorPrefs.SetBool("RGSdisplayHelp", value);
                _displayHelp = value;
            }
        }

        private float beatHeight = 20;
        private float trackWidth = 60f;

        private bool lockToPlayHead = true;

        //Default is root octave
        private int paintOctaveSelected = 0;
        private int paintBeatLengthSelected = 0;

        private AnimBool[] visible;

        #endregion

        private static float[] beatLengthScale = new float[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };
        private static string[] beatLengthScaleDisplay = Array.ConvertAll(beatLengthScale, item => item.ToString());

        //Our scale uses 4 as root octave
        private static int[] octaveScale = new int[] { 4, 5, 6, 7, 8 };

        private static Color[] octaveScaleColor;

        public static Color HtmlToColor(string colorString)
        {
            if (!colorString.StartsWith("#"))
                colorString = $"#{colorString}";

            ColorUtility.TryParseHtmlString(colorString, out var color);
            return color;
        }


        //But display as 3 as root octave to match the DefaultNotePrefabMapping
        private static string[] octaveScaleDisplay = Array.ConvertAll(octaveScale, item => (item - 1).ToString());

        private static int[] trackNumberScale = new int[] { 4, 6 };
        private static string[] trackNumberScaleDisplay = Array.ConvertAll(trackNumberScale, item => item.ToString());

        private static float[] beatStepScale = new float[] { 1, 0.5f, 0.25f };
        private static string[] beatStepScaleDisplay = Array.ConvertAll(beatStepScale, item => item.ToString());

        private GUIStyle centerLabel, titleLabel, scaleLabel, boxStyle, boxNoPaddingStyle;

        private bool inited;

        private SongItem songItem;

        private AudioSource previewAudio;

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Refresh"), false, Refresh);
        }

        public void Refresh()
        {
            inited = false;
            OnEnable();
        }

        public void ResetScroll()
        {
            scroll = new Vector2(0, beatHeight * (_itemLimit + _itemLimit / 8));
        }

        private void OnEnable()
        {
            _itemLimit = itemLimit;

            ResetScroll();

            Undo.undoRedoPerformed += Repaint;
            EditorApplication.update += Update;

            visible = new AnimBool[5];
            for (int i = 0; i < visible.Length; i++)
            {
                visible[i] = new AnimBool(Repaint);
                visible[i].speed = 8;
            }

            visible[0].value = true;
            visible[2].value = true;
            visible[3].value = true;

            _numberOfTracksSelected = Mathf.Min(numberOfTracksSelected, trackNumberScale.Length - 1);
            _stepSizeSelected = stepSizeSelected;
            _displayHelp = displayHelp;
            _displayNoteDot = displayNoteDot;
            _colorPresetSelected = colorPresetSelected;

            UpdateNoteColor();
        }

        public void UpdateNoteColor()
        {
            switch (_colorPresetSelected)
            {
                case 1:
                    octaveScaleColor = new Color[] {
                            Color.green,
                            Color.blue,
                            Color.cyan,
                            Color.magenta,
                            Color.yellow,
                        };
                    break;

                default:
                    if (EditorGUIUtility.isProSkin)
                    {
                        octaveScaleColor = new Color[] {
                            HtmlToColor("6EFF6B"),
                            HtmlToColor("7F81FF"),
                            HtmlToColor("6ECAFF"),
                            HtmlToColor("FFADED"),
                            HtmlToColor("fffb1f"),
                        };
                    }
                    else
                    {
                        octaveScaleColor = new Color[] {
                            HtmlToColor("8ac926"),
                            HtmlToColor("208ed4"),
                            HtmlToColor("3bceac"),
                            HtmlToColor("bc87de"),
                            HtmlToColor("ffca3a"),
                        };
                    }
                    break;
            }
        }

        void Update()
        {
            //Need to repaint for the play head to redraw
            if (previewAudio && previewAudio.isPlaying)
            {
                Repaint();
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
            EditorApplication.update -= Update;

            if (previewAudio)
            {
                DestroyImmediate(previewAudio.gameObject);
            }
        }

        private void InitStyle()
        {
            if (!inited)
            {
                inited = true;
                centerLabel = new GUIStyle(EditorStyles.largeLabel);
                centerLabel.alignment = TextAnchor.MiddleCenter;


                titleLabel = new GUIStyle(EditorStyles.boldLabel);
                titleLabel.fontSize = 18;
                titleLabel.fixedHeight = 36;
                // titleLabel.alignment = TextAnchor.MiddleCenter;


                boxStyle = new GUIStyle("button");
                var color = EditorStyles.label.normal.textColor;
                //Make the dark text color lighter on light skin
                color.a = EditorGUIUtility.isProSkin ? 1 : 0.7f;
                boxStyle.normal.textColor = color;

                boxStyle.fixedHeight = beatHeight;

                scaleLabel = new GUIStyle();
                scaleLabel.fixedHeight = beatHeight;
                scaleLabel.fixedWidth = trackWidth;
                scaleLabel.padding = boxStyle.padding;
                scaleLabel.margin = boxStyle.margin;


                scaleLabel.normal.textColor = EditorStyles.label.normal.textColor;
                scaleLabel.fontSize = 10; //EditorStyles.centeredGreyMiniLabel.fontSize
                scaleLabel.alignment = TextAnchor.MiddleCenter;


                boxNoPaddingStyle = new GUIStyle("box");

                boxNoPaddingStyle.padding = new RectOffset();
                boxNoPaddingStyle.margin = new RectOffset();
            }
        }

        private void HandleSpace()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Space)
                    {
                        HandlePlayButton();
                        e.Use();
                        Repaint();
                    }
                    break;
            }
        }

        public int ColorToolbar(int selected, string[] content, Color[] colors)
        {
            EditorGUILayout.BeginHorizontal();

            var tempColor = GUI.backgroundColor;
            var tempColor2 = GUI.color;
            for (int i = 0; i < content.Length; i++)
            {
                var bgColor = octaveScaleColor[i];
                bgColor.a = selected == i ? 1 : 0.38f;
                GUI.backgroundColor = bgColor;

                var color = GUI.color;
                color.a = selected == i ? 1 : 0.38f;
                GUI.color = color;

                if (GUILayout.Button(content[i]))
                    selected = i;
            }
            GUI.backgroundColor = tempColor;
            GUI.color = tempColor2;

            EditorGUILayout.EndHorizontal();

            return selected;
        }

        private void OnGUI()
        {
            InitStyle();

            HandleSpace();

            viewCount = SongItem.RoundToNearestBeat(Mathf.RoundToInt((position.height / beatHeight)) * (beatStepScale[_stepSizeSelected]));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(280));
            // EditorGUILayout.LabelField("Sequencer", titleLabel);
            using (new EditorUtils.FoldoutScope(visible[0], out var shouldDraw, "SongItem", true, null, true))
            {
                if (shouldDraw)
                {
                    songItem = EditorGUILayout.ObjectField(songItem, typeof(SongItem), false) as SongItem;
                }
            }
            using (new EditorGUI.DisabledScope(!songItem))
            {
                using (new EditorUtils.FoldoutScope(visible[1], out var shouldDraw, "Display"))
                {
                    if (shouldDraw)
                    {
                        EditorGUILayout.LabelField("Number of tracks:");
                        EditorGUI.BeginChangeCheck();
                        var temp = GUILayout.Toolbar(_numberOfTracksSelected, trackNumberScaleDisplay);
                        if (EditorGUI.EndChangeCheck())
                            numberOfTracksSelected = temp;

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Beat Step:");

                        EditorGUI.BeginChangeCheck();
                        temp = GUILayout.Toolbar(_stepSizeSelected, beatStepScaleDisplay);
                        if (EditorGUI.EndChangeCheck())
                            stepSizeSelected = temp;

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Note Color:");

                        EditorGUI.BeginChangeCheck();
                        temp = GUILayout.Toolbar(_colorPresetSelected, colorPresetDisplay);
                        if (EditorGUI.EndChangeCheck())
                        {
                            colorPresetSelected = temp;
                            UpdateNoteColor();
                        }

                        EditorGUILayout.Space();

                        EditorGUI.BeginChangeCheck();
                        var tempBool = EditorGUILayout.Toggle("Display Note Dot", _displayNoteDot);
                        if (EditorGUI.EndChangeCheck())
                            displayNoteDot = tempBool;
                    }
                }

                using (new EditorUtils.FoldoutScope(visible[2], out var shouldDraw, "Draw"))
                {
                    if (shouldDraw)
                    {
                        if (_displayHelp)
                            EditorGUILayout.HelpBox("Different octave can be mapped into different note prefab.", MessageType.Info);
                        EditorGUILayout.LabelField("Octave:");
                        paintOctaveSelected = ColorToolbar(paintOctaveSelected, octaveScaleDisplay, octaveScaleColor);
                        // paintOctaveSelected = GUILayout.Toolbar(paintOctaveSelected, octaveScaleDisplay);
                        EditorGUILayout.Space();
                        if (_displayHelp)
                            EditorGUILayout.HelpBox("Beat length is only used when note prefab is long note", MessageType.Info);
                        EditorGUILayout.LabelField("Beat Length:");
                        paintBeatLengthSelected = GUILayout.Toolbar(paintBeatLengthSelected, beatLengthScaleDisplay);
                    }
                }

                GUILayout.FlexibleSpace();

                using (new EditorUtils.FoldoutScope(visible[3], out var shouldDraw, "Preview Control"))
                {
                    if (shouldDraw)
                    {
                        if (_displayHelp)
                            EditorGUILayout.HelpBox("During play, selecting on the scale area will jump the preview time to there.", MessageType.Info);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var tempBgColor = GUI.backgroundColor;
                            if (previewAudio)
                            {
                                if (previewAudio.isPlaying)
                                    tempBgColor = Color.green;
                                else
                                    tempBgColor = Color.yellow;
                            }
                            using (new BackgroundColorScope(tempBgColor))
                            {
                                if (GUILayout.Button(new GUIContent("Play/Pause", "Use Space as shortcut to play/pause directly.")))
                                {
                                    HandlePlayButton();
                                }
                            }

                            if (GUILayout.Button("Stop"))
                            {
                                ResetScroll();
                                if (previewAudio != null)
                                {
                                    previewAudio.Stop();
                                    DestroyImmediate(previewAudio.gameObject);
                                }
                            }
                        }
                        if (GUILayout.Button(!lockToPlayHead ? "Don't Follow" : "Follow"))
                        {
                            lockToPlayHead = !lockToPlayHead;
                        }
                    }
                }

                using (new EditorUtils.FoldoutScope(visible[4], out var shouldDraw, "Options"))
                {
                    if (shouldDraw)
                    {
                        EditorGUI.BeginChangeCheck();
                        var temp = EditorGUILayout.Toggle("Display Help", _displayHelp);
                        if (EditorGUI.EndChangeCheck())
                            displayHelp = temp;

                        EditorGUI.BeginChangeCheck();
                        var temp2 = EditorGUILayout.DelayedIntField(new GUIContent("Item Limit"), (int)_itemLimit);
                        if (EditorGUI.EndChangeCheck())
                            itemLimit = temp2;

                        if (GUILayout.Button("Clear Notes"))
                        {
                            if (EditorUtility.DisplayDialog("Clear notes", "This will erased all your current notes.", "Clear", "Cancel"))
                            {
                                Undo.RecordObject(songItem, "Clear Notes");
                                songItem.notes.Clear();
                                EditorUtility.SetDirty(songItem);
                            }
                        }
                        if (GUILayout.Button("Import notes from midi"))
                        {
                            var midiFile = SongItemEditor.FindMidiFile(songItem);
                            if (midiFile != null && EditorUtility.DisplayDialog("Import notes from midi", "This will erased all your current notes, and import from the midi clip if found", "Import", "Cancel"))
                            {
                                SongItemEditor.UpdateBpm(midiFile, songItem);
                            }
                        }
                    }
                }
            }
            // else
            // {
            //     GUILayout.FlexibleSpace();
            // }
            EditorGUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();
            rect.x += rect.width - 1;
            rect.width = 1;
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));


            if (songItem)
                DrawSequencer();

            EditorGUILayout.EndHorizontal();
        }

        private void HandlePlayButton()
        {
            if (!previewAudio)
            {
                //Create a dummy audio source in scene to preview the audio
                var tempAudio = new GameObject("Audio Preview");
                tempAudio.tag = "EditorOnly";
                tempAudio.hideFlags = HideFlags.HideAndDontSave;
                previewAudio = tempAudio.AddComponent<AudioSource>();
                previewAudio.clip = songItem.clip;
                previewAudio.Play();
            }
            else
            {
                //We toggle the play/pause
                if (previewAudio.isPlaying)
                    previewAudio.Pause();
                else
                    previewAudio.Play();
            }
        }

        private void DrawSequencer()
        {
            EditorGUILayout.BeginVertical();

            //Calculating the currnet play head
            var playHeadIndex = -2f;
            if (previewAudio)
            {
                playHeadIndex = SongItem.RoundToNearestBeat(previewAudio.time * songItem.bpm / 60);

                //Sync the scroll to the current play head
                if (lockToPlayHead && previewAudio.isPlaying)
                {
                    //Apply a little bit extra offset at the end
                    var scopeOfView = viewCount * beatHeight - (6 * beatStepScale[_stepSizeSelected] * beatHeight);

                    //Sync the scroll position
                    var targetScrollY = (_itemLimit - playHeadIndex) * beatHeight - scopeOfView;
                    if (Event.current.type != EventType.Repaint && Mathf.Abs(scroll.y - targetScrollY) > scopeOfView)
                        scroll.y = targetScrollY;

                    // Debug.Log(playHeadIndex + " " + targetScrollY + " " + scroll.y);
                }
            }

            var tempScroll = EditorGUILayout.BeginScrollView(scroll);

            if (!(previewAudio && previewAudio.isPlaying) || !lockToPlayHead)
                scroll = tempScroll;

            float firstIndex = (int)(scroll.y / beatHeight);
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, _itemLimit - viewCount));

            GUILayout.Space(firstIndex * beatHeight);

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(trackWidth * (trackNumberScale[_numberOfTracksSelected] + 1))))
            {
                //Loop through each row
                for (float j = firstIndex; j < Mathf.Min(_itemLimit, firstIndex + viewCount); j += beatStepScale[_stepSizeSelected])
                {
                    var beatIndex = _itemLimit - j - beatStepScale[_stepSizeSelected];
                    if ((beatIndex + beatStepScale[_stepSizeSelected]) % 8 == 0)
                    {
                        DrawDivider();
                    }

                    EditorGUILayout.BeginHorizontal();
                    for (int i = -1; i < trackNumberScale[_numberOfTracksSelected]; i++)
                    {
                        if (i == -1)
                        {
                            var scaleDisplay = "|";
                            if (beatIndex == (int)beatIndex || beatIndex % 0.5f == 0)
                            {
                                scaleDisplay = (beatIndex + 1).ToString();
                            }
                            GUILayout.Label(scaleDisplay, scaleLabel, GUILayout.Height(beatHeight), GUILayout.Width(trackWidth));

                            //Detect click action on the scale label
                            if (Event.current.isMouse && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                            {
                                if (previewAudio)
                                {
                                    previewAudio.time = 60f / songItem.bpm * beatIndex;
                                    Repaint();

                                    //We stop the preview first if its playing
                                    if (previewAudio.isPlaying)
                                    {
                                        previewAudio.Pause();
                                    }
                                }
                            }
                        }
                        else
                        {
                            DrawStep(i, beatIndex);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (playHeadIndex == beatIndex)
                    {
                        var c = Color.yellow;
                        c.a = EditorGUIUtility.isProSkin ? 0.1f : 0.3f;
                        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), c);
                    }
                }
            }
            // Debug.Log("First + " + FirstIndex.ToString());
            GUILayout.Space(Mathf.Max(0, (_itemLimit - firstIndex - viewCount) * beatHeight));

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal(boxNoPaddingStyle);
            GUILayout.Label("", scaleLabel, GUILayout.Width(trackWidth));
            for (int i = 0; i < trackNumberScale[_numberOfTracksSelected]; i++)
            {
                var targetNoteName = noteNameMapping[i];
                GUILayout.Label(targetNoteName.ToString(), scaleLabel, GUILayout.Width(trackWidth));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStep(int trackIndex, float beatIndex)
        {
            var targetNoteName = noteNameMapping[trackIndex];

            var contains = songItem.notes.FindIndex(x => x.beatIndex == beatIndex && x.noteName == targetNoteName);
            var label = "○";
            var bgColor = GUI.backgroundColor;
            var fontColor = GUI.color;
            MidiNote currentNote = null;
            if (contains != -1)
            {
                currentNote = songItem.notes[contains];
                label = "●";//"<" + "●" + ">";

                //Default to green note
                var color = Color.green;
                var colorIndex = currentNote.noteOctave - 4;
                if (colorIndex >= 0 && colorIndex < octaveScaleColor.Length)
                {
                    color = octaveScaleColor[colorIndex];
                }

                GUI.backgroundColor = color;
            }
            else
            {
                var color2 = bgColor;
                color2.a = 0.2f;
                GUI.backgroundColor = color2;

                var color3 = fontColor;
                color3.a = 0.8f;
                GUI.color = color3;
            }
            var display = new GUIContent(_displayNoteDot ? label : null);
            display.tooltip = "Empty Note";
            if (contains != -1)
            {
                //Setting the tooltip display
                display.tooltip = "Position: " + (currentNote.beatIndex + 1) +
                //Minus by 1 to make it root octave as 3
                "\n" + "Octave: " + (currentNote.noteOctave - 1) +
                "\n" + "Length: " + currentNote.beatLengthIndex
                ;
            }
            if (GUILayout.Button(display, boxStyle, GUILayout.Width(trackWidth), GUILayout.Height(beatHeight)))
            {
                Undo.RecordObject(songItem, "Edit Notes");
                if (contains == -1)
                {
                    songItem.notes.Add(new MidiNote()
                    {
                        beatIndex = beatIndex,
                        beatLengthIndex = beatLengthScale[paintBeatLengthSelected],

                        noteName = targetNoteName,
                        noteOctave = octaveScale[paintOctaveSelected],
                        //in seconds
                        time = 60f / songItem.bpm * beatIndex,
                        noteLength = 60f / songItem.bpm * beatLengthScale[paintBeatLengthSelected],
                    });
                }
                else
                {
                    songItem.notes.RemoveAt(contains);
                }
                EditorUtility.SetDirty(songItem);
            }
            GUI.backgroundColor = bgColor;
            GUI.color = fontColor;
        }

        private void DrawDivider(bool useBeatHeight = true, int thickness = 2, int padding = 10)
        {
            if (useBeatHeight)
                padding = (int)beatHeight - thickness;
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, EditorStyles.centeredGreyMiniLabel.normal.textColor);
        }
    }

    public class BackgroundColorScope : GUI.Scope
    {
        private Color originalColor;
        public BackgroundColorScope(Color color)
        {
            originalColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        protected override void CloseScope()
        {
            GUI.backgroundColor = originalColor;
        }
    }
}
