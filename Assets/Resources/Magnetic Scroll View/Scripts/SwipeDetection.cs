//Magnetic Panel System
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace MagneticScrollView
{
    public enum Swipe
    {
        Cancel,
        Up,
        Down,
        Right,
        Left
    }

    /// <summary>
    /// Manages swipe detections and calls registered functions for each event.
    /// </summary>
    [DisallowMultipleComponent]
    public class SwipeDetection : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        #region FIELDS
        /// <summary>
        /// Reference to the RectTransform component of the viewport.
        /// </summary>
        [Tooltip ("Reference to the RectTransform component of the viewport.")]
        public RectTransform viewport;
        /// <summary>
        /// List of swipe events that will be invoked according to the swipe direction.
        /// </summary>
        public List<SwipeEvent> swipeEvents = new List<SwipeEvent> ();
        /// <summary>
        /// Set minimum time for the swipe event to be released.
        /// </summary>
        [Tooltip ("Set minimum time for the swipe event release")]
        [SerializeField] private float minTime = 0.01f;
        /// <summary>
        /// Set maximum time for the swipe event to be released.
        /// </summary>
        [Tooltip ("Set maximum time for the swipe event release")]
        [SerializeField] private float maxTime = 0.5f;

        private Vector2 m_startPos;
        private Vector2 m_endPos;
        private Vector2 m_swipeSpeed;

        private float m_startTime;
        private float m_endTime;
        private float m_swipeTime;

        private bool pointerDown;

        private Swipe swipeDirection;
        #endregion

        #region PROPERTIES
        public float MinTime
        {
            get { return minTime; }
            set { minTime = Mathf.Clamp (value, 0.01f, 1f); }
        }

        public float MaxTime
        {
            get { return maxTime; }
            set { maxTime = Mathf.Clamp (value, 0.01f, 1f); }
        }

        public Vector2 StartPos
        {
            get { return m_startPos; }
        }

        public Vector2 EndPos
        {
            get { return m_endPos; }
        }

        public Vector2 SwipeSpeed
        {
            get { return m_swipeSpeed; }
        }

        public float StartTime
        {
            get { return m_startTime; }
        }

        public float EndTime
        {
            get { return m_endTime; }
        }

        public float SwipeTime
        {
            get { return m_swipeTime; }
        }

        /// <summary>
        /// The direction of the last swipe.
        /// </summary>
        public Swipe SwipeDirection
        {
            get { return swipeDirection; }
        }
        #endregion

        #region METHODS
        private void Start ()
        {
        }

        private void SwipeTo (Vector2 distance)
        {
            if (Mathf.Abs (distance.x) >= Mathf.Abs (distance.y))
            {
                if (distance.x > 0)
                {
                    swipeDirection = Swipe.Right;
                    Execute (Swipe.Right);
                }
                else if (distance.x < 0)
                {
                    swipeDirection = Swipe.Left;
                    Execute (Swipe.Left);
                }
            }
            else
            {
                if (distance.y > 0)
                {
                    swipeDirection = Swipe.Up;
                    Execute (Swipe.Up);
                }
                else if (distance.y < 0)
                {
                    swipeDirection = Swipe.Down;
                    Execute (Swipe.Down);
                }
            }

            //Debug.Log (swipeDirection);
        }

        public virtual void OnBeginDrag (PointerEventData ped)
        {
            if (ped.button != PointerEventData.InputButton.Left || ped.pointerId > 0)
                return;

            if (viewport != null && !RectTransformUtility.RectangleContainsScreenPoint (viewport, ped.position, ped.pressEventCamera))
                return;

            m_startTime = Time.time;
            m_startPos = ped.position;
            pointerDown = true;
        }

        public virtual void OnEndDrag (PointerEventData ped)
        {
            if (ped.button != PointerEventData.InputButton.Left || !pointerDown || ped.pointerId > 0)
                return;

            //Debug.Log ("End Drag");

            m_endTime = Time.time;
            m_endPos = ped.position;
            m_swipeTime = m_endTime - m_startTime;
            pointerDown = false;

            Vector2 distance = m_endPos - m_startPos;

            m_swipeSpeed = distance / m_swipeTime * Time.deltaTime;

            //Debug.Log (m_swipeTime);
            if (m_swipeTime < maxTime && m_swipeTime > minTime)
            {
                SwipeTo (distance);
            }
            else
            {
                swipeDirection = Swipe.Cancel;
                Execute (Swipe.Cancel);
            }
        }

        private void Execute (Swipe id)
        {
            //Debug.Log ("Executing");
            int index = 0;
            int count = swipeEvents.Count;

            SwipeEvent evt;
            UnityEvent cancelEvent = null;
            while (index < count)
            {
                evt = swipeEvents[index];
                if ((evt.eventID == id) && (evt.callback != null))
                {
                    evt.callback.Invoke ();
                    //Debug.Log (id);
                    return;
                }

                if (evt.eventID == Swipe.Cancel)
                    cancelEvent = evt.callback;

                index++;
            }

            if (cancelEvent != null)
            {
                //Debug.Log ("Cancel");
                cancelEvent.Invoke ();
            }
        }

        public void TheGameObject (GameObject go)
        {
            Debug.Log (go, go);
        }
        #endregion

        #region INNER CLASS
        /// <summary>
        /// Swipe event callback with its id.
        /// </summary>
        [Serializable]
        public class SwipeEvent/* : Object*/
        {
            public UnityEvent callback;
            public Swipe eventID;

            public SwipeEvent (Swipe swipe)
            {
                callback = new UnityEvent ();
                eventID = swipe;
            }
        }
        #endregion
    }

}