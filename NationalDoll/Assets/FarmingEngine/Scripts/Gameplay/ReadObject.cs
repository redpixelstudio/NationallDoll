using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FarmingEngine
{
    [RequireComponent(typeof(Selectable))]
    public class ReadObject : MonoBehaviour
    {
        public string title;

        [TextArea(3, 4)]
        public string text;

        void Start()
        {

        }

    }

}
