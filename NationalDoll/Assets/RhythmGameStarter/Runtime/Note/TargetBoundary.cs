using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    public class TargetBoundary : MonoBehaviour
    {
        private TrackManager trackManager;

        private void Awake()
        {
            trackManager = GetComponentInParent<TrackManager>();
        }

        void OnTriggerExit(Collider col)
        {
            if (col.tag == "Note")
            {
                if (trackManager.useNotePool)
                {
                    trackManager.ResetNoteToPool(col.gameObject);
                }
                else
                {
                    Destroy(col.gameObject);
                }
            }
        }
    }
}