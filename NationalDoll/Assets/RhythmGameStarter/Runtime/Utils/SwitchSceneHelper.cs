using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace RhythmGameStarter
{
    public class SwitchSceneHelper : MonoBehaviour
    {
        public float delay;
        public bool triggerOnce;

        private bool triggered;

        private Coroutine coroutine;

        public void SwitchScene(string sceneName)
        {
            if (triggered && triggerOnce) return;
            triggered = true;

            if (delay > 0)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                    
                coroutine = StartCoroutine(SwitchSceneCoroutine(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }

        }

        IEnumerator SwitchSceneCoroutine(string sceneName)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneName);
        }
    }
}