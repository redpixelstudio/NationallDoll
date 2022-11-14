using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace RhythmGameStarter
{
    public class CountDown : MonoBehaviour
    {
        public int seconds = 3;

        [CollapsedEvent]
        public StringEvent onCountDown;

        [CollapsedEvent]
        public UnityEvent onCountDownFinshed;

        public void StartCountDown()
        {
            StopCoroutine("CountDown");
            StartCoroutine(CountDownCoroutine());
        }

        private IEnumerator CountDownCoroutine()
        {
            // onCountDown.Invoke(seconds);
            for (int i = seconds - 1; i >= 0; i--)
            {
                onCountDown.Invoke((i + 1).ToString());
                yield return new WaitForSeconds(1);
            }
            onCountDownFinshed.Invoke();
        }
    }
}