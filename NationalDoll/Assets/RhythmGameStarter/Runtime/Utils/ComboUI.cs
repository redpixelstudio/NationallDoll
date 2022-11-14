using UnityEngine;

namespace RhythmGameStarter
{
    public class ComboUI : MonoBehaviour
    {
        public string[] animatorParams;

        private Animator anim;

        void Start()
        {
            anim = GetComponent<Animator>();
        }

        public void OnComboAdd()
        {
            if (anim == null)
            {
                anim = GetComponent<Animator>();
            }
            anim.SetTrigger(animatorParams[0]);
        }

        public void OnVisibilityChanged(bool show)
        {
            anim.SetBool(animatorParams[1], show);
        }
    }
}