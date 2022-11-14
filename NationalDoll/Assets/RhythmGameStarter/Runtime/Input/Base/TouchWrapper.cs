using UnityEngine;

public struct TouchWrapper
{
#if ENABLE_INPUT_SYSTEM
    public TouchWrapper(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        fingerId = touch.touchId;
        position = touch.screenPosition;
        phase = ToBuiltinTouchPhase(touch.phase);
    }

    private static TouchPhase ToBuiltinTouchPhase(UnityEngine.InputSystem.TouchPhase touchPhase)
    {
        switch (touchPhase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                return TouchPhase.Began;
            case UnityEngine.InputSystem.TouchPhase.Moved:
                return TouchPhase.Moved;
            case UnityEngine.InputSystem.TouchPhase.Ended:
                return TouchPhase.Ended;
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                return TouchPhase.Canceled;
            case UnityEngine.InputSystem.TouchPhase.Stationary:
                return TouchPhase.Stationary;
        }

        return TouchPhase.Canceled;
    }
#endif

    public TouchWrapper(Touch touch)
    {
        fingerId = touch.fingerId;
        position = touch.position;
        phase = touch.phase;
    }

    public int fingerId { get; set; }
    public Vector2 position { get; set; }
    public TouchPhase phase { get; set; }
}