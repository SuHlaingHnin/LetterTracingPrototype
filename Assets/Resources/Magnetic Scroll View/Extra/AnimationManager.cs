using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagneticScrollView
{
    [DisallowMultipleComponent]
    public class AnimationManager : MonoBehaviour
    {

        public string selectedBool;

        private Animator lastSelection;

        public void Start ()
        {

        }

        public void TriggerAnimation (GameObject gameObject)
        {
            Animator objAnimator = null;
            if (gameObject != null)
                objAnimator = gameObject.GetComponent<Animator> ();

            if (lastSelection != null && objAnimator != lastSelection)
                lastSelection.SetBool (selectedBool, false);

            if (objAnimator != null)
                objAnimator.SetBool (selectedBool, true);

            lastSelection = objAnimator;
        }

        public void TriggerAnimation (int index)
        {

        }
    }
}