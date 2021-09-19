using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

namespace MagneticScrollView
{
    [CustomEditor (typeof (SwipeDetection))]
    [CanEditMultipleObjects]
    public class SwipeDetectionEditor : Editor
    {
        //SerializedProperty viewport;
        SerializedProperty swipeEventList;
        SerializedProperty minTime;
        SerializedProperty maxTime;

        GUIContent buttonContent;
        GUIContent [] eventTypes;
        GUIContent eventIDName;
        GUIContent iconToolbarMinus;

        //SwipeDetection myTarget;

        float minT, maxT;

        protected virtual void OnEnable ()
        {
            //myTarget = (SwipeDetection)target;

            //viewport = serializedObject.FindProperty ("viewport");
            swipeEventList = serializedObject.FindProperty ("swipeEvents");
            minTime = serializedObject.FindProperty ("minTime");
            maxTime = serializedObject.FindProperty ("maxTime");
            minT = minTime.floatValue;
            maxT = maxTime.floatValue;

            buttonContent = new GUIContent ("Add New Swipe Event Type");
            iconToolbarMinus = new GUIContent (EditorGUIUtility.IconContent ("Toolbar Minus"));
            iconToolbarMinus.tooltip = "Remove This Event";
            eventIDName = new GUIContent ("");
            string [] menuNames = Enum.GetNames (typeof (Swipe));
            eventTypes = new GUIContent [menuNames.Length];
            for (int i = 0; i < eventTypes.Length; i++)
            {
                eventTypes [i] = new GUIContent (menuNames [i]);
            }
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            int toBeRemovedEntry = -1;

            //GUILayout.Space (4f);
            //EditorGUILayout.PropertyField (viewport);

            //if (viewport.objectReferenceValue == null)
            //    EditorGUILayout.HelpBox ("Viewport reference is missing. This GameObject's Rect will be used as viewport.", MessageType.Info);
            ////EditorGUILayout.PropertyField (minTime, new GUIContent ("Min Time (s)"));
            ////EditorGUILayout.PropertyField (maxTime, new GUIContent ("Max Time (s)"));            
            //GUILayout.Space (4f);
            EditorGUILayout.BeginHorizontal ();

            EditorGUIUtility.labelWidth = 120f;

            EditorGUILayout.PrefixLabel (new GUIContent ("Min-Max Time (s)"));
            minT = Mathf.Clamp (EditorGUILayout.FloatField (minT, GUILayout.MaxWidth (38f)), 0.001f, maxT);
            EditorGUILayout.MinMaxSlider (ref minT, ref maxT, 0.001f, 1f);
            maxT = Mathf.Clamp (EditorGUILayout.FloatField (maxT, GUILayout.MaxWidth (38f)), 0.001f, 1f);

            minTime.floatValue = minT;
            maxTime.floatValue = maxT;
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.Space ();
            Vector2 iconSize = GUIStyle.none.CalcSize (iconToolbarMinus);
            for (int i = 0; i < swipeEventList.arraySize; i++)
            {
                SerializedProperty swipeEvent = swipeEventList.GetArrayElementAtIndex (i);
                SerializedProperty callback = swipeEvent.FindPropertyRelative ("callback");
                SerializedProperty eventID = swipeEvent.FindPropertyRelative ("eventID");
                eventIDName.text = "Swipe " + eventID.enumDisplayNames [eventID.enumValueIndex];
                EditorGUILayout.PropertyField (callback, eventIDName, new GUILayoutOption [0]);

                Rect lastRect = GUILayoutUtility.GetLastRect ();
                Rect minusRect = new Rect ((lastRect.xMax - iconSize.x) - 8f, lastRect.y + 1f, iconSize.x, iconSize.y);
                if (GUI.Button (minusRect, iconToolbarMinus, GUIStyle.none))
                {
                    toBeRemovedEntry = i;
                }
                EditorGUILayout.Space ();
            }

            if (toBeRemovedEntry > -1)
            {
                RemoveEntry (toBeRemovedEntry);
            }

            Rect position = GUILayoutUtility.GetRect (buttonContent, GUI.skin.button);

            position.x += (position.width - 200f) / 2f;
            position.width = 200f;

            if (GUI.Button (position, buttonContent))
            {
                ShowEventMenu ();
            }

            serializedObject.ApplyModifiedProperties ();
        }

        void RemoveEntry (int toBeRemovedEntry)
        {
            swipeEventList.DeleteArrayElementAtIndex (toBeRemovedEntry);
        }

        private void OnAddNewSelected (object index)
        {
            int num = (int)index;
            swipeEventList.arraySize++;
            swipeEventList.GetArrayElementAtIndex (swipeEventList.arraySize - 1).FindPropertyRelative ("eventID").enumValueIndex = num;
            serializedObject.ApplyModifiedProperties ();
        }

        void ShowEventMenu ()
        {
            GenericMenu menu = new GenericMenu ();
            for (int i = 0; i < eventTypes.Length; i++)
            {
                bool flag = true;
                for (int j = 0; j < swipeEventList.arraySize; j++)
                {
                    if (swipeEventList.GetArrayElementAtIndex (j).FindPropertyRelative ("eventID").enumValueIndex == i)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    menu.AddItem (eventTypes [i], false, new GenericMenu.MenuFunction2 (OnAddNewSelected), i);
                }
                else
                {
                    menu.AddDisabledItem (eventTypes [i]);
                }
            }
            menu.ShowAsContext ();
            Event.current.Use ();
        }
    }
}