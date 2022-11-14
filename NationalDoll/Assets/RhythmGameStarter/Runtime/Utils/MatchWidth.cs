using UnityEngine;

namespace RhythmGameStarter
{
    [ExecuteInEditMode]
    public class MatchWidth : MonoBehaviour
    {
        [Comment("When using perspective camera, this component can handles the camera's fov and make sure all the track is being shown on screen")]
        public float horizontalFoV = 90.0f;

        [Comment("When using orthographic camera, this component can handles the camera's orthographicSize and make sure all the track is being shown on screen")]
        public float orthographicSize = 2.0f;

        public Camera _camera;

        private void Update()
        {
            if (!_camera) return;

            if (_camera.orthographic)
            {
                float halfWidth = Mathf.Tan(0.5f * orthographicSize * Mathf.Deg2Rad);
                float halfHeight = halfWidth * Screen.height / Screen.width;
                float size = 2.0f * Mathf.Atan(halfHeight) * Mathf.Rad2Deg;
                _camera.orthographicSize = size;
            }
            else
            {
                float halfWidth = Mathf.Tan(0.5f * horizontalFoV * Mathf.Deg2Rad);
                float halfHeight = halfWidth * Screen.height / Screen.width;
                float verticalFoV = 2.0f * Mathf.Atan(halfHeight) * Mathf.Rad2Deg;
                _camera.fieldOfView = verticalFoV;
            }

        }
    }
}