using UnityEngine;
using UnityEngine.Events;

namespace RhythmGameStarter
{
    public class LongNoteDetecter : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        public UnityEvent OnNoteTouchDown, OnNoteTouchUp;

        [HideInInspector]
        public bool exitedLineArea = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void OnTouchDown()
        {
            OnNoteTouchDown.Invoke();

            var c = spriteRenderer.color;
            c.a = 0.5f;
            spriteRenderer.color = c;
        }

        public void OnTouchUp()
        {
            OnNoteTouchUp.Invoke();

            var c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        void OnTriggerExit(Collider col)
        {
            if (col.tag == "LineArea")
            {
                exitedLineArea = true;
            }
        }
    }
}