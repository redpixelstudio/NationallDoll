using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    public class ResetPosition : MonoBehaviour
    {
        void Awake()
        {
            transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}