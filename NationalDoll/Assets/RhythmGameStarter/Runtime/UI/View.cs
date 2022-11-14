using UnityEngine;
using UnityEngine.Events;

namespace RhythmGameStarter
{
    [RequireComponent(typeof(CanvasGroup))]
    public class View : MonoBehaviour
    {
        public bool hideOnStart = true;
        public bool resetPosition = true;
        public float transitionTime = 0.5f;
        private CanvasGroup canvasGroup;

        [Title("Events"), CollapsedEvent]
        public UnityEvent onShow;
        [CollapsedEvent]
        public UnityEvent onHide;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (resetPosition)
            {
                transform.localPosition = new Vector3(0, 0, 0);
            }
        }

        private void Start()
        {
            if (hideOnStart)
            {
                InstantHide();
            }
        }

        private bool isHidden;

        public void Show()
        {
            isHidden = false;
            canvasGroup.alpha = 0;
            gameObject.SetActive(true);

            onShow.Invoke();
        }

        public void Hide()
        {
            isHidden = true;
            
            onHide.Invoke();
        }

        public void InstantHide()
        {
            Hide();
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, isHidden ? 0 : 1, Time.unscaledDeltaTime / transitionTime);

            if (canvasGroup.alpha == 0 && isHidden)
            {
                gameObject.SetActive(false);
            }
        }
    }
}