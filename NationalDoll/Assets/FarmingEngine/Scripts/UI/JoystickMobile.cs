using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{

    /// <summary>
    /// Display the mobile joystick
    /// </summary>

    public class JoystickMobile : MonoBehaviour
    {
        public RectTransform pin;

        private CanvasGroup canvas;
        private RectTransform rect;

        void Awake()
        {
            canvas = GetComponent<CanvasGroup>();
            rect = GetComponent<RectTransform>();
            canvas.alpha = 0f;

            if (!TheGame.IsMobile())
                enabled = false;
        }

        void Update()
        {
            PlayerControlsMouse controls = PlayerControlsMouse.Get();

            float target_alpha = controls.IsJoystickActive() && !PlayerUI.GetFirst().IsBuildMode() ? 1f : 0f;
            canvas.alpha = Mathf.MoveTowards(canvas.alpha, target_alpha, 4f * Time.deltaTime);

            Vector2 screenPos = controls.GetJoystickPos();
            rect.anchoredPosition = TheUI.Get().ScreenPointToCanvasPos(screenPos);
            pin.anchoredPosition = controls.GetJoystickDir() * 50f;

        }
    }

}