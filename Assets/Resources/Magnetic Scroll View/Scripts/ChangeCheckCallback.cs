using UnityEngine;
using System.Reflection;

namespace MagneticScrollView
{
    /// <summary>
    /// MonoBehaviour class used to check whether the object dimensions has been changed.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    internal class ChangeCheckCallback : MonoBehaviour
    {
        //[HideInInspector]
        private MagneticScrollRect magneticScrollView;
        private MethodInfo AssignElements;

        void OnEnable ()
        {
            magneticScrollView = GetComponentInParent<MagneticScrollRect> ();
            AssignElements = typeof (MagneticScrollRect).GetMethod ("AssignElements",
                BindingFlags.NonPublic |
                BindingFlags.Instance);
            //hideFlags = HideFlags.HideInInspector;
            //hideFlags = HideFlags.None;
        }

        private void OnRectTransformDimensionsChange ()
        {
            //Debug.Log ("Dimension Changed", gameObject);

            if (gameObject.activeInHierarchy && enabled && magneticScrollView != null)
            {
                AssignElements.Invoke (magneticScrollView, null);
                magneticScrollView.ArrangeElements ();
            }
        }
    }
}
