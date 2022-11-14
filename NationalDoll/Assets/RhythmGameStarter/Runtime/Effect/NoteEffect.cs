using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/note-effects")]
    public class NoteEffect : MonoBehaviour
    {
        public bool hasNestedEffect;

        public bool inUse;

        private List<NoteEffect> nestedEffects = new List<NoteEffect>();
        private List<ParticleSystem> particleSys = new List<ParticleSystem>();

        private NoteEffect effectParent;

        void Awake()
        {
            if (hasNestedEffect)
            {
                particleSys.AddRange(GetComponentsInChildren<ParticleSystem>());
                nestedEffects.AddRange(GetComponentsInChildren<NoteEffect>());

                //remove self
                nestedEffects.Remove(this);
            }
            else
            {
                particleSys.Add(GetComponent<ParticleSystem>());
            }

            if (transform.parent)
            {
                effectParent = transform.parent.GetComponent<NoteEffect>();
            }
        }

        public void SeeIfEffectStillAlive()
        {
            if (hasNestedEffect)
            {
                inUse = false;
                foreach (var s in nestedEffects)
                {
                    if (s.inUse)
                    {
                        inUse = true;
                        break;
                    }
                }
            }
        }

        public void OnParticleSystemStopped()
        {
            inUse = false;

            if (!hasNestedEffect && effectParent)
            {
                effectParent.SeeIfEffectStillAlive();
            }
        }

        public void StartEffect()
        {
            StartEffect(null);
        }

        public void StartEffect(Transform target)
        {
            if (target){
                transform.position = target.position;
                transform.rotation = target.rotation;
            }
            foreach (var s in particleSys)
            {
                s.Play();
            }
        }

        public void StopEffect()
        {
            foreach (var s in particleSys)
            {
                s.Stop();
            }
        }
    }
}