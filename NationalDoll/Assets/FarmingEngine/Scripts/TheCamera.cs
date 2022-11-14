using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    public enum FreelookMode
    {
        Hold=0,
        Toggle=10,
        Always=20,
        Never=30,
    }

    /// <summary>
    /// Main camera script
    /// </summary>

    public class TheCamera : MonoBehaviour
    {
        [Header("Move/Zoom")]
        public bool move_enabled = true; //Uncheck if you want to use your own camera system
        public float move_speed = 10f;
        public float rotate_speed = 90f;
        public float zoom_speed = 0.5f;
        public float zoom_in_max = 0.5f;
        public float zoom_out_max = 1f;
        public bool inverted_rotate = false; //Rotating the camera controls will be reversed
        public bool smooth_camera = true; //Camera will be more smooth but less accurate

        [Header("Mobile Only")]
        public float rotate_speed_touch = 10f; //Mobile touch
        public float zoom_speed_touch = 1f; //Mobile touch

        [Header("Third Person Only")]
        public FreelookMode freelook_mode;
        public float freelook_speed_x = 0f;
        public float freelook_speed_y = 0f;

        [Header("Target")]
        public GameObject follow_target;
        public Vector3 follow_offset;

        private Vector3 current_vel;
        private Vector3 rotated_offset;
        private Vector3 current_offset;
        private Vector3 custom_offset;
        private float current_rotate = 0f;
        private float current_zoom = 0f;
        private Transform target_transform;
        private bool is_locked;

        private Camera cam;

        private Vector3 shake_vector = Vector3.zero;
        private float shake_timer = 0f;
        private float shake_intensity = 1f;

        private static TheCamera _instance;

        void Awake()
        {
            _instance = this;
            cam = GetComponent<Camera>();
            rotated_offset = follow_offset;
            current_offset = follow_offset;

            GameObject cam_target = new GameObject("CameraTarget");
            target_transform = cam_target.transform;
            target_transform.position = transform.position;
            target_transform.rotation = transform.rotation;
        }

        private void Start()
        {
            PlayerControlsMouse mouse = PlayerControlsMouse.Get();
            mouse.onRightClick += (Vector3 vect) => { ToggleLock(); };
        }

        void LateUpdate()
        {
            if (follow_target == null)
            {
                //Auto assign follow target
                PlayerCharacter first = PlayerCharacter.GetFirst();
                if (first != null)
                    follow_target = first.gameObject;
                return;
            }

            if (!move_enabled)
                return;

            PlayerControls controls = PlayerControls.GetFirst();
            PlayerControlsMouse mouse = PlayerControlsMouse.Get();

            //Rotate
            current_rotate = 0f;
            current_rotate += controls.GetRotateCam() * rotate_speed;
            if (inverted_rotate)
                current_rotate = -current_rotate; //Reverse rotate
            current_rotate += mouse.GetTouchRotate() * rotate_speed_touch;

            //Zoom 
            current_zoom += mouse.GetTouchZoom() * zoom_speed_touch; //Mobile 2 finger zoom
            current_zoom += mouse.GetMouseScroll() * zoom_speed; //Mouse scroll zoom
            current_zoom = Mathf.Clamp(current_zoom, -zoom_out_max, zoom_in_max);

            if (freelook_mode == FreelookMode.Hold)
                SetLockMode(mouse.IsMouseHoldRight());
            if (freelook_mode == FreelookMode.Always)
                SetLockMode(true);
            if (freelook_mode == FreelookMode.Never)
                SetLockMode(false);
            if (controls.IsGamePad())
                Cursor.visible = !is_locked && mouse.IsUsingMouse();

            bool free_rotation = IsFreeRotation();
            if (free_rotation)
                UpdateFreeCamera();
            else
                UpdateCamera();

            //Untoggle if on top of UI
            if (is_locked && TheUI.Get() && TheUI.Get().IsBlockingPanelOpened())
                ToggleLock();

            //Shake FX
            if (shake_timer > 0f)
            {
                shake_timer -= Time.deltaTime;
                shake_vector = new Vector3(Mathf.Cos(shake_timer * Mathf.PI * 8f) * 0.02f, Mathf.Sin(shake_timer * Mathf.PI * 7f) * 0.02f, 0f);
                transform.position += shake_vector * shake_intensity;
            }
        }

        private void UpdateCamera()
        {
            rotated_offset = Quaternion.Euler(0, current_rotate * Time.deltaTime, 0) * rotated_offset;
            target_transform.RotateAround(follow_target.transform.position, Vector3.up, current_rotate * Time.deltaTime);
            current_offset = rotated_offset - rotated_offset * current_zoom + custom_offset;

            Vector3 target_pos = follow_target.transform.position + current_offset;
            target_transform.position = target_pos;

            //Move to target position
            if (smooth_camera)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target_transform.rotation, move_speed * Time.deltaTime);
                transform.position = Vector3.SmoothDamp(transform.position, target_transform.position, ref current_vel, 1f / move_speed);
            }
            else
            {
                transform.rotation = target_transform.rotation;
                transform.position = target_transform.position;
            }
        }

        private void UpdateFreeCamera()
        {
            PlayerControls controls = PlayerControls.GetFirst();
            PlayerControlsMouse mouse = PlayerControlsMouse.Get();
            Vector2 mouse_delta = Vector2.zero;
            if(is_locked)
                mouse_delta += mouse.GetMouseDelta();
            if(controls.IsGamePad())
                mouse_delta += controls.GetFreelook();

            Quaternion target_backup = target_transform.transform.rotation;
            Vector3 rotate_backup = rotated_offset;

            rotated_offset = Quaternion.AngleAxis(freelook_speed_y * -mouse_delta.y * 0.5f * Time.deltaTime, target_transform.right) * rotated_offset;
            rotated_offset = Quaternion.Euler(0f, freelook_speed_x * mouse_delta.x * Time.deltaTime, 0) * rotated_offset;

            target_transform.RotateAround(follow_target.transform.position, target_transform.right, freelook_speed_y * -mouse_delta.y * Time.deltaTime);
            target_transform.RotateAround(follow_target.transform.position, Vector3.up, freelook_speed_x * mouse_delta.x * Time.deltaTime);

            //Lock to not rotate too much
            if (target_transform.transform.up.y < 0.2f)
            {
                target_transform.transform.rotation = target_backup;
                rotated_offset = rotate_backup;
            }

            current_offset = rotated_offset - rotated_offset * current_zoom + custom_offset;
            Vector3 target_pos = follow_target.transform.position + current_offset;
            target_transform.position = target_pos;

            //Move to target position
            if (smooth_camera)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target_transform.rotation, move_speed * Time.deltaTime);
                transform.position = Vector3.SmoothDamp(transform.position, target_transform.position, ref current_vel, 1f / move_speed);
            }
            else
            {
                transform.rotation = target_transform.rotation;
                transform.position = target_transform.position;
            }
        }

        public void SetLockMode(bool locked)
        {
            if (is_locked != locked)
            {
                is_locked = locked;
                Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !locked;
            }
        }

        public void ToggleLock()
        {
            if (freelook_mode == FreelookMode.Toggle)
            {
                SetLockMode(!is_locked);
            }
        }

        public void MoveToTarget(Vector3 target)
        {
            transform.position = target + current_offset;
        }

        public void Shake(float intensity = 2f, float duration = 0.5f)
        {
            shake_intensity = intensity;
            shake_timer = duration;
        }

        public void SetOffset(Vector3 offset)
        {
            custom_offset = offset;
        }

        public Vector3 GetTargetPos()
        {
            return transform.position - current_offset;
        }

        //Use as center for optimization
        public Vector3 GetTargetPosOffsetFace(float dist)
        {
            return transform.position - current_offset + GetFacingFront() * dist;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        public Vector3 GetFacingFront()
        {
            Vector3 dir = transform.forward;
            dir.y = 0f;
            return dir.normalized;
        }

        public Vector3 GetFacingRight()
        {
            Vector3 dir = transform.right;
            dir.y = 0f;
            return dir.normalized;
        }

        //Get camera direction for shooting projectiles, will only work if IsFreeRotation
        public Vector3 GetAimDir(PlayerCharacter character, float distance = 10f)
        {
            Vector3 far = transform.position + transform.forward * distance;
            Vector3 aim = far - character.GetColliderCenter();
            return aim.normalized;
        }

        //Direct aim dir from the camera
        public Vector3 GetFacingDir()
        {
            return transform.forward;
        }

        public Quaternion GetFacingRotation()
        {
            Vector3 facing = GetFacingFront();
            return Quaternion.LookRotation(facing.normalized, Vector3.up);
        }

        public bool IsLocked()
        {
            return is_locked;
        }

        public bool IsFreeRotation()
        {
            PlayerControls controls = PlayerControls.GetFirst();
            return freelook_mode != FreelookMode.Never && (is_locked || controls.IsGamePad());
        }

        public bool IsInside(Vector2 screen_pos)
        {
            return cam.pixelRect.Contains(screen_pos);
        }

        public Camera GetCam()
        {
            return cam;
        }

        public static Camera GetCamera()
        {
            Camera camera = _instance != null ? _instance.GetCam() : Camera.main;
            return camera;
        }

        public static TheCamera Get()
        {
            return _instance;
        }
    }

}