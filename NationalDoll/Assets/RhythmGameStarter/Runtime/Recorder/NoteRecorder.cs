using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace RhythmGameStarter
{
    /// <summary>
    /// Main script for handling the recording behaviour
    /// </summary>
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/note-sequences/recording-mode")]
    public class NoteRecorder : MonoBehaviour
    {
        public static NoteRecorder INSTANCE;

        [Title("Required")]
        public SongManager targetSongManager;

        [Title("Options")]
        [Tooltip("Set the default mode of the recorder here")]
        public Mode mode;

        [Tooltip("The SongItem target for the NoteRecorder to override in SongManager when begin recording, usually will be a blank SongItem with the AudioClip assigned")]
        public SongItem defaultRecordingTarget;
        [Tooltip("Print out the each note record's detail in the console")]
        public bool debugLog;
        [Tooltip("After how long you pressed will the note be considered a long note")]
        public float longNoteThershold = 0.5f;
        [Tooltip("After how far you swiped (In world space position) will the note be considered a swipe note")]
        public float swipeThreshold = 1f;
        [Tooltip("If enabled, a copy of the recording will be saved right in the editor as a SongItem, only works in Editor.")]
        public bool saveInEditor = true;
        

        [Title("UI Bindings")]
        public TextMeshProUGUI countdownTextUI;
        public TextMeshProUGUI statusTextUI;
        public TextMeshProUGUI recordingModeStatusUI;
        public TMP_Dropdown recordingModeUI;
        public Button saveButtonUI;
        public Button restartButtonUI;
        public Button stopButtonUI;


        [Title("Events")]
        [CollapsedEvent("Will be called when the a note event is recorded")]
        public NoteRecordEvent onNoteRecorded;

        [CollapsedEvent("Will be called when the sequence is going to be saved, useful to link to custom note saving implementation")]
        public RecordDataEvent onSaveRecord;

        [Serializable] public class NoteRecordEvent : UnityEvent<NoteEventData> { }
        [Serializable] public class RecordDataEvent : UnityEvent<RecordData> { }


        #region RUNTIME_FIELD
        [HideInInspector] public bool IsRecording { get => _isRecording || mode == Mode.Recording; private set => _isRecording = value; }
        private SongItem songManagerDefaultSongItem;
        private bool _isRecording;
        [NonSerialized] public SongItem recordingTarget;
        [NonSerialized] public RecordData currentRecord;
        [NonSerialized] public SongItem previewItem;
        private Coroutine currentCountdown;
        #endregion

        public class RecordData
        {
            public SongItem songItem;
            public SongItem newSongItem;
            public List<NoteEventData> allNotes = new List<NoteEventData>();
        }

        public struct NoteEventData
        {
            public Track track;
            public Note.NoteAction action;
            public Note.SwipeDirection swipeDirection;
            public float startTime;
            public float duration;
        }

        public enum Mode
        {
            Recording, Preview, Normal
        }

        private void Awake()
        {
            INSTANCE = this;

            recordingTarget = defaultRecordingTarget;
        }

        private void OnDestroy()
        {
            //Clean up the preview item
            if (previewItem)
            {
                Destroy(previewItem);
            }
        }

        /// <summary>
        /// Setting the current preview item with a LiteSongItem de-serialized from json
        /// </summary>
        /// <param name="liteSongItem">Preview sequence</param>
        /// <param name="fileName">File name to display</param>
        public void SetPreview(LiteSongItem liteSongItem, string fileName)
        {
            if (!previewItem)
            {
                previewItem = Instantiate(recordingTarget);
            }

            previewItem.LoadNotesFrom(liteSongItem);
            previewItem.name = fileName;

            UpdateMode(Mode.Preview);
        }

        /// <summary>
        /// Set the new recording target, usually a SongItem with blank or reference sequence
        /// </summary>
        /// <param name="songItem">New Recording Target</param>
        public void SetRecordingTarget(SongItem songItem)
        {
            //Clean up the current recroding preview item
            if (previewItem)
            {
                Destroy(previewItem);
            }

            recordingTarget = songItem;

            RefreshStatusText();
        }

        /// <summary>
        /// Triggering a refresh of the status display
        /// </summary>
        public void RefreshStatusText()
        {
            UpdateMode(mode);
        }

        /// <summary>
        /// Update and refresh the recording mode internally
        /// </summary>
        /// <param name="mode">The new recording mode to apply</param>
        private void UpdateMode(Mode mode)
        {
            this.mode = mode;

            if (recordingModeUI)
                recordingModeUI.value = (int)mode;

            switch (mode)
            {
                case Mode.Recording:
                    targetSongManager.defaultSong = recordingTarget;
                    UpdateRecordingStatusText(recordingTarget ? $"{recordingTarget.name}" : "No Recording Target");
                    break;
                case Mode.Preview:
                    targetSongManager.defaultSong = previewItem;
                    UpdateRecordingStatusText(previewItem ? $"{previewItem.name}" : "No Preview Target");
                    break;
                case Mode.Normal:
                    targetSongManager.defaultSong = songManagerDefaultSongItem;
                    UpdateRecordingStatusText($"{songManagerDefaultSongItem.name}");
                    break;
            }
        }

        private void UpdateRecordingStatusText(string text)
        {
            if (recordingModeStatusUI)
                recordingModeStatusUI.text = text;
        }

        private void Start()
        {
            //Cache the default song from the SongManager, so we can reset it later if we not recording mode is not on
            songManagerDefaultSongItem = targetSongManager.defaultSong;
            targetSongManager.onSongStart.AddListener(OnSongStart);
            targetSongManager.onSongStartPlay.AddListener(OnSongStartPlay);
            targetSongManager.onSongFinished.AddListener(OnSongFinished);

            UpdateMode(mode);

            HideCountDownUI();

            if (recordingModeUI)
            {
                recordingModeUI.onValueChanged.AddListener((value) =>
                {
                    UpdateMode((Mode)value);
                });
            }

            if (saveButtonUI != null)
            {
                saveButtonUI.onClick.AddListener(SaveAndStop);
                saveButtonUI.interactable = false;
            }

            if (statusTextUI != null)
            {
                statusTextUI.canvasRenderer.SetAlpha(0);
            }

            if (restartButtonUI != null)
            {
                restartButtonUI.onClick.AddListener(() =>
                {
                    targetSongManager.PlaySong();
                });
                restartButtonUI.interactable = false;
            }

            if (stopButtonUI != null)
            {
                stopButtonUI.onClick.AddListener(Stop);
                stopButtonUI.interactable = false;
            }
        }

        IEnumerator BeginCountDown()
        {
            countdownTextUI.gameObject.SetActive(true);
            var startTime = Time.time;
            var delay = targetSongManager.delay;
            var diff = Time.time - startTime;

            while (diff < delay)
            {
                diff = Time.time - startTime;

                if (delay - diff < 0)
                    break;

                countdownTextUI.text = System.Math.Round(delay - diff, 1).ToString();
                yield return null;
            }

            countdownTextUI.gameObject.SetActive(false);
        }

        /// <summary>
        /// Call back from SongManager when a play is invoked before any delay
        /// </summary>
        private void OnSongStartPlay()
        {
            if (countdownTextUI != null)
            {
                if (currentCountdown != null) StopCoroutine(currentCountdown);
                currentCountdown = StartCoroutine(BeginCountDown());
            }

            if (restartButtonUI != null) restartButtonUI.interactable = true;
            if (stopButtonUI != null) stopButtonUI.interactable = true;

            if (recordingModeUI != null) recordingModeUI.interactable = false;
        }

        private void OnSongStart()
        {
            if (mode == Mode.Recording)
            {
                BeginRecording();
            }
        }

        private void OnSongFinished()
        {
            HideCountDownUI();
        }

        private void HideCountDownUI()
        {
            if (countdownTextUI != null)
            {
                if (currentCountdown != null) StopCoroutine(currentCountdown);
                countdownTextUI.gameObject.SetActive(false);
            }
        }

        private void BeginRecording()
        {
            HideCountDownUI();

            if (currentRecord != null || _isRecording)
            {
                Debug.LogWarning("Already in recording!");
                return;
            }

            _isRecording = true;

            if (saveButtonUI != null) saveButtonUI.interactable = true;
        }

        public static string GetFileNameWithTime(SongItem target)
        {
            return $"{target.name} {DateTime.Now:dd-MMM-yy HH-mm-ss}"; //-fff
        }

        /// <summary>
        /// Trigger a save event, if saveInEditor is true, a SongItem will also be created
        /// </summary>
        public void Save()
        {
            if (currentRecord != null)
            {
                var clonedSongItem = Instantiate(recordingTarget);
                clonedSongItem.notes = new List<SongItem.MidiNote>();
                foreach (var noteEventData in currentRecord.allNotes)
                {
                    var beat = noteEventData.startTime * currentRecord.songItem.bpm / 60;
                    var beatLength = noteEventData.duration * currentRecord.songItem.bpm / 60;

                    var beatIndex = SongItem.RoundToNearestBeat(beat);
                    var beatLengthIndex = SongItem.RoundToNearestBeat(beatLength);

                    beatLengthIndex = Mathf.Max(beatLengthIndex, 1);

                    var octave = 3;
                    if (noteEventData.action == Note.NoteAction.LongPress)
                    {
                        octave = 4;
                    }
                    else if (noteEventData.action == Note.NoteAction.Swipe)
                    {
                        switch (noteEventData.swipeDirection)
                        {
                            case Note.SwipeDirection.Right:
                                octave = 5;
                                break;
                            case Note.SwipeDirection.Left:
                                octave = 6;
                                break;
                            case Note.SwipeDirection.Up:
                                octave = 7;
                                break;
                            case Note.SwipeDirection.Down:
                                octave = 8;
                                break;
                        }
                    }

                    clonedSongItem.notes.Add(new SongItem.MidiNote()
                    {
                        noteName = SongItem.noteNameMapping[noteEventData.track.transform.GetSiblingIndex()],
                        noteOctave = octave + 1,

                        //These two is for recalculating the currect time when the bpm is changed
                        beatIndex = beatIndex,
                        beatLengthIndex = beatLengthIndex,

                        //in seconds
                        time = 60f / currentRecord.songItem.bpm * beatIndex,
                        noteLength = 60f / currentRecord.songItem.bpm * beatLengthIndex,
                    });
                }

                currentRecord.newSongItem = clonedSongItem;

#if UNITY_EDITOR
                if (saveInEditor)
                {
                    var savePath = "Assets/NoteRecordings";

                    if (!UnityEditor.AssetDatabase.IsValidFolder(savePath))
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "NoteRecordings");

                    var filePath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(savePath + "/" + GetFileNameWithTime(currentRecord.songItem) + ".asset");

                    UnityEditor.AssetDatabase.CreateAsset(clonedSongItem, filePath);
                }
#endif

                onSaveRecord.Invoke(currentRecord);
            }
        }

        /// <summary>
        /// Stop the current recording session
        /// </summary>
        public void Stop()
        {
            targetSongManager.StopSong();
            _isRecording = false;
            currentRecord = null;

            if (saveButtonUI != null) saveButtonUI.interactable = false;
            if (recordingModeUI != null) recordingModeUI.interactable = true;
        }

        /// <summary>
        /// Stop the current recording session, and trigger a save event
        /// </summary>
        public void SaveAndStop()
        {
            Save();
            Stop();
        }

        /// <summary>
        /// Record a note event entry for the current recording session, used by NoteArea.cs
        /// </summary>
        /// <param name="data">Note event data</param>
        public void RecordNote(NoteEventData data)
        {
            if (!_isRecording) return;

            if (currentRecord == null)
            {
                currentRecord = new RecordData()
                {
                    songItem = targetSongManager.currentSongItem,
                };
            }

            currentRecord.allNotes.Add(data);

            onNoteRecorded.Invoke(data);

            if (statusTextUI)
            {
                statusTextUI.text = data.action + (data.action == Note.NoteAction.Swipe ? $", {data.swipeDirection}" : null) + " at " + (data.track.transform.GetSiblingIndex() + 1).ToString();

                statusTextUI.canvasRenderer.SetAlpha(1);
                statusTextUI.CrossFadeAlpha(0, 0.8f, true);
            }

            if (debugLog)
                Debug.Log($"Track {data.track.name} data : {data.action}, {data.startTime}, {data.swipeDirection}");
        }
    }
}