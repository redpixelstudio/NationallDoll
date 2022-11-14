using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    /// <summary>
    /// Simple script for handling recording target switching
    /// </summary>
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/note-sequences/recording-mode")]
    public class NoteRecorderTargetList : MonoBehaviour
    {
        [Comment("Handling note recording target switching")]
        public Transform container;
        public SimpleToolbarHelper toolbarHelper;

        [Tooltip("Must contain Button & TextMeshProUGUI component")]
        public GameObject itemPrefab;

        [Title("Recording Targets")]
        public bool setFirstAsDefault = true;

        [ReorderableDisplay("Item")]
        public SongItemList recordingTargets;

        [Serializable] public class SongItemList : ReorderableList<SongItem> { }

        private void Start()
        {
            if (setFirstAsDefault)
            {
                NoteRecorder.INSTANCE.SetRecordingTarget(recordingTargets.First());
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            foreach (Transform child in container)
                GameObject.Destroy(child.gameObject);

            foreach (var target in recordingTargets)
            {
                if (!target) continue;

                var itemName = target.name;

                var item = Instantiate(itemPrefab, container);
                item.transform.localScale = Vector3.one;
                item.name = itemName;

                var headerLabel = item.GetComponentInChildren<TextMeshProUGUI>();
                headerLabel.text = item.name;

                var button = item.GetComponentInChildren<Button>();
                button.onClick.AddListener(() =>
                {
                    if (NoteRecorder.INSTANCE.recordingTarget)
                    {
                        NoteRecorder.INSTANCE.SetRecordingTarget(target);
                        toolbarHelper.OpenAt(0);
                    }
                });
            }
        }
    }
}