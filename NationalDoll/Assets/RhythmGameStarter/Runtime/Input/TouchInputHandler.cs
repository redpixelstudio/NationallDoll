using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/input")]
    public class TouchInputHandler : BaseTouchHandler
    {
        void Update()
        {
            //Input
            for (int i = 0; i < Input.touchCount; ++i)
            {
                var touch = Input.GetTouch(i);

                //We also see if this touch was used by the UI
                if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    TouchAt(touch);
            }
        }

        private void TouchAt(Touch touch)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
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
            var touch = Input.touches.Where(x => x.fingerId == id).FirstOrDefault();
            return new TouchWrapper(touch);
        }
    }
}