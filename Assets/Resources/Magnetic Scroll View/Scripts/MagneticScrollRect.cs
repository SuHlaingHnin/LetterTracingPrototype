using UnityEngine;
//using System;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEditor;
//using UnityEditor.Experimental.SceneManagement;
#endif

namespace MagneticScrollView
{
    #region ENUM TYPES

    public enum SnapMode
    {
        Swipe,
        SnapToNearest,
        Both,
        None
    }

    public enum ResizeMode
    {
        PresetSize,
        FitToViewport,
        Free
    }

    public enum LayoutMode
    {
        Circular,
        Linear
    }

    //[Serializable]
    public enum Alignment
    {
        Horizontal,
        Vertical
    }

    public enum Curvature
    {
        Convex = -1,
        Concave = 1
    }

    #endregion

    /// <summary>
    /// The class responsible for organizing and controlling the content.
    /// </summary>

#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    [AddComponentMenu("UI/Magnetic Scroll Rect", 37)]
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class MagneticScrollRect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region FIELDS

        //PUBLIC VARIABLES

        /// <summary>
        /// Reference to the RectTransform component of the viewport.
        /// </summary>
        [Tooltip("Reference to the RectTransform component of the viewport.")]
        public RectTransform viewport;
        /// <summary>
        /// When Inertia is set the content will continue to move when the pointer is released after a drag.
        /// </summary>
        [Tooltip("When Inertia is set the content will continue to move when the pointer is released after a drag.")]
        public bool inertia;
        ///// <summary>
        ///// Smooth transion of elements from a point to another on snapping.
        ///// </summary>
        //[Tooltip ("Smooth transion of elements from a point to another on snapping.")]
        //public bool smoothTransition;

        public AnimationCurve transitionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        /// <summary>
        /// A UnityEvent that is invoked when the curren selection changes. The event can send the current selection as a GameObject type dynamic argument.
        /// </summary>
        [Tooltip("A UnityEvent that is invoked when the current selection changes. The event can send the current selection as a GameObject type dynamic argument.")]
        public SelectionChangeEvent onSelectionChange;
        [Tooltip("A UnityEvent that is invoked on the scrolling coroutine.")]
        public UnityEngine.Events.UnityEvent onScrolling;

        /// <summary>
        /// Update the CurrentIndex in realtime when Inertia is toggled on.
        /// </summary>
        [Tooltip("Update the current selected index in real-time when Inertia is toggled on.")]
        public bool realtimeSelection;

        /// <summary>
        /// This will delay the OnSelectionChange event to the end of scrolling.
        /// </summary>
        [Tooltip("This will delay the OnSelectionChange event to the end of scrolling.")]
        [SerializeField, HideInInspector] public bool onSelectionChangeDelay;

        //PRIVATE SERIALIZED        

        [SerializeField] private RectTransform[] elements = new RectTransform[0];
        [SerializeField] private IndexTableManager indexTableManager;
        //        [SerializeField] private Vector2 [] elementPivotCancel;
        //        [SerializeField] private Vector2 viewportPivotCancel;

        [SerializeField] private SwipeDetection m_swipeDetect;
        [SerializeField] private bool m_autoArranging = false;
        [Tooltip("Inverts positioning order of elements.")]
        [SerializeField] private bool m_invertOrder = false;
        [Tooltip("Allows to scroll infinitely, going from first to last element and vice versa.")]
        [SerializeField] private bool m_infiniteScrolling = false;
        [Tooltip("Creates a margin between the elements and viewport.")]
        [SerializeField] private bool m_useMargin = true;
        [Tooltip("Allows the Circular Factor to rotate elements according to its angular position. This will centralize the pivot automatically")]
        [SerializeField] private bool m_rotate = true;
        [Tooltip("Set the element size manually if Resize Mode is set to 'Preset Size'.")]
        [SerializeField] private Vector2 m_elementsSize = new Vector2(100, 100);
        [Tooltip("Changes the Snap Mode (Swipe = 0 | Snap To Nearest = 1 | Both = 2 | None = 3).")]
        [SerializeField] private SnapMode m_snapMode = SnapMode.Swipe;
        [Tooltip("Changes the Resize Mode (Preset Size = 0 | Fit To Viewport = 1 | Free = 2).")]
        [SerializeField] private ResizeMode m_resizeMode = ResizeMode.PresetSize;
        [Tooltip("Elements alignment ordering (Horizontal or Vertical).")]
        [SerializeField] private Alignment m_alignment = Alignment.Horizontal;
        [Tooltip("Changes Layout Mode (Circular or Linear).")]
        [SerializeField] private LayoutMode m_layoutMode = LayoutMode.Circular;
        [Tooltip("Changes the curvature type (Concave or Convex)")]
        [SerializeField] private Curvature m_curvature = Curvature.Concave;
        //		[Tooltip("This will create a alignment offset")]
        //		[SerializeField, Range (-1f, 1f)] private float globalOffset = 0f;
        [Tooltip("The space between elements.")]
        [SerializeField, Range(0f, 1000f)] private float m_elementPadding = 0f;
        [Tooltip("Increases the limits of scrolling when Infinite Scrolling is toggled off.")]
        [SerializeField, Range(0f, 45f)] private float m_scrollAdditionalLimits = 0f;
        [Tooltip("Float value between 0 - 1 that defines how circular the elements will be positioned.")]
        [SerializeField, Range(0f, 1f)] private float m_circularFactor = 0f;
        [Tooltip("Defines how distant from center the elements will be positioned.")]
        [SerializeField, Range(0f, 1f)] private float m_distanceOffset = 0f;
        [Tooltip("When Inertia is set the deceleration rate determines how quickly the contents stop moving. A rate of 0 will stop the movement immediately. A value of 1 means the movement will never slow down.")]
        [SerializeField, Range(0f, 1f)] private float m_decelerationRate = 0.15f;
        [Tooltip("The angular speed in which the elements move from one point to another.")]
        [SerializeField, Range(0.1f, 10f)] private float m_transitionSpeed = 5f;
        [Tooltip("Slows down the content dragging.")]
        [SerializeField, Range(0f, 1f)] private float m_dragDelay = 0.5f;

#if UNITY_EDITOR
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject forwardButton;
        [SerializeField] private GameObject indexTable;
        //[Tooltip ("Use buttons to scroll content.")]
        //[SerializeField] private bool useButtons = false;
        //[Tooltip ("Use Index Table to indicate the index position.")]
        //[SerializeField] private bool useIndexTable = false;
#endif
        //PRIVATE NON-SERIALIZED

        private float radius = 0;
        private float m_scrollAngle;
        //		private float globalOffsetAngle;
        private float[] elementAngleOffset;

