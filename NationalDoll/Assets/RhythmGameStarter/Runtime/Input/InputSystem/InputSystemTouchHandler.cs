using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/input")]
    public class InputSystemTouchHandler : BaseTouchHandler
    {
#if ENABLE_INPUT_SYSTEM
        protected override void Start()
        {
            base.Start();
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        }

        void Update()
        {
            //Input
            var touches = Touch.activeTouches;
            for (int i = 0; i < touches.Count; ++i)
            {
                var touch = touches[i];

                //We also see if this touch was used by the UI
                if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.touchId))
                    TouchAt(touch);
            }
        }

        private void TouchAt(Touch touch)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);
            // print(touch.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var t = hit.rigidbody.GetComponent<TrackTriggerArea>();
                if (!t) return;

                t.TriggerNote(new TouchWrapper(touch));
            }
        }

        public override TouchWrapper GetTouchById(int id)
        {
            foreach (var touch in Touch.activeTouches)
                if (touch.touchId == id) return new TouchWrapper(touch);

            return default(TouchWrapper);
        }
#endif
    }
}