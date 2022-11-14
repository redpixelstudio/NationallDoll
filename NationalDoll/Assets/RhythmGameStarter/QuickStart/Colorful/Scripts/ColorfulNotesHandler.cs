using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    public class ColorfulNotesHandler : MonoBehaviour
    {
        [Comment("Simple script to handle note init callback from TrackManager and assign random color to the notes. Also takes care of most of the custom logic for this demo.")]
        public List<ColorEntry> randomColors;

        private SongManager songManager;

        private void Awake() {
            songManager = GameObject.FindObjectOfType<SongManager>();
        }

        private void Start()
        {
            randomColors.ForEach(x =>
            {
                x.label = x.uiDisplay.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                x.label.text = "-";

                x.uiDisplay.GetComponentInChildren<Image>().color = x.color;
            });

            songManager.onSongStart.AddListener(() =>
            {
                randomColors.ForEach(x =>
                {
                    x.label.text = "-";
                    x.count = 0;
                });
            });
        }

        //For receiving call back from the TrackManager's (onNoteInit) event, when a note is being init
        public void OnNoteInit(Note note)
        {
            var selectedColor = randomColors[Random.Range(0, randomColors.Count)];

            //Loop through all the notes, then assign a random color to them
            foreach (var renderer in note.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.name != "Swipe Indicator")
                    renderer.color = selectedColor.color;
            }

            //We appends the color name to the the note object, so we can recognize it back later on
            note.name = selectedColor.name;
        }

        //For receiving call back from the TrackManager's (onNoteTriggered) event, when a note is being hit
        public void OnNoteTriggered(Note note)
        {
            //Loop through all the notes to find a match one, and we increase the corresponding count, and also update the UI 
            foreach (var color in randomColors)
            {
                if (note.name == color.name)
                {
                    color.count++;
                    color.label.text = color.count.ToString();
                    break;
                }
            }
        }
    }

    [System.Serializable]
    public class ColorEntry
    {
        public string name;
        public Color color;
        public GameObject uiDisplay;

        
        [System.NonSerialized] public TMPro.TextMeshProUGUI label;
        [System.NonSerialized] public float count;
    }
}
