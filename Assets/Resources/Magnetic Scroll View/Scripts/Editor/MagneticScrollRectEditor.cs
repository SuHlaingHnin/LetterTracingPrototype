using UnityEditor;
using UnityEngine;
using UnityEditor.Events;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MagneticScrollView
{
    [System.Serializable]
    [CanEditMultipleObjects]
    [CustomEditor (typeof (MagneticScrollRect))]
    public class MagneticScrollRectEditor : Editor
    {
        private SerializedProperty elements, viewport, indexTableManager, autoArranging, /*useButtons, useIndexTable,*/ alignment, layoutMode, curvature, inertia, decelerationRate, realtimeSelection, infiniteScrolling, smoothTransition, transitionCurve, invertOrder, useMargin, rotate, snapMode,
            resizeMode, elementsSize, globalOffset, elementPadding, scrollAdditionalLimits, circularFactor, distanceOffset, transitionSpeed, dragDelay, backButton, forwardButton, indexTable, onSelectionChange, onSelectionChangeDelay, onScrolling;

        private GUIContent autoArrangingButton;
        private MagneticScrollRect myTarget;
        private Mask viewportMask;

        private bool layoutSettings = true, controlSettings = true, otherSettings = true, maskViewport, showMaskGraphic, useButtons = false, useIndexTable = false;

        private MethodInfo SetupSwipeDetection;
        private MethodInfo ResetAnchors;
        private const string bgSpritePath = "UI/Skin/Background.psd";

        private void OnEnable ()
        {
            myTarget = (MagneticScrollRect)target;

            if (myTarget.viewport != null)
            {
                viewportMask = myTarget.viewport.GetComponent<Mask> ();
                if (viewportMask != null)
                {
                    maskViewport = viewportMask.enabled;
                    showMaskGraphic = viewportMask.showMaskGraphic;
                }
            }

            //Variables
            elements = serializedObject.FindProperty ("elements");
            viewport = serializedObject.FindProperty ("viewport");
            indexTableManager = serializedObject.FindProperty ("indexTableManager");
            autoArranging = serializedObject.FindProperty ("m_autoArranging");
            alignment = serializedObject.FindProperty ("m_alignment");
            layoutMode = serializedObject.FindProperty ("m_layoutMode");
            curvature = serializedObject.FindProperty ("m_curvature");
            inertia = serializedObject.FindProperty ("inertia");
            decelerationRate = serializedObject.FindProperty ("m_decelerationRate");
            realtimeSelection = serializedObject.FindProperty ("realtimeSelection");
            infiniteScrolling = serializedObject.FindProperty ("m_infiniteScrolling");
            //smoothTransition = serializedObject.FindProperty ("smoothTransition");
            transitionCurve = serializedObject.FindProperty ("transitionCurve");
            invertOrder = serializedObject.FindProperty ("m_invertOrder");
            useMargin = serializedObject.FindProperty ("m_useMargin");
            rotate = serializedObject.FindProperty ("m_rotate");
            snapMode = serializedObject.FindProperty ("m_snapMode");
            resizeMode = serializedObject.FindProperty ("m_resizeMode");
            elementsSize = serializedObject.FindProperty ("m_elementsSize");
            //			globalOffset = serializedObject.FindProperty ("globalOffset");
            elementPadding = serializedObject.FindProperty ("m_elementPadding");
            scrollAdditionalLimits = serializedObject.FindProperty ("m_scrollAdditionalLimits");
            circularFactor = serializedObject.FindProperty ("m_circularFactor");
            distanceOffset = serializedObject.FindProperty ("m_distanceOffset");
            transitionSpeed = serializedObject.FindProperty ("m_transitionSpeed");
            dragDelay = serializedObject.FindProperty ("m_dragDelay");
            //useButtons = serializedObject.FindProperty ("useButtons");
            //useIndexTable = serializedObject.FindProperty ("useIndexTable");
            onSelectionChange = serializedObject.FindProperty ("onSelectionChange");
            onSelectionChangeDelay = serializedObject.FindProperty ("onSelectionChangeDelay");
            onScrolling = serializedObject.FindProperty ("onScrolling");

            //Not visible in Inspector
            backButton = serializedObject.FindProperty ("backButton");
            forwardButton = serializedObject.FindProperty ("forwardButton");
            indexTable = serializedObject.FindProperty ("indexTable");

            SetupSwipeDetection = typeof (MagneticScrollRect).GetMethod ("SetupSwipeDetection", BindingFlags.NonPublic | BindingFlags.Instance);
            ResetAnchors = typeof (MagneticScrollRect).GetMethod ("ResetAnchors", BindingFlags.NonPublic | BindingFlags.Instance);

            autoArrangingButton = new GUIContent ();
            autoArrangingButton.tooltip = "The Auto Arranging Mode allow to automatically re-organize the elements";

            if (indexTable.objectReferenceValue == null && indexTableManager.objectReferenceValue != null)
                indexTable.objectReferenceValue = (indexTableManager.objectReferenceValue as IndexTableManager).gameObject;

            //Debug.Log (indexTableManager.objectReferenceValue);
            //Debug.Log ("Enable");

            var backButtonGO = backButton.objectReferenceValue as GameObject;
            var forwardButtonGO = forwardButton.objectReferenceValue as GameObject;
            var indexTableGO = indexTable.objectReferenceValue as GameObject;

            //Debug.Log (indexTableGO, indexTableGO);

            if (backButtonGO != null && forwardButtonGO != null)
            {
                useButtons = backButtonGO.activeSelf || forwardButtonGO.activeSelf;
                if (!useButtons)
                {
                    backButton.objectReferenceValue.hideFlags = HideFlags.HideInHierarchy;
                    forwardButton.objectReferenceValue.hideFlags = HideFlags.HideInHierarchy;
                }
            }

            if (indexTableGO == null)
            {
                useIndexTable = false;
                serializedObject.ApplyModifiedProperties ();
            }
            else
            {
                useIndexTable = indexTableGO.activeSelf;
                indexTableGO.hideFlags = HideFlags.None;
                //if (!useIndexTable)
                //    indexTableGO.hideFlags = HideFlags.HideInHierarchy;
                serializedObject.ApplyModifiedProperties ();
            }

            EditorApplication.DirtyHierarchyWindowSorting ();
            //EditorApplication.RepaintHierarchyWindow ();

        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            ShowInspector ();
            serializedObject.ApplyModifiedProperties ();
        }

        private void ShowInspector ()
        {
            //EditorGUIUtility.labelWidth += 35f;
            Rect lastRect;
            EditorStyles.foldout.fontStyle = FontStyle.Bold;
            EditorStyles.helpBox.fontSize = 11;
            EditorStyles.helpBox.fontStyle = FontStyle.Normal;
            //EditorGUI.indentLevel++;
            EditorGUILayout.Space ();

            GUILayout.BeginVertical (new GUIStyle (GUI.skin.box));
            GUILayout.Space (4f);

            EditorGUI.BeginChangeCheck ();
            EditorGUIUtility.labelWidth -= 50f;
            EditorGUILayout.PropertyField (viewport);
            EditorGUIUtility.labelWidth += 50f;
            if (EditorGUI.EndChangeCheck ())
            {
                serializedObject.ApplyModifiedProperties ();
                if (viewport.objectReferenceValue != null)
                {
                    viewportMask = myTarget.viewport.GetComponent<Mask> ();
                    myTarget.ArrangeElements ();
                }
            }

            if (viewport.objectReferenceValue == null)
                EditorGUILayout.HelpBox ("Viewport reference is missing!", MessageType.Error);
            //EditorGUI.indentLevel--;

            GUILayout.Space (4f);
            GUILayout.EndVertical ();
            GUILayout.Space (1f);

            // LAYOUT OPTIONS

            layoutSettings = EditorGUILayout.Foldout (layoutSettings, "Layout Settings", true);
            if (layoutSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth += 35f;

                EditorGUILayout.Space ();
                Rect rect = GUILayoutUtility.GetRect (autoArrangingButton, GUI.skin.button, GUILayout.Height (35f));

                rect.x += (rect.width - 225f) / 2f - 4;
                rect.width = 225f;

                GUIStyle newStyle = new GUIStyle (GUI.skin.button);
                //newStyle.normal.background.Color

                if (autoArranging.boolValue)
                {
                    autoArrangingButton.text = "STOP AUTO ARRANGING";
                    newStyle.normal.textColor = new Color (0.7f, 0, 0);
                }
                else
                {
                    autoArrangingButton.text = "START AUTO ARRANGING";
                    newStyle.normal.textColor = new Color (0, 0.7f, 0);
                }


                newStyle.fontSize = 13;
                newStyle.fontStyle = FontStyle.Bold;

                if (GUI.Button (rect, autoArrangingButton, newStyle))
                {
                    Undo.RecordObject (myTarget, "Auto Arranging State Change");
                    if (autoArranging.boolValue)
                    {
                        autoArranging.boolValue = false;
                        myTarget.StopAutoArranging ();
                    }
                    else
                    {
                        autoArranging.boolValue = true;
                        myTarget.StartAutoArranging ();
                    }

                    serializedObject.ApplyModifiedProperties ();
                }
                EditorGUILayout.Space ();

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (alignment);
                if (EditorGUI.EndChangeCheck ()) // END CHECKK
                {
                    serializedObject.ApplyModifiedProperties ();
                    for (int i = 0; i < elements.arraySize; i++)
                        Undo.RegisterCompleteObjectUndo (elements.GetArrayElementAtIndex (i).objectReferenceValue, "Elements has changed");

                    myTarget.ArrangeElements (false);
                    SetupSwipeDetection.Invoke (myTarget, null);
                }

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (layoutMode);
                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();

                    for (int i = 0; i < elements.arraySize; i++)
                        Undo.RegisterCompleteObjectUndo (elements.GetArrayElementAtIndex (i).objectReferenceValue, "Elements has changed");

                    if (layoutMode.enumValueIndex == (int)LayoutMode.Linear)
                    {
                        infiniteScrolling.boolValue = false;
                        circularFactor.floatValue = 0f;
                    }

                    myTarget.ResetScroll ();
                    myTarget.ArrangeElements ();
                }
                if (layoutMode.enumValueIndex == (int)LayoutMode.Circular)
                {
                    EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                    EditorGUILayout.PropertyField (curvature);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        serializedObject.ApplyModifiedProperties ();

                        for (int i = 0; i < elements.arraySize; i++)
                            Undo.RegisterCompleteObjectUndo (elements.GetArrayElementAtIndex (i).objectReferenceValue, "Elements has changed");

                        myTarget.ResetScroll ();
                        myTarget.ArrangeElements ();
                    }
                }

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (resizeMode);
                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();

                    for (int i = 0; i < elements.arraySize; i++)
                        Undo.RegisterCompleteObjectUndo (elements.GetArrayElementAtIndex (i).objectReferenceValue, "Elements has changed");

                    if (resizeMode.enumValueIndex != (int)ResizeMode.FitToViewport)
                        ResetAnchors.Invoke (myTarget, null);

                    myTarget.ArrangeElements ();
                }

                if (resizeMode.enumValueIndex == (int)ResizeMode.PresetSize)
                {
                    EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                    EditorGUILayout.PropertyField (elementsSize);
                    if (EditorGUI.EndChangeCheck ()) // END CHECK
                    {
                        for (int i = 0; i < elements.arraySize; i++)
                            Undo.RegisterCompleteObjectUndo (elements.GetArrayElementAtIndex (i).objectReferenceValue, "Elements has changed");

                        if (viewport.objectReferenceValue != null)
                        {
                            RectTransform viewportRT = viewport.objectReferenceValue as RectTransform;
                            elementsSize.vector2Value = new Vector2 (
                                Mathf.Clamp (elementsSize.vector2Value.x, 0.1f, viewportRT.rect.width),
                                Mathf.Clamp (elementsSize.vector2Value.y, 0.1f, viewportRT.rect.height)
                                );
                        }

                        serializedObject.ApplyModifiedProperties ();
                        myTarget.ArrangeElements ();
                    }
                }

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (invertOrder);
                EditorGUILayout.PropertyField (useMargin);
                //				EditorGUILayout.PropertyField (globalOffset);
                EditorGUILayout.PropertyField (elementPadding);

                if (layoutMode.enumValueIndex == (int)LayoutMode.Circular)
                {
                    EditorGUILayout.PropertyField (circularFactor);
                    EditorGUILayout.PropertyField (distanceOffset);
                    EditorGUILayout.PropertyField (rotate);
                }

                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    for (int i = 0; i < elements.arraySize; i++)
                        Undo.RecordObject (elements.GetArrayElementAtIndex (i).objectReferenceValue, "Element Changed");

                    serializedObject.ApplyModifiedProperties ();
                    myTarget.ArrangeElements (false);
                    myTarget.ResetScroll ();
                }

                GUILayout.Space (2f);
                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth -= 35f;
            }

            // CONTROL OPTIONS

            controlSettings = EditorGUILayout.Foldout (controlSettings, "Control Settings", true);
            if (controlSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth += 35f;

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (snapMode);
                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();
                    Undo.RecordObject (myTarget, "Snap Mode has changed");
                    myTarget.ResetScroll ();
                    //if (snapMode.enumValueIndex.IsIn<int> ((int)SnapMode.Swipe, (int)SnapMode.Both))
                    //{  
                    SetupSwipeDetection.Invoke (myTarget, null);
                    //}
                    //else if (myTarget.SwipeDetect != null)
                    //{
                    //SetupSwipeDetection.Invoke (myTarget, null);
                    GUIUtility.ExitGUI ();
                }

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (inertia);
                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();
                }

                if (inertia.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField (decelerationRate);
                    if (snapMode.enumValueIndex != (int)SnapMode.SnapToNearest)
                        EditorGUILayout.PropertyField (realtimeSelection, new GUIContent ("Realtime Selection"));
                    EditorGUI.indentLevel--;
                }

                if (snapMode.enumValueIndex == (int)SnapMode.SnapToNearest)
                    EditorGUILayout.PropertyField (realtimeSelection);

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                EditorGUILayout.PropertyField (infiniteScrolling);
                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();
                    if (layoutMode.enumValueIndex == (int)LayoutMode.Linear)
                        infiniteScrolling.boolValue = false;
                    myTarget.ResetScroll ();
                }

                //if (snapMode.enumValueIndex != (int)SnapMode.SnapToNearest)
                //{
                //    EditorGUILayout.HelpBox ("Inertia only works in Swipe, Both or None Snap Mode!", MessageType.Warning, true);
                //}

                if (layoutMode.enumValueIndex == (int)LayoutMode.Linear)
                {
                    EditorGUILayout.HelpBox ("Infinite Scrolling only works with 'Circular' Layout Mode!", MessageType.Warning, true);
                }

                //EditorGUILayout.PropertyField (smoothTransition);
                EditorGUILayout.PropertyField (transitionCurve);
                EditorGUILayout.PropertyField (transitionSpeed, new GUIContent ("Transition Speed"));
                if (!infiniteScrolling.boolValue) EditorGUILayout.PropertyField (scrollAdditionalLimits);
                EditorGUILayout.PropertyField (dragDelay);

                GUILayout.Space (2f);
                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth -= 35f;
            }

            otherSettings = EditorGUILayout.Foldout (otherSettings, "Other Settings", true);
            if (otherSettings)
            {
                //EditorGUIUtility.labelWidth += 35f;

                //Begin Horizontal
                EditorGUILayout.BeginHorizontal ();

                //Begin Vertical
                EditorGUILayout.BeginVertical ();
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                //EditorGUILayout.PropertyField (useButtons);
                useButtons = EditorGUILayout.Toggle (new GUIContent ("Use Buttons", "Use buttons to scroll content."), useButtons);

                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();
                    if (useButtons)
                        CreateButtons ();
                    else
                        RemoveButtons ();

                    //EditorApplication.RepaintHierarchyWindow ();
                    EditorApplication.DirtyHierarchyWindowSorting ();
                }

                if (indexTable.objectReferenceValue == null)
                    useIndexTable = false;

                EditorGUI.BeginChangeCheck (); // BEGIN CHECK
                //EditorGUILayout.PropertyField (useIndexTable);
                useIndexTable = EditorGUILayout.Toggle (new GUIContent ("Use Index Table", "Use Index Table to indicate the index position."), useIndexTable);
                if (EditorGUI.EndChangeCheck ()) // END CHECK
                {
                    serializedObject.ApplyModifiedProperties ();
                    if (useIndexTable)
                        CreateIndexTable ();
                    else
                        RemoveIndexTable ();

                    //EditorApplication.RepaintHierarchyWindow ();
                    EditorApplication.DirtyHierarchyWindowSorting ();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical ();

                if (viewportMask != null)
                {
                    EditorGUILayout.BeginVertical (); //Begin Vertical
                    EditorGUI.BeginChangeCheck ();

                    maskViewport = EditorGUILayout.Toggle (new GUIContent ("Mask Viewport", "This will enable/disable the Mask Component of the viewport."), maskViewport);
                    showMaskGraphic = EditorGUILayout.Toggle (new GUIContent ("Show Mask Image", "This option will toggle on/off the Show Mask Graphic in the Mask Component of the viewport."), showMaskGraphic);

                    //Undo.RecordObject (this, "Inspector Change");
                    if (EditorGUI.EndChangeCheck ())
                    {
                        serializedObject.ApplyModifiedProperties ();

                        if (myTarget.viewport != null)
                        {
                            Undo.RecordObject (viewportMask, "Inspector Change");
                            viewportMask.enabled = maskViewport;
                            viewportMask.showMaskGraphic = showMaskGraphic;
                        }
                    }

                    if (myTarget.viewport != null)
                    {
                        maskViewport = viewportMask.enabled;
                        showMaskGraphic = viewportMask.showMaskGraphic;
                        Repaint ();
                        EditorUtility.SetDirty (viewportMask);
                    }

                    EditorGUILayout.EndVertical ();

                }
                EditorGUILayout.EndHorizontal ();
            }

            GUILayout.Space (15f);

            EditorGUILayout.PropertyField (onSelectionChange);

            lastRect = GUILayoutUtility.GetLastRect ();
            GUI.Label (new Rect ((lastRect.x + lastRect.width) - 60f, lastRect.y + 1f, 80f, 20f), new GUIContent ("Delay"));
            onSelectionChangeDelay.boolValue = GUI.Toggle (new Rect ((lastRect.x + lastRect.width) - 20f, lastRect.y + 1f, 100f, 20f), onSelectionChangeDelay.boolValue, "");
            EditorGUILayout.PropertyField (onScrolling);

            EditorStyles.foldout.fontStyle = FontStyle.Normal;

            EditorGUILayout.Space ();

            //EditorGUILayout.PropertyField (serializedObject.FindProperty ("text"));
        }

        #region Add/Remove Objects
        private void CreateButtons ()
        {
            NewButton (ref backButton, "Scroll Back", new Vector2 (-180f, -25f), myTarget.ScrollBack);
            NewButton (ref forwardButton, "Scroll Forward", new Vector2 (180f, -25f), myTarget.ScrollForward);
        }

        private void NewButton (ref SerializedProperty button, string text, Vector2 offset, UnityAction call)
        {
            if (button.objectReferenceValue == null)
            {
                button.objectReferenceValue = new GameObject (text);
                GameObject buttonGO = button.objectReferenceValue as GameObject;
                RectTransform buttonRT = buttonGO.AddComponent<RectTransform> ();
                //RectTransform buttonRT = buttonGO.GetComponent<RectTransform> ();
                buttonRT.sizeDelta = new Vector2 (120f, 30f);

                RectTransform viewportRT = myTarget.viewport.GetComponent<RectTransform> ();
                buttonRT.anchoredPosition = new Vector2 (offset.x, viewportRT.anchoredPosition.y - viewportRT.rect.height / 2 + offset.y);

                buttonGO.AddComponent<CanvasRenderer> ();
                buttonGO.AddComponent<Image> ();

                Button buttonBT = buttonGO.AddComponent<Button> ();
                UnityEventTools.AddPersistentListener (buttonBT.onClick, call);
                buttonGO.transform.SetParent (myTarget.transform, false);
                buttonGO.layer = 5;

                GameObject textGO;
                if (buttonGO.GetComponentInChildren<Text> () == null)
                    textGO = new GameObject (text + " Text");
                else
                    textGO = buttonGO.GetComponentInChildren<Text> ().gameObject;

                textGO.transform.SetParent (buttonGO.transform, false);
                textGO.layer = 5;
                textGO.AddComponent<RectTransform> ();
                textGO.AddComponent<CanvasRenderer> ();
                Text textComp = textGO.AddComponent<Text> ();
                textComp.text = text;
                textComp.alignment = TextAnchor.MiddleCenter;
                textComp.color = Color.black;

                Undo.RegisterCreatedObjectUndo (button.objectReferenceValue, "New Button Created");
            }
            else if (button.objectReferenceValue.hideFlags == HideFlags.HideInHierarchy)
            {
                GameObject buttonGO = (GameObject)button.objectReferenceValue;
                Undo.RegisterCompleteObjectUndo (buttonGO, "Button status changed");
                buttonGO.hideFlags = HideFlags.None;
                buttonGO.tag = "Untagged";
                buttonGO.SetActive (true);
            }
        }

        private void RemoveButtons ()
        {
            if (backButton.objectReferenceValue != null)
            {
                GameObject backButtonGO = (GameObject)backButton.objectReferenceValue;
                Undo.RegisterCompleteObjectUndo (backButtonGO, "Back button removed");
                backButtonGO.hideFlags = HideFlags.HideInHierarchy;
                backButtonGO.tag = "EditorOnly";
                backButtonGO.SetActive (false);
            }

            if (forwardButton.objectReferenceValue != null)
            {
                GameObject forwardButtonGO = (GameObject)forwardButton.objectReferenceValue;
                Undo.RegisterCompleteObjectUndo (forwardButtonGO, "Forward button removed");
                forwardButtonGO.hideFlags = HideFlags.HideInHierarchy;
                forwardButtonGO.tag = "EditorOnly";
                forwardButtonGO.SetActive (false);
            }
        }

        private void CreateIndexTable ()
        {
            //Debug.Log ("Creating Index table");
            var idxTable = myTarget.gameObject.GetComponentInChildren (typeof (IndexTableManager), true);
            GameObject indexTableGO = null;

            if (idxTable != null)
                indexTable.objectReferenceValue = idxTable.gameObject;

            if (indexTable.objectReferenceValue == null)
            {
                indexTable.objectReferenceValue = new GameObject ("Index Table");
                indexTableGO = indexTable.objectReferenceValue as GameObject;
                indexTableGO.transform.SetParent (myTarget.transform, false);
                indexTableGO.layer = 5;

                RectTransform indexTableRT = indexTableGO.AddComponent<RectTransform> ();
                indexTableRT.anchorMin = new Vector2 (0.5f, 0);
                indexTableRT.anchorMax = indexTableRT.anchorMin;
                indexTableRT.anchoredPosition = new Vector2 (0f, myTarget.viewport.anchoredPosition.y / 2 + 25f);
                indexTableGO.AddComponent<CanvasRenderer> ();

                //serializedObject.ApplyModifiedProperties ();
                indexTableManager.objectReferenceValue = indexTableGO.AddComponent<IndexTableManager> ();
                IndexTableManager idxTableManager = indexTableManager.objectReferenceValue as IndexTableManager;
                idxTableManager.MoveIndicator (0, invertOrder.boolValue);
                Undo.RegisterCreatedObjectUndo (indexTable.objectReferenceValue, "New Index Table Created");
            }
            else if (indexTable.objectReferenceValue.hideFlags == HideFlags.HideInHierarchy)
            {
                indexTableGO = (GameObject)indexTable.objectReferenceValue;
                Undo.RegisterCompleteObjectUndo (indexTableGO, "Index Table is now active");
                indexTableGO.hideFlags = HideFlags.None;
                indexTableGO.tag = "Untagged";
                indexTableGO.SetActive (true);
            }

            //GameObject scrollViewPrefab = PrefabUtility.GetCorrespondingObjectFromSource (myTarget.gameObject);
            ////Debug.Log(scrollViewPrefab, scrollViewPrefab);
            //if (scrollViewPrefab != null && indexTableGO != null && PrefabUtility.GetCorrespondingObjectFromSource(indexTableGO) == null)
            //    PrefabUtility.ApplyAddedGameObject(indexTableGO, AssetDatabase.GetAssetPath(scrollViewPrefab), InteractionMode.UserAction);
        }

        private void RemoveIndexTable ()
        {
            if (indexTable.objectReferenceValue != null)
            {
                GameObject indexTableGO = (GameObject)indexTable.objectReferenceValue;
                Undo.RegisterCompleteObjectUndo (indexTableGO, "Index Table removed");
                indexTableGO.hideFlags = HideFlags.HideInHierarchy;
                indexTableGO.tag = "EditorOnly";
                indexTableGO.SetActive (false);
            }
        }

        #endregion
    }
}