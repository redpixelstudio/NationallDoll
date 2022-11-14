using UnityEngine;

namespace RhythmGameStarter
{
    public class TriggerState : MonoBehaviour
    {
        public string colliderTag;
        public bool isTriggered, justExit, justEnter;

        private void LateUpdate()
        {
            // Reseting for next frame
            justExit = false;
            justEnter = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(colliderTag))
            {
                isTriggered = true;
                justEnter = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(colliderTag))
            {
                isTriggered = false;
                justExit = true;
            }
        }
    }
}