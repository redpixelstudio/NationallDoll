using UnityEngine;

namespace RhythmGameStarter
{
    /// <summary>
    /// Work with CanvasHelper to extend ui in SafeArea to full screen, useful for ui background element
    /// </summary>
    public class CanvasSafeAreaExtender : MonoBehaviour
    {
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            CanvasHelper.OnResolutionOrOrientationChanged.AddListener(UpdateSafeOffset);
        }

        private void UpdateSafeOffset()
        {
            var safeArea = Screen.safeArea;
            var safeOffsetV = Mathf.Abs((Screen.height - safeArea.size.y) / 2);
            var safeOffsetH = Mathf.Abs((Screen.width - safeArea.size.x) / 2);
            // Debug.Log(safeArea);
            // Debug.Log(safeArea.min);
            // Debug.Log(safeArea.max);
            rectTransform.offsetMin = new Vector2(-safeArea.xMin, -safeArea.yMin);
            rectTransform.offsetMax = new Vector2(Screen.width - safeArea.xMax, Screen.height - safeArea.yMax);
        }

        private void Start()
        {
            UpdateSafeOffset();
        }
    }
}