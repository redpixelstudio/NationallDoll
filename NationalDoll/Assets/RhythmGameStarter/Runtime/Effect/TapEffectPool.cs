using System.Collections.Generic;
using UnityEngine;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/note-effects")]
    public class TapEffectPool : MonoBehaviour
    {
        [Comment("Simple effect pool for tap effects, used by NoteArea")]
        public int poolSize;
        public GameObject effects;

        private List<NoteEffect> effectsPools = new List<NoteEffect>();

        private Transform effectsParent;

        void Awake()
        {
            effectsParent = new GameObject("Effects").transform;

            effectsParent.SetParent(transform);
            effectsParent.transform.localScale = Vector3.one;

            effectsParent.position = transform.position;

            for (int i = 0; i < poolSize; i++)
            {
                GetNewEffect();
            }
        }

        private NoteEffect GetNewEffect()
        {
            var g = Instantiate(effects);

            var ordinalScale = effectsParent.transform.localScale;
            g.transform.SetParent(effectsParent);

            g.transform.position = effectsParent.position;
            g.transform.localScale = ordinalScale;

            var effect = g.GetComponent<NoteEffect>();

            effectsPools.Add(effect);

            return effect;
        }

        private NoteEffect GetUnUsedEffect()
        {
            var effect = effectsPools.Find(x => !x.inUse);

            if (effect == null)
            {
                effect = GetNewEffect();
            }

            effect.inUse = true;
            return effect;
        }

        public NoteEffect EmitEffects(Transform target)
        {
            var effect = GetUnUsedEffect();
            effect.StartEffect(target);
            return effect;
        }
    }
}