        private float startTime;
        private float endTime;
        private float dragTime;

        public int m_currentSelected;
        //private int targetIndex;

        private Coroutine inertiaCoroutine;
        private Coroutine scrollCoroutine;

        private Vector2 startLocalCursor = Vector2.zero;
        private Vector2 lastLocalCursor;
        private Vector2 dragSpeed;

        private bool isScrolling;
        private bool isArranging;
        private bool pointerDown;
        private bool routeToParent;

        private CanvasGroup viewportCG;
        #endregion

        #region CONSTANTS
        private const float minDragSpeed = 20f;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Reference to the SwipeDetection Component.
        /// </summary>
        public SwipeDetection SwipeDetect
        {
            get
            {
                if (m_swipeDetect == null)
                    m_swipeDetect = gameObject.GetComponent<SwipeDetection>();

                return m_swipeDetect;
            }
        }

        /// <summary>
        /// Returns the array of active elements.
        /// </summary>
        public RectTransform[] Elements
        {
            get { return elements; }
        }

        /// <summary>
        /// Set the element size manually if Resize Mode is set to 'Preset Size'
        /// </summary>
        public Vector2 ElementsSize
        {
            get { return m_elementsSize; }
            set
            {
                m_elementsSize = value;
                ArrangeElements();
            }
        }

        /// <summary>
        /// Trys to change elements width by parsing a string value.
        /// </summary>
        public string ElementsWidth
        {
            get { return m_elementsSize.x.ToString(); }
            set
            {
                float x;
                bool validValue = float.TryParse(value, out x);
                //Debug.Log (x.GetType () + " " + x);
                ElementsSize = validValue ? new Vector2(x, m_elementsSize.y) : m_elementsSize;
            }
        }

        /// <summary>
        /// Trys to change elements height by parsing a string value.
        /// </summary>
        public string ElementsHeight
        {
            get { return m_elementsSize.y.ToString(); }
            set
            {
                float y;
                bool validValue = float.TryParse(value, out y);
                //Debug.Log (y.GetType() + " " + y);
                ElementsSize = validValue ? new Vector2(m_elementsSize.x, y) : m_elementsSize;
            }
        }

        /// <summary>
        /// Float value between 0 - 1 that defines how circular the elements will be positioned.
        /// </summary>
        public float CircularFactor
        {
            get { return m_circularFactor; }
            set
            {
                m_circularFactor = Mathf.Clamp01(value);
                AlignElements();
            }
        }

        /// <summary>
        /// Defines how distant from center the elements will be positioned.
        /// </summary>
        public float DistanceOffset
        {
            get { return m_distanceOffset; }
            set
            {
                m_distanceOffset = Mathf.Clamp01(value);
                AlignElements();
            }
        }

        /// <summary>
        /// When Inertia is set the deceleration rate determines how quickly the contents stop moving. A rate of 0 will stop the movement immediately. A value of 1 means the movement will never slow down.
        /// </summary>
        public float DecelerationRate
        {
            get { return m_decelerationRate; }
            set { m_decelerationRate = Mathf.Clamp(value, 0.01f, 0.9f); }
        }

        /// <summary>
        /// The angular speed in which the elements move from one point to another.
        /// </summary>
        public float TransitionSpeed
        {
            get { return 180f * m_transitionSpeed / elements.Length * Time.unscaledDeltaTime; }
            set { m_transitionSpeed = Mathf.Clamp(value, 0.1f, 10f); }
        }

        /// <summary>
        /// Slows down the content dragging.
        /// </summary>
        public float DragDelay
        {
            get { return m_dragDelay; }
            set { m_dragDelay = Mathf.Clamp(value, 0.1f, 1f); }
        }

        /// <summary>
        /// Allows to scroll infinitely, going from first to last element and vice versa.
        /// </summary>
        public bool InfiniteScrolling
        {
            get
            {
                if (m_layoutMode == LayoutMode.Linear)
                    return false;
                else
                    return m_infiniteScrolling;
            }

            set
            {
                if (m_layoutMode == LayoutMode.Linear)
                {
                    Debug.LogWarning("Infinite Scrolling only works with Circular Layout Mode!");
                    m_infiniteScrolling = false;
                }
                else
                    m_infiniteScrolling = value;

                ResetScroll();
            }
        }

        /// <summary>
        /// Inverts positioning order of elements.
        /// </summary>
        public bool InvertOrder
        {
            get { return m_invertOrder; }
            set
            {
                m_invertOrder = value;
                AlignElements();
                //ResetScroll ();
            }
        }

        /// <summary>
        /// Creates a margin between the elements and viewport.
        /// </summary>
        public bool UseMargin
        {
            get { return m_useMargin; }
            set
            {
                m_useMargin = value;
                AlignElements();
            }
        }

        /// <summary>
        /// Allows the Circular Factor to rotate elements according to its angular position. This will centralize the pivot automatically.
        /// </summary>
        public bool Rotate
        {
            get { return m_rotate; }
            set
            {
                m_rotate = value;
                AlignElements();
            }
        }

        /// <summary>
        /// The space between elements.
        /// </summary>
        public float ElementPadding
        {
            get { return Mathf.Clamp(m_elementPadding, 0f, 1000f); }
            set
            {
                m_elementPadding = Mathf.Clamp(value, 0f, 1000f);
                AlignElements();
            }
        }

        /// <summary>
        /// Increases the limits of scrolling when Infinite Scrolling is toggled off.
        /// </summary>
        public float ScrollAdditionalLimits
        {
            get { return Mathf.Clamp(m_scrollAdditionalLimits, 0f, 45f); }
            set { m_scrollAdditionalLimits = Mathf.Clamp(value, 0f, 45f); }
        }

        /// <summary>
        /// Elements alignment ordering (Horizontal or Vertical).
        /// </summary>      
        public Alignment AlignmentEnum
        {
            get { return m_alignment; }
            set
            {
                m_alignment = value;
                AlignElements();
                SetupSwipeDetection();
            }
        }

        /// <summary>
        /// Elements alignment ordering (Horizontal or Vertical).
        /// </summary> 
        public int AlignmentInt
        {
            get { return (int)m_alignment; }
            set { AlignmentEnum = (Alignment)value; }
        }

        /// <summary>
        /// Change Layout Mode (Circular or Linear).
        /// </summary>
        public LayoutMode LayoutModeEnum
        {
            get { return m_layoutMode; }
            set
            {
                m_layoutMode = value;
                if (m_layoutMode == LayoutMode.Linear)
                    m_infiniteScrolling = false;
                ResetScroll();
                AlignElements();
            }
        }

        /// <summary>
        /// Change Layout Mode (Circular or Linear).
        /// </summary>
        public int LayoutModeInt
        {
            get { return (int)m_layoutMode; }
            set { LayoutModeEnum = (LayoutMode)value; }
        }

        /// <summary>
        /// Changes the curvature type (Concave or Convex)
        /// </summary>
        public Curvature CurvatureEnum
        {
            get { return m_curvature; }
            set
            {
                m_curvature = value;
                ResetScroll();
                AlignElements();
            }
        }

        /// <summary>
        /// Changes the curvature type (Concave or Convex)
        /// </summary>
        public int CurvatureInt
        {
            get { return (int)m_curvature; }
            set { CurvatureEnum = (Curvature)value; }
        }

        /// <summary>
        /// Changes the Resize Mode (Preset Size = 0 | Fit To Viewport = 1 | Free = 2).
        /// </summary>
        public ResizeMode ResizeModeEnum
        {
            get { return m_resizeMode; }
            set
            {
                m_resizeMode = value;
                if (value != ResizeMode.FitToViewport)
                    ResetAnchors();
                ArrangeElements();
                ResetScroll();
            }
        }

        /// <summary>
        /// Changes the Resize Mode (Preset Size = 0 | Fit To Viewport = 1 | Free = 2).
        /// </summary>
        public int ResizeModeInt
        {
            get { return (int)m_resizeMode; }
            set { ResizeModeEnum = (ResizeMode)value; }
        }

        /// <summary>
        /// Changes the Snap Mode (Swipe = 0 | Snap To Nearest = 1 | Both = 2 | None = 3).
        /// </summary>
        public SnapMode SnapModeEnum
        {
            get { return m_snapMode; }
            set
            {
                m_snapMode = value;
                SetupSwipeDetection();
            }
        }

        /// <summary>
        /// Changes the Snap Mode (Swipe = 0 | Snap To Nearest = 1 | Both = 2 | None = 3).
        /// </summary>
        public int SnapModeInt
        {
            get { return (int)m_snapMode; }
            set { SnapModeEnum = (SnapMode)value; }
        }

        /// <summary>
        /// Returns the current selection, the element snapped to center.
        /// </summary>
        public int CurrentSelectedIndex
        {
            get { return (int)Mathf.Repeat(m_currentSelected, elements.Length); }
            private set { m_currentSelected = (int)Mathf.Repeat(value, elements.Length); }
        }

        public GameObject CurrentSelectedObject
        {
            get { return elements[m_currentSelected].gameObject; }
        }

        public float ScrollAngle
        {
            get { return m_scrollAngle; }
            private set
            {
                m_scrollAngle = float.IsNaN(value) ? m_scrollAngle : value; /*% 360f*/;
            }
        }

        public float NormalizedScrollAngle
        {
            get { return ScrollAngle / 180f; }
        }

        private bool AutoArranging
        {
            get
            { return m_autoArranging; }

            set
            {
                m_autoArranging = value;
                if (value == m_autoArranging)
                    return;
                else if (value == true)
                    StartAutoArranging();
                else
                    StopAutoArranging();
            }
        }

        private CanvasGroup ViewportCG
        {
            get
            {
                if (viewport != null)
                {
                    if (viewportCG == null)
                        viewportCG = viewport.GetComponent<CanvasGroup>();

                    if (viewportCG == null)
                        viewportCG = viewport.gameObject.AddComponent<CanvasGroup>();
                }

                return viewportCG;
            }
        }
        #endregion

        #region Methods
        void Awake()
        {
            //Debug.Log ("Awaken");           

            if (gameObject.GetComponent<SwipeDetection>() != null)
                m_swipeDetect = gameObject.GetComponent<SwipeDetection>();
            else if (m_snapMode == SnapMode.Swipe)
                m_swipeDetect = gameObject.AddComponent<SwipeDetection>();

            SetupStartingSettings();
            SetupSwipeDetection();


#if UNITY_EDITOR
            if (!Application.isPlaying && m_autoArranging)
                StartAutoArranging();
            else
                StopAutoArranging();
#else            
            if (m_autoArranging)
                StopAutoArranging ();
#endif
        }

        private void Update()
        {
            // Debug.Log(GetNormalizedElementAngle(0, true));
            if (!isScrolling)
            {
                if (SwipeDetect == null)
                {
                    if (m_snapMode.IsEqual(SnapMode.Swipe, SnapMode.Both))
                        SetupSwipeDetection();
                }
                else
                {
                    //bool boolean = m_snapMode.IsDifferent (SnapMode.Swipe, SnapMode.Both);
                    if (m_snapMode.IsDifferent(SnapMode.Swipe, SnapMode.Both))
                    {
                        Debug.LogWarning("Cannot use SwipeDetection while not in Swipe mode");
                        if (Application.isPlaying) Destroy(m_swipeDetect); else DestroyImmediate(m_swipeDetect);
                    }
                }
            }

            if (viewport != null)
            {
                int count = ChildCount(viewport);

                if (count != elements.Length)
                {
                    AssignElements();
                }

                if (m_autoArranging)
                {
                    for (int i = 0; i < elements.Length; i++)
                        if (elements[i].hasChanged)
                        {
                            ArrangeElements(false);
                            elements[i].hasChanged = false;
                        }

                    if (viewport.hasChanged)
                    {
                        ArrangeElements(false);
                        viewport.hasChanged = false;
                    }
                }
            }



            //InputCheck ();
        }


        private void OnEnable()
        {
            //Debug.Log ("Enabled");
#if UNITY_EDITOR
            if (!Application.isPlaying)
                SetupStartingSettings();

            ResetScroll();
#endif
            m_currentSelected = 0;

            if (onSelectionChange != null && elements.Length > 0)
                onSelectionChange.Invoke(elements[m_currentSelected].gameObject);

            if (indexTableManager == null)
                indexTableManager = GetComponentInChildren<IndexTableManager>();


            //StartCoroutine (RemoveDuplicatedIndexTable ());

            //if (indexTableManager != null)
            //    indexTableManager.RemovePrefabs ();
        }

        public GameObject GetSelected(out int index)
        {
            index = (int)Mathf.Repeat(m_currentSelected, elements.Length);
            return elements[index].gameObject;
        }

        //private void OnTransformChildrenChanged ()
        //{
        //    StartCoroutine (RemoveDuplicatedIndexTable ());
        //}

        //private IEnumerator RemoveDuplicatedIndexTable ()
        //{
        //    yield return new WaitForEndOfFrame ();
        //    Component [] indexTableList = GetComponentsInChildren (typeof (IndexTableManager), true);
        //    //Debug.Log ("Destroying duplicateds");
        //    //Debug.Log ("Destroying duplicateds, " + indexTableList.Length);
        //    if (indexTableList.Length > 1)
        //    {
        //        foreach (IndexTableManager indexTableMNG in indexTableList)
        //        {
        //            if (PrefabUtility.IsPartOfPrefabInstance (indexTableMNG.gameObject))
        //            {
        //                //Debug.Log ("Prefab");
        //                indexTableManager = indexTableMNG;
        //                indexTable = indexTableMNG.gameObject;
        //            }
        //            else
        //            {
        //                //Debug.Log ("Not a prefab, " + indexTableMNG.gameObject.hideFlags);
        //                if (!Application.isPlaying)
        //                    DestroyImmediate (indexTableMNG.gameObject);


        //            }
        //        }

        //        //var stage = PrefabStageUtility.GetPrefabStage (gameObject);
        //        //if (stage != null)
        //        //{
        //        //Debug.Log (AssetDatabase.LoadAssetAtPath (stage.prefabAssetPath, typeof (GameObject)), AssetDatabase.LoadAssetAtPath (stage.prefabAssetPath, typeof (GameObject)));
        //        //string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot (gameObject);
        //        //Debug.Log (AssetDatabase.LoadAssetAtPath<GameObject> (stage.prefabAssetPath), AssetDatabase.LoadAssetAtPath<GameObject> (stage.prefabAssetPath));
        //        //PrefabUtility.SaveAsPrefabAsset (gameObject, assetPath);
        //        //}
        //    }



        //    //if (PrefabUtility.IsPartOfPrefabAsset (gameObject))
        //}


        private void SetupStartingSettings()
        {
            //Debug.Log ("SetupStartingSettings");
            if (viewport != null && ChildCount(viewport) > 0)
            {
                m_currentSelected = 0;
                isArranging = false;
                isScrolling = false;

                elements = new RectTransform[ChildCount(viewport)];
                //                elementPivotCancel = new Vector2 [elements.Length];
                //                elementAngleOffset = new float [elements.Length];

                AssignElements();

                ViewportCG.blocksRaycasts = true;
                ViewportCG.hideFlags = HideFlags.None;

                if (indexTableManager)
                    indexTableManager.MoveIndicator(m_currentSelected, m_invertOrder);
            }
        }

        private int ChildCount(Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
                if (parent.GetChild(i).gameObject.activeSelf)
                    count++;
            return count;
        }

        private void AssignElements()
        {
            //Debug.Log ("Assigning Elements");
            if (viewport != null)
            {
                int count = ChildCount(viewport);
                if (elements == null || elements.Length != count)
                    elements = new RectTransform[count];

                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i] = viewport.transform.GetChild(i).GetComponent<RectTransform>();
                }
            }

            //Debug.Log (viewport.childCount + ", " + elements.Length);

            ArrangeElements();
            ResetScroll();
        }

        /// <summary>
        /// This will re-organize the elements.
        /// </summary>
        /// <param name="resizeElements">Whether the elements should be resized or not, true by default.</param>
        public void ArrangeElements(bool resizeElements = true)
        {
            if (isArranging == false)
            {
                // Debug.Log ("Arranging Elements");
                isArranging = true;

                if (resizeElements == true)
                    ResizeElements();
                AlignElements();

                isArranging = false;
            }
        }

        private void ResizeElements()
        {
            //Debug.Log ("Resizing Elements");
            if (m_resizeMode == ResizeMode.PresetSize)
                SetElementsSize();
            else if (m_resizeMode == ResizeMode.FitToViewport)
                FitElementsToViewport();
        }

        private void SetElementsSize()
        {
            //Debug.Log ("Set Elements Size");
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_elementsSize.x);
                elements[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_elementsSize.y);
            }
        }

        private void ResetAnchors()
        {
            //Debug.Log ("Set Elements Size");
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].sizeDelta = new Vector2(elements[i].rect.width, elements[i].rect.height);
                elements[i].anchorMin = Vector2.one / 2;
                elements[i].anchorMax = Vector2.one / 2;

            }
        }

        private void FitElementsToViewport()
        {
            //Debug.Log ("Fit Elements To Viewport");
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].anchorMin = Vector2.zero;
                elements[i].anchorMax = Vector2.one;
                elements[i].sizeDelta = Vector2.zero;
                elements[i].anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// Starts the auto arranging mode. This will detect any change made to elements or to the viewport, forcing it to re-organize all elements. Some settings changes may cause the auto-arranging.
        /// </summary>
        public void StartAutoArranging()
        {
            //Debug.Log ("Auto Arranging Has Started");
            m_autoArranging = true;
            ArrangeElements();

            for (int i = 0; i < elements.Length + 1; i++)
            {
                GameObject gameObject = null;
                if (i < elements.Length)
                    gameObject = elements[i].gameObject;
                else if (viewport != null)
                    gameObject = viewport.gameObject;

                if (gameObject != null)
                {
                    //Debug.Log ("Adding ChangeCheck");
                    ChangeCheckCallback changeCheck = gameObject.GetComponent<ChangeCheckCallback>();
                    if (changeCheck == null)
                        changeCheck = gameObject.AddComponent<ChangeCheckCallback>();
                    else if (!changeCheck.enabled)
                        changeCheck.enabled = true;
                }
            }
        }

        /// <summary>
        /// Stops the auto arranging mode. This will prevent the elements of being automatically re-organized, even if elements, viewport or settings has been changed.
        /// </summary>
        public void StopAutoArranging()
        {
            //Debug.Log ("Auto Arranging Has Stopped");
            m_autoArranging = false;

            for (int i = 0; i < elements.Length; i++)
            {
                //Debug.Log ("Adding ChangeCheck");            
                ChangeCheckCallback changeCheck = elements[i].gameObject.GetComponent<ChangeCheckCallback>();
                if (changeCheck != null && changeCheck.enabled)
                    changeCheck.enabled = false;
            }
        }

        private void AlignElements()
        {
            //Debug.Log ("Aligning Elements");
            if (viewport != null)
            {
                //Undo.RegisterCompleteObjectUndo (this, "Object changed");
                float elementSize, nextElementSize, viewportSize, /*viewportRadius,*/ halfPerimeter = 0f;
                float[] position = new float[elements.Length];

                //                elementPivotCancel = new Vector2 [elements.Length];

                //                for (int i = 0; i < elementPivotCancel.Length; i++)
                //                {
                //                    elementPivotCancel [i] = new Vector2 (
                //                    elements [i].rect.width * Mathf.Lerp (-0.5f, 0.5f, elements [i].pivot.x),
                //                    elements [i].rect.height * Mathf.Lerp (-0.5f, 0.5f, elements [i].pivot.y)
                //                    );
                //                }
                //
                //                if (viewport != null)
                //                {
                //                    viewportPivotCancel = new Vector2 (
                //                            viewport.rect.width * Mathf.Lerp (0.5f, -0.5f, viewport.pivot.x),
                //                            viewport.rect.height * Mathf.Lerp (0.5f, -0.5f, viewport.pivot.y)
                //                            );
                //                }

                for (int i = 0; i < elements.Length; i++)
                {
                    if (m_layoutMode == LayoutMode.Circular && m_rotate)
                        elements[i].pivot = Vector2.one / 2f;

                    position[i] = halfPerimeter;
                    if (m_alignment == Alignment.Horizontal)
                    {
                        elementSize = elements[i].rect.width;
                        nextElementSize = elements[(i + 1) % elements.Length].rect.width;
                        viewportSize = viewport.rect.width;
                    }
                    else
                    {
                        elementSize = elements[i].rect.height;
                        nextElementSize = elements[(i + 1) % elements.Length].rect.height;
                        viewportSize = viewport.rect.height;
                    }

                    halfPerimeter += elementSize / 2f + nextElementSize / 2f + m_elementPadding;
                    if (m_useMargin)
                    {
                        if (m_resizeMode == ResizeMode.PresetSize)
                            halfPerimeter += (viewportSize - elementSize) / 2f;
                        else if (m_resizeMode == ResizeMode.Free)
                            halfPerimeter += viewportSize / 2f;
                    }
                }

                radius = halfPerimeter / Mathf.PI;
                elementAngleOffset = new float[elements.Length + 1];

                //				viewportRadius = (m_alignment == Alignment.Horizontal)?	viewport.rect.width / 2f : viewport.rect.height / 2f;
                //
                //				float adjacentLeg = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(radius, 2) - Mathf.Pow(viewportRadius, 2)));
                //				globalOffsetAngle = Mathf.Lerp(
                //					globalOffset * ProportionalAngle (halfPerimeter, 180f, viewportRadius),
                //					globalOffset * Vector3.Angle (new Vector3 (viewportRadius, 0f, adjacentLeg), radius * Vector3.forward),
                //					m_circularFactor
                //				);

                for (int i = 0; i < elements.Length; i++)
                {
                    elementAngleOffset[i] = (position[i] / radius) * Mathf.Rad2Deg;
                    CircularPosition(elements, radius, i);
                    elements[i].hasChanged = false;
                }
                elementAngleOffset[elements.Length] = 180f;
            }
        }

        private float ProportionalAngle(float fullLength, float equivalentAngle, float targetLength)
        {
            float targetAngle = targetLength * equivalentAngle / fullLength;
            return targetAngle;
        }

        /// <summary>
        /// Scrolls the content backwards.
        /// </summary>
        public void ScrollBack()
        {
            //Debug.Log ("ScrollBack");

            float speed = (m_alignment == Alignment.Horizontal) ? Mathf.Abs(dragSpeed.x) : Mathf.Abs(dragSpeed.y);
            //Debug.Log (speed);

            if (inertia && speed > minDragSpeed && dragTime < 0.05f)
            {
                StartInertia();
            }
            else if (!isScrolling /*&& inertiaCoroutine == null*/)
            {
                dragSpeed = Vector2.zero;
                if (!m_invertOrder && (m_currentSelected > 0 || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
                    CurrentSelectedIndex--;
                else if (m_invertOrder && (m_currentSelected < (elements.Length - 1) || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
                    CurrentSelectedIndex++;

                //CurrentSelectedIndex = targetIndex;
                ChangeLetters.m_ChangeLetters.DecreaseIndex();
                StartScrolling();
            }
        }
        public void ScrollBack(int index)
        {
            //Debug.Log ("ScrollBack");

            float speed = (m_alignment == Alignment.Horizontal) ? Mathf.Abs(dragSpeed.x) : Mathf.Abs(dragSpeed.y);
            //Debug.Log (speed);

            if (inertia && speed > minDragSpeed && dragTime < 0.05f)
            {
                StartInertia();
            }
            else if (!isScrolling /*&& inertiaCoroutine == null*/)
            {
                dragSpeed = Vector2.zero;
                if (!m_invertOrder && (m_currentSelected > 0 || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
                    index--;
                else if (m_invertOrder && (m_currentSelected < (elements.Length - 1) || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
                    index++;

                CurrentSelectedIndex = index;
                ChangeLetters.m_ChangeLetters.DecreaseIndex();
                StartScrolling();
            }
        }

        /// <summary>
        /// Scrolls the content forward.
        /// </summary>

        public void ScrollForward()
        {
            //Debug.Log ("ScrollForward");
            //Debug.Log (isScrolling);

            float speed = (m_alignment == Alignment.Horizontal) ? Mathf.Abs(dragSpeed.x) : Mathf.Abs(dragSpeed.y);

            if (inertia && speed > minDragSpeed && dragTime < 0.05f)
            {
                StartInertia();
            }
            else if (!isScrolling /* && inertiaCoroutine == null */)
            {
                dragSpeed = Vector2.zero;
                if (!m_invertOrder && (m_currentSelected < (elements.Length - 1) || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
                    CurrentSelectedIndex++;
                else if (m_invertOrder && (m_currentSelected > 0 || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
                    CurrentSelectedIndex--;

                //CurrentSelectedIndex = targetIndex;
                ChangeLetters.m_ChangeLetters.IncreaeIndex();
                StartScrolling();
            }
        }
        public void Scroll(int index)
        {
            //Debug.Log ("ScrollForward");
            //Debug.Log (isScrolling);

            // float speed = (m_alignment == Alignment.Horizontal) ? Mathf.Abs(dragSpeed.x) : Mathf.Abs(dragSpeed.y);

            // if (inertia && speed > minDragSpeed && dragTime < 0.05f)
            // {
            //     StartInertia();
            // }
            // else if (!isScrolling /* && inertiaCoroutine == null */)
            // {
            //     dragSpeed = Vector2.zero;
            //     if (!m_invertOrder && (m_currentSelected < (elements.Length - 1) || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
            //         index++;
            //     else if (m_invertOrder && (m_currentSelected > 0 || (m_infiniteScrolling && m_layoutMode == LayoutMode.Circular)))
            //         index--;

            CurrentSelectedIndex = index;
            ChangeLetters.m_ChangeLetters.ChangeUIValues(index + 1);
            ChangeLetters.m_ChangeLetters.m_index = index + 1;
            StartScrolling();
            // }
        }

        /// <summary>
        /// This should be used on Swipe Cancel event.
        /// </summary>
        public void SwipeCancel()
        {
            //Debug.Log ("Swipe Cancel");
            if (!isScrolling)
            {
                if (m_snapMode == SnapMode.Both || m_snapMode == SnapMode.SnapToNearest)
                    SnapAtNearest();
                else if (m_snapMode == SnapMode.Swipe)
                    StartScrolling();
            }
        }

        /// <summary>
        /// Scrolls to a specific index.
        /// </summary>
        /// <param name="index"></param>
        public void ScrollTo(int index)
        {
            StopAllCoroutines();
            isScrolling = false;
            ScrollAngle %= 180f;
            // index = Random.Range (0, elements.Length - 1);
            CurrentSelectedIndex = index = Mathf.Clamp(index, 0, elements.Length - 1);
            StartScrolling();
        }

        /// <summary>
        /// Instantly scrolls to the desired element.
        /// </summary>
        /// <param name="index">Index.</param>
        public void InstantScrollTo(int index)
        {
            StopAllCoroutines();
            isScrolling = false;
            CurrentSelectedIndex = index;
            ScrollAngle = -elementAngleOffset[index];
            indexTableManager.MoveIndicator(index, InvertOrder);
            for (int i = 0; i < elements.Length; i++)
                CircularPosition(elements, radius, i);
        }

        private void SnapAtNearest()
        {
            if (!isScrolling)
            {
                //Debug.Log ("Snapping to nearest");
                CurrentSelectedIndex = FindNearestElement();
                StartScrolling();
            }
        }

        private int FindNearestElement()
        {
            int index;

            float angle = Mathf.Repeat(180f - m_scrollAngle, 180f);
            angle.NearestAbsolute(elementAngleOffset, out index);

            if (!m_infiniteScrolling)
            {
                if (m_scrollAngle <= -elementAngleOffset[elements.Length - 1])
                    index = elements.Length - 1;
                else if (m_scrollAngle >= 0f)
                    index = 0;
            }

            return index;
        }

        private void StartInertia()
        {
            inertiaCoroutine = StartCoroutine(InertiaCoroutine());
        }

        private IEnumerator InertiaCoroutine()
        {
            if (elements.Length < 1)
                yield break;

            //Debug.Log ("Inertia Started");
            isScrolling = true;

            float singleAxisSpeed = (m_alignment == Alignment.Horizontal) ? dragSpeed.x : -dragSpeed.y;
            float minScrollLimit = -elementAngleOffset[elements.Length - 1] - ScrollAdditionalLimits, maxSrollLimit = ScrollAdditionalLimits;
            dragSpeed = Vector2.zero;
            onSelectionChange.Invoke(null);

            //Debug.Log(singleAxisSpeed);

            Sign direction = (Sign)Mathf.Sign((m_invertOrder) ? -singleAxisSpeed : singleAxisSpeed);

            while (Mathf.Abs(singleAxisSpeed) > 0f)
            {
                //Debug.Log(direction);
                singleAxisSpeed *= Mathf.Pow(m_decelerationRate, Time.unscaledDeltaTime);

                if (realtimeSelection && m_snapMode != SnapMode.None) // Changing selection in realtime.
                {
                    CurrentSelectedIndex = FindNearestElement();
                    onSelectionChange.Invoke(elements[m_currentSelected].gameObject);
                    if (indexTableManager) indexTableManager.MoveIndicator(m_currentSelected, m_invertOrder);
                }

                if (!m_infiniteScrolling && (m_scrollAngle <= minScrollLimit || m_scrollAngle >= maxSrollLimit) && m_snapMode != SnapMode.None)
                    singleAxisSpeed = TransitionSpeed * Mathf.Sign(singleAxisSpeed);


                if (m_snapMode != SnapMode.None && Mathf.Abs(MyMath.AngleFromLength(singleAxisSpeed, radius)) <= TransitionSpeed)
                {
                    isScrolling = false;

                    if (m_snapMode.IsEqual(SnapMode.Swipe, SnapMode.Both))
                    {
                        CurrentSelectedIndex = FindNearestElement();
                        int nextIndex = m_currentSelected + ((int)direction * -1);
                        CurrentSelectedIndex = (m_infiniteScrolling) ? nextIndex : Mathf.Clamp(nextIndex, 0, elements.Length - 1);
                        StartScrolling(true);
                    }
                    else if (m_snapMode == SnapMode.SnapToNearest)
                        SnapAtNearest();

                    break;
                }
                else if (m_snapMode == SnapMode.None && Mathf.Abs(singleAxisSpeed) < 1f)
                {
                    isScrolling = false;
                    break;
                }

                SetScrollingAngle(singleAxisSpeed);
                yield return null;
            }

            yield return null;
        }

        private void StartScrolling(bool inertiaEnd = false)
        {
            scrollCoroutine = StartCoroutine(ScrollCoroutine(inertiaEnd));
        }

        private IEnumerator ScrollCoroutine(bool intertiaEnd)
        {
            if (!isScrolling && elements.Length > 0)
            {
                //Debug.Log ("Scrolling");
                isScrolling = true;

                if (!m_infiniteScrolling)
                    if (m_scrollAngle < -elementAngleOffset[elements.Length - 1])
                        CurrentSelectedIndex = elements.Length - 1;
                    else if (m_scrollAngle > 0f)
                        CurrentSelectedIndex = 0;

                if (indexTableManager)
                    indexTableManager.MoveIndicator(m_currentSelected, m_invertOrder);


                float indexAngle = -elementAngleOffset[m_currentSelected];
                float targetAngle = (m_infiniteScrolling) ? ScrollAngle - MyMath.EnhancedRepeat(ScrollAngle - indexAngle, 90f) : indexAngle;
                //float minDist = Mathf.Abs (ScrollAngle - targetAngle) * 0.25f;

                //Debug.Log (ScrollAngle);
                //Debug.Log (targetAngle);
                //Debug.Log (CurrentSelectedIndex);

                if (!onSelectionChangeDelay)
                    onSelectionChange.Invoke(elements[m_currentSelected].gameObject);

                if (!intertiaEnd)
                {
                    float curveTime = 0f;
                    float curveAmount = transitionCurve.Evaluate(curveTime);
                    float initialAngle = ScrollAngle;
                    while (curveAmount < 1.0f)
                    {

                        curveTime += m_transitionSpeed * Time.unscaledDeltaTime;
                        curveAmount = transitionCurve.Evaluate(curveTime);
                        ScrollAngle = Mathf.Lerp(initialAngle, targetAngle, curveAmount);

                        for (int i = 0; i < elements.Length; i++)
                            CircularPosition(elements, radius, i);

                        //Debug.Log (ScrollAngle);
                        yield return null;
                    }
                }
                else
                {
                    while (ScrollAngle != targetAngle)
                    {
                        ScrollAngle = Mathf.MoveTowards(ScrollAngle, targetAngle, TransitionSpeed);

                        for (int i = 0; i < elements.Length; i++)
                            CircularPosition(elements, radius, i);

                        onScrolling.Invoke();
                        yield return null;
                    }
                }

                if (onSelectionChangeDelay)
                    onSelectionChange.Invoke(elements[m_currentSelected].gameObject);

                ScrollAngle %= 180f;
                ViewportCG.blocksRaycasts = true;
                inertiaCoroutine = null;
                scrollCoroutine = null;
                isScrolling = false;
            }
        }

        /// <summary>
        /// Resets the scroll position.
        /// </summary>
        public void ResetScroll()
        {
            StopAllCoroutines();
            m_currentSelected = 0;
            ScrollAngle = 0;
            isScrolling = false;
            if (ViewportCG != null)
                ViewportCG.blocksRaycasts = true;
            SetScrollingAngle(0);

            //AlignElements ();

            if (onSelectionChange != null && elements.Length > 0)
                onSelectionChange.Invoke(elements[m_currentSelected].gameObject);

            if (indexTableManager != null)
                indexTableManager.MoveIndicator(m_currentSelected, m_invertOrder);

            //if (Application.isPlaying)
            //    StartCoroutine(ScrollCoroutine ());
        }

        public void OnPointerDown(PointerEventData ped)
        {
            //Debug.Log (isScrolling);

            if (ped.button != PointerEventData.InputButton.Left || !RectTransformUtility.RectangleContainsScreenPoint(viewport, ped.position, ped.pressEventCamera) || ped.pointerId > 0)
                return;

            //Debug.Log("Pointer Down");

            pointerDown = true;
            dragTime = 0f;

            if (isScrolling)
            {
                if (scrollCoroutine != null)
                {
                    StopCoroutine(scrollCoroutine);
                    scrollCoroutine = null;
                    isScrolling = false;
                }

                if (inertiaCoroutine != null)
                {
                    StopCoroutine(inertiaCoroutine);
                    inertiaCoroutine = null;
                    isScrolling = false;
                }

                CurrentSelectedIndex = FindNearestElement();
            }

        }

        public void OnPointerUp(PointerEventData ped)
        {
            if (ped.button != PointerEventData.InputButton.Left || !pointerDown)
                return;

            //Debug.Log("Pointer Up");

            if (m_snapMode != SnapMode.None && (dragTime == 0f /*|| (Input.touchCount > 1 && ped.pointerId <= 0)*/))
            {
                SnapAtNearest();
            }
        }

        private void DoForParents<T>(System.Action<T> action) where T : IEventSystemHandler
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                foreach (var component in parent.GetComponents<Component>())
                {
                    if (component is T)
                        action((T)(IEventSystemHandler)component);
                }
                parent = parent.parent;
            }
        }

        // public void OnInitializePotentialDrag (PointerEventData ped)
        // {
        //     DoForParents<IInitializePotentialDragHandler>((parent) => { parent.OnInitializePotentialDrag(ped); });
        // }

        public void OnBeginDrag(PointerEventData ped)
        {
            if (m_alignment == Alignment.Vertical && Mathf.Abs(ped.delta.x) > Mathf.Abs(ped.delta.y))
                routeToParent = true;
            else if (m_alignment == Alignment.Horizontal && Mathf.Abs(ped.delta.x) < Mathf.Abs(ped.delta.y))
                routeToParent = true;
            else
                routeToParent = false;

            if (routeToParent)
            {
                DoForParents<IBeginDragHandler>((parent) => { parent.OnBeginDrag(ped); });
                // return;
            }

            if (ped.button != PointerEventData.InputButton.Left || ped.pointerId > 0)
                return;

            pointerDown = false;
            if ((viewport != null && !RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, ped.position, ped.pressEventCamera, out startLocalCursor)) ||
                !RectTransformUtility.RectangleContainsScreenPoint(viewport, ped.position, ped.pressEventCamera))
                return;

            dragTime = 0f;
            dragSpeed = Vector2.zero;
            startTime = Time.time;
            pointerDown = true;
            ViewportCG.blocksRaycasts = true;
        }

        public void OnEndDrag(PointerEventData ped)
        {
            if (routeToParent)
            {
                DoForParents<IEndDragHandler>((parent) => { parent.OnEndDrag(ped); });
                routeToParent = false;
                SwipeCancel();
                return;
            }

            //Debug.Log ("End Drag");

            if (ped.button != PointerEventData.InputButton.Left || !pointerDown || ped.pointerId > 0)
                return;

            if (viewport != null && !RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, ped.position, ped.pressEventCamera, out lastLocalCursor))
                return;


            endTime = Time.time;
            dragTime = endTime - startTime;
            float singleSpeed = (m_alignment == Alignment.Horizontal) ? Mathf.Abs(dragSpeed.x) : Mathf.Abs(dragSpeed.y);

            if (m_snapMode.IsEqual(SnapMode.SnapToNearest, SnapMode.None) && inertia && singleSpeed > minDragSpeed && dragTime < 0.05f)
                StartInertia();
            else if (m_snapMode == SnapMode.SnapToNearest)
                SnapAtNearest();

            ViewportCG.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData ped)
        {
            if (routeToParent)
            {
                DoForParents<IDragHandler>((parent) => { parent.OnDrag(ped); });
            }

            if (ped.button != PointerEventData.InputButton.Left || !pointerDown || ped.pointerId > 0)
                return;

            if (viewport != null && !isScrolling)
            {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, ped.position, ped.pressEventCamera, out lastLocalCursor))
                    return;

                Vector2 distance = (lastLocalCursor - startLocalCursor) * Mathf.Lerp(1f, 0f, m_dragDelay); ;

                endTime = Time.time;
                dragTime = endTime - startTime;
                dragSpeed = distance / dragTime * Time.unscaledDeltaTime;
                dragSpeed = (float.IsInfinity(dragSpeed.x) || float.IsInfinity(dragSpeed.y) || float.IsNaN(dragSpeed.x) || float.IsNaN(dragSpeed.y)) ? Vector2.zero : dragSpeed;

                float step = (m_alignment == Alignment.Horizontal) ? distance.x : -distance.y;
                SetScrollingAngle(step);

                startLocalCursor = lastLocalCursor;
                startTime = Time.time;
                onScrolling.Invoke();

                if (realtimeSelection && m_snapMode == SnapMode.SnapToNearest) // Changing selection in realtime.
                {
                    CurrentSelectedIndex = FindNearestElement();
                    onSelectionChange.Invoke(elements[m_currentSelected].gameObject);
                    if (indexTableManager) indexTableManager.MoveIndicator(m_currentSelected, m_invertOrder);
                }
            }
        }

        private void SetScrollingAngle(float step)
        {
            if (viewport != null && elements.Length > 0)
            {
                //Debug.Log (step);

                float offsetAngle = MyMath.AngleFromLength(step, radius);
                if (m_invertOrder)
                    offsetAngle *= -1;

                ScrollAngle = m_scrollAngle + offsetAngle;

                if (!m_infiniteScrolling)
                {
                    ScrollAngle = Mathf.Clamp(ScrollAngle, -elementAngleOffset[elements.Length - 1] - m_scrollAdditionalLimits, m_scrollAdditionalLimits);
                }

                //Debug.Log (ScrollAngle);
                //Debug.Log (Mathf.Floor (-ScrollAngle / 180f));

                for (int i = 0; i < elements.Length; i++)
                    CircularPosition(elements, radius, i);
            }
        }

        private void CircularPosition(RectTransform[] transformArray, float radius, int index)
        {
            if (elements.Length < 1)
                return;

            float angle, angPosition, circularFactor = m_circularFactor;
            int curve = (int)m_curvature;
            Vector3 pos, direction = (m_alignment == Alignment.Horizontal) ? Vector3.right : Vector3.down;

            if (m_layoutMode == LayoutMode.Linear)
            {
                angle = m_scrollAngle + elementAngleOffset[index];
                circularFactor = 0;
            }
            else
            {
                float repeatedAngle = MyMath.EnhancedRepeat(m_scrollAngle, 180f);
                float offset = elementAngleOffset[index];
                angle = MyMath.EnhancedRepeat(repeatedAngle + offset, 90f);
            }

            if (m_invertOrder)
                angle *= -1;

            angPosition = Mathf.Lerp(radius * (angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle * Mathf.Deg2Rad), circularFactor);

            pos = direction * angPosition;

            //            pos.x = pos.x + elementPivotCancel [index].x + viewportPivotCancel.x;
            //            pos.y = pos.y + elementPivotCancel [index].y + viewportPivotCancel.y;

            if (m_layoutMode == LayoutMode.Circular)
            {
                pos.z = Mathf.Lerp(0, (radius * curve * Mathf.Cos(angle * Mathf.Deg2Rad)) + (radius * -curve), circularFactor);
                pos.z += radius * m_distanceOffset;
                transformArray[index].localPosition = pos.z * Vector3.forward;
            }

            transformArray[index].anchoredPosition = pos;

            if (m_rotate && m_layoutMode == LayoutMode.Circular)
                transformArray[index].localEulerAngles = Vector3.up * (m_circularFactor * (angle * curve));
            else
                transformArray[index].localEulerAngles = Vector3.zero;

            //Debug.Log (angle);
            //Debug.Log (transformArray [index].localRotation);
        }

        public float GetElementAngle(int index, bool absolute)
        {
            if (elements.Length < 1)
                return 0;

            index = (m_infiniteScrolling) ? index % elements.Length : Mathf.Clamp(index, 0, elements.Length - 1);
            float indexAngle = -elementAngleOffset[index];
            float angleDistance = MyMath.EnhancedRepeat(ScrollAngle - indexAngle, 90f);
            return (absolute) ? Mathf.Abs(angleDistance) : angleDistance;
        }

        public float GetNormalizedElementAngle(int index, bool absolute)
        {
            float angle = GetElementAngle(index, absolute);
            float normalizedAngle = angle % 90f / 90f;
            return normalizedAngle;
        }

        // public void AddNewElement ()
        // {
        //     if (viewport != null)
        //     {
        //         GameObject element = new GameObject ("New Element");
        //         element.transform.SetParent (viewport.transform);
        //         element.layer = 5;

        //         element.AddComponent<RectTransform> ();
        //         element.AddComponent<UnityEngine.UI.Image> ();
        //     }
        // }

        //public void RemoveElement ()
        //{
        //    foreach (Transform child in viewport.transform)
        //    {
        //        GameObject.Destroy (child.gameObject);
        //    }

        //}
        //public void Debugging ()
        //{
        //    Debug.Log ("Working!!!");
        //}

        private void SetupSwipeDetection()
        {
            //Debug.Log ("Setting Up Swipe Detection");

            if (SwipeDetect == null && m_snapMode.IsEqual(SnapMode.Swipe, SnapMode.Both))
                m_swipeDetect = gameObject.AddComponent<SwipeDetection>();
            else if (SwipeDetect != null && m_snapMode.IsEqual(SnapMode.SnapToNearest, SnapMode.None))
                SafeOperations.Destroy(m_swipeDetect);

            if (m_swipeDetect != null && m_snapMode.IsEqual(SnapMode.Swipe, SnapMode.Both))
            {
#if UNITY_EDITOR
                Undo.RecordObject(m_swipeDetect, "Events created");
#endif
                m_swipeDetect.viewport = viewport;
                if (m_swipeDetect.isActiveAndEnabled)
                {

                    m_swipeDetect.swipeEvents = null;
                    m_swipeDetect.swipeEvents = new List<SwipeDetection.SwipeEvent>();

                    if (m_alignment == Alignment.Horizontal)
                    {
                        m_swipeDetect.swipeEvents.Add(new SwipeDetection.SwipeEvent(Swipe.Left));
                        m_swipeDetect.swipeEvents.Add(new SwipeDetection.SwipeEvent(Swipe.Right));
                    }
                    else
                    {
                        m_swipeDetect.swipeEvents.Add(new SwipeDetection.SwipeEvent(Swipe.Up));
                        m_swipeDetect.swipeEvents.Add(new SwipeDetection.SwipeEvent(Swipe.Down));
                    }

                    m_swipeDetect.swipeEvents.Add(new SwipeDetection.SwipeEvent(Swipe.Cancel));
#if UNITY_EDITOR
                    UnityEventTools.AddPersistentListener(m_swipeDetect.swipeEvents[0].callback, ScrollForward);
                    UnityEventTools.AddPersistentListener(m_swipeDetect.swipeEvents[1].callback, ScrollBack);
                    UnityEventTools.AddPersistentListener(m_swipeDetect.swipeEvents[2].callback, SwipeCancel);
#else
                    m_swipeDetect.swipeEvents [0].callback.AddListener (ScrollForward);
                    m_swipeDetect.swipeEvents [1].callback.AddListener (ScrollBack);
                    m_swipeDetect.swipeEvents [2].callback.AddListener (SwipeCancel);
#endif
                }
            }
        }
    }
    #endregion

    [System.Serializable]
    public class SelectionChangeEvent : UnityEngine.Events.UnityEvent<GameObject>
    {
    }
}