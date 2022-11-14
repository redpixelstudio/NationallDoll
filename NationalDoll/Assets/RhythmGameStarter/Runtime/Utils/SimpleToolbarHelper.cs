using System;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    /// <summary>
    /// Simple toolbar ui controller for toggling between multiple UI panels
    /// </summary>
    public class SimpleToolbarHelper : MonoBehaviour
    {
        public int defaultTab = -1;

        [ReorderableDisplay("Tab")]
        public TabList tabs;

        [Serializable]
        public class TabList : ReorderableList<Tab> { }

        [Serializable]
        public class Tab
        {
            public Button button;
            public TMPro.TextMeshProUGUI label;
            public GameObject controlTarget;

            [NonSerialized] public string labelText;

            public void SetActive(bool active)
            {
                controlTarget.SetActive(active);
                label.text = active ? labelText + " >" : labelText;
            }
        }

        public void CloseEverthing()
        {
            foreach (var tt in tabs)
            {
                tt.SetActive(false);
            }
        }

        public void OpenAt(int index)
        {
            if (index >= 0 && index < tabs.Count)
            {
                foreach (var tt in tabs)
                    if (tt != tabs[index]) tt.SetActive(false);

                tabs[index].SetActive(true);
            }
        }

        private void Start()
        {
            foreach (var tt in tabs)
            {
                tt.labelText = tt.label.text;
                tt.SetActive(false);
            }

            foreach (var t in tabs)
            {
                t.button.onClick.AddListener(() =>
                {
                    foreach (var tt in tabs)
                        if (tt != t) tt.SetActive(false);

                    t.SetActive(!t.controlTarget.activeSelf);
                });
            }

            if (defaultTab >= 0 && defaultTab < tabs.Count)
            {
                tabs[defaultTab].SetActive(true);
            }
        }

    }
}