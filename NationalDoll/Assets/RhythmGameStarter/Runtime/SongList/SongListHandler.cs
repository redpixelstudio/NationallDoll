using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    public class SongListHandler : MonoBehaviour
    {
        // [Comment("Handling note recording target switching")]
        public Transform container;

        [Tooltip("Must contain Button & TextMeshProUGUI component")]
        public GameObject itemPrefab;

        [ReorderableDisplay("Item")]
        public SongItemList songItems;

        [Comment("Events")]
        [CollapsedEvent]
        public SongItemEvent onItemSelect;


        [Serializable] public class SongItemList : ReorderableList<SongItem> { }

        private void Start()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            foreach (Transform child in container)
                GameObject.Destroy(child.gameObject);

            int i = 0;
            foreach (var target in songItems)
            {
                if (!target) continue;

                i++;

                var itemName = target.name;

                var item = Instantiate(itemPrefab, container);
                item.transform.localScale = Vector3.one;
                item.name = itemName;

                var songListItem = item.GetComponent<SongListItem>();

                songListItem.label.text = target.name;
                songListItem.authorLabel.text = target.author;
                songListItem.indexLabel.text = i.ToString();
                if (target.TryGetMetadata("difficulties", out var difficulties))
                {
                    songListItem.difficultiesFill.fillAmount = difficulties.intValue / 3f;
                }
                else
                {
                    songListItem.difficultiesFill.gameObject.SetActive(false);
                }
                songListItem.button.onClick.AddListener(() =>
                {
                    onItemSelect.Invoke(target);
                });
            }
        }
    }
}