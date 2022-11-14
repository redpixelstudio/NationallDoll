using System.Linq;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RhythmGameStarter
{
    /// <summary>
    /// MonoBehaviour Class for handling the on device note recording saving with Application.persistentDataPath
    /// And also handles the UI for displaying all the previous recordings history
    /// </summary>
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/note-sequences/recording-mode")]
    public class NoteRecorderStorage : MonoBehaviour
    {

#if UNITY_EDITOR
        [MenuItem("Tools/Rhythm Game/Recorder/Reveal Editor Recording Folder", false, 10)]
        private static void RevalEditorRecordingFolder()
        {
            EditorUtility.RevealInFinder(RECORDING_FOLDER);
        }

        [MenuItem("Tools/Rhythm Game/Recorder/Reveal Editor Recording Folder", true, 10)]
        private static bool RevalEditorRecordingFolderCheck()
        {
            return Directory.Exists(RECORDING_FOLDER);
        }
#endif

        [Comment("Handling note recording serialization on device to persistentDataPath")]
        public Transform container;
        public SimpleToolbarHelper toolbarHelper;

        [Tooltip("Must contain TextMeshProUGUI component")]
        public GameObject headerPrefab;

        [Tooltip("Must contain Button & TextMeshProUGUI component")]
        public GameObject itemPrefab;

        public static string RECORDING_FOLDER { get => $"{Application.persistentDataPath}/NoteRecordings"; }

        private void Start()
        {
            EnsureFolder(RECORDING_FOLDER);
            RefreshUI();
        }

        /// <summary>
        /// Used for the callback from NoteRecorder.cs as a custom file saver implementation for non-editor environment
        /// </summary>
        /// <param name="data">Recorded notes data</param>
        public void SaveNewRecord(NoteRecorder.RecordData data)
        {
            // Application.persistentDataPath
            var liteSongItem = new LiteSongItem(data.songItem.name, data.newSongItem);

            var targetFolder = $"{RECORDING_FOLDER}/{data.songItem.name}";
            var targetPath = $"{targetFolder}/{NoteRecorder.GetFileNameWithTime(data.songItem)}.json";

            EnsureFolder(RECORDING_FOLDER);
            EnsureFolder(targetFolder);

            File.WriteAllText(targetPath, JsonUtility.ToJson(liteSongItem));

            RefreshUI();
        }

        private void RefreshUI()
        {
            foreach (Transform child in container)
                GameObject.Destroy(child.gameObject);

            EnsureFolder(RECORDING_FOLDER);

            var allSongsFolder = Directory.GetDirectories(RECORDING_FOLDER);
            foreach (var song in allSongsFolder)
            {
                var folderName = Path.GetFileNameWithoutExtension(song);

                //Creating the header for this song folder
                {
                    var header = Instantiate(headerPrefab, container);
                    header.transform.localScale = Vector3.one;
                    header.name = folderName;

                    var headerLabel = header.GetComponentInChildren<TextMeshProUGUI>();
                    headerLabel.text = header.name;
                }

                string[] recordings = new DirectoryInfo(song).GetFiles().OrderByDescending(f => f.LastWriteTime).Select(x => x.FullName).ToArray();

                foreach (var recording in recordings)
                {
                    var fileName = Path.GetFileNameWithoutExtension(recording);
                    var item = Instantiate(itemPrefab, container);
                    item.transform.localScale = Vector3.one;
                    item.name = fileName.Replace(folderName, null).Trim();

                    var label = item.GetComponentInChildren<TextMeshProUGUI>();
                    label.text = item.name;

                    var button = item.GetComponentInChildren<Button>();
                    button.onClick.AddListener(() =>
                    {
                        if (NoteRecorder.INSTANCE.recordingTarget)
                        {
                            NoteRecorder.INSTANCE.SetPreview(LiteSongItem.FromJson(File.ReadAllText(recording)), fileName);
                            toolbarHelper.OpenAt(0);
                        }
                    });
                }
            }
        }

        private void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}