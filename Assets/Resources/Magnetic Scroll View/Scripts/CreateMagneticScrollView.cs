//Magnetic Scroll View

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MagneticScrollView
{
    public class CreateMagneticScrollView
    {
#region Constants
        private const string bgSpritePath = "UI/Skin/Background.psd";
        private const string UIMaskPath = "UI/Skin/UIMask.psd";
        private const string knobPath = "UI/Skin/Knob.psd";
#endregion

        [MenuItem ("GameObject/UI/Magnetic Scroll View", false, 2063)]
        private static void CreateScrollViewObject ()
        {
            EventSystem evtSys = Object.FindObjectOfType<EventSystem> ();
            if (evtSys == null)
                new GameObject ("Event System", typeof (EventSystem), typeof (StandaloneInputModule));

            //GameObject selection = Selection.activeGameObject;
            GameObject canvasObj = (Object.FindObjectOfType<Canvas> ()) ? Object.FindObjectOfType<Canvas> ().gameObject : null;
            Camera camera = Object.FindObjectOfType<Camera> ();


            if (canvasObj == null)
            {
                canvasObj = new GameObject ("Canvas", typeof (Canvas));
                canvasObj.layer = 5;
                Canvas canvasComp = canvasObj.GetComponent<Canvas> ();

                CanvasScaler canvasScaler = canvasObj.GetComponent<CanvasScaler> ();
                if (canvasScaler == null)
                    canvasScaler = canvasObj.AddComponent<CanvasScaler> ();

                GraphicRaycaster raycaster = canvasObj.GetComponent<GraphicRaycaster> ();
                if (raycaster == null)
                    raycaster = canvasObj.AddComponent<GraphicRaycaster> ();

                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2 (800f, 480f);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;


                if (camera == null)
                {
                    canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                else
                {
                    canvasComp.renderMode = RenderMode.ScreenSpaceCamera;
                    canvasComp.worldCamera = Object.FindObjectOfType<Camera> ();
                }
            }            

            GameObject scrollView = new GameObject ("Magnetic Scroll View",
                typeof (RectTransform),
                typeof (CanvasRenderer),
                typeof (Image));


            scrollView.transform.SetParent (canvasObj.transform, false);
            scrollView.layer = 5;

            RectTransform scrollViewRT = scrollView.GetComponent<RectTransform> ();
            scrollViewRT.sizeDelta = new Vector2 (700f, 400f);

            Image scrollViewImage = scrollView.GetComponent<Image> ();
            scrollViewImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite> (bgSpritePath);
            scrollViewImage.color = new Color (1f, 1f, 1f, 0.5f);
            scrollViewImage.type = Image.Type.Sliced;

            GameObject viewport = new GameObject ("Viewport",
                typeof (RectTransform),
                typeof (Mask),
                typeof (CanvasRenderer),
                typeof (Image));


            viewport.transform.SetParent (scrollView.transform, false);
            viewport.layer = 5;

            RectTransform viewportRT = viewport.GetComponent<RectTransform> ();
            viewportRT.sizeDelta = new Vector2 (500f, 300f);

            Mask viewportMask = viewport.GetComponent<Mask> ();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.GetComponent<Image> ();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite> (UIMaskPath);
            viewportImage.type = Image.Type.Sliced;

            int elementNumber = 3;
            for (int i = 0; i < elementNumber; i++)
            {
                GameObject newElement = new GameObject ();
                newElement.name = "Panel " + (i + 1).ToString ();
                RectTransform newElementRT = newElement.AddComponent<RectTransform> ();

                newElement.transform.SetParent (viewport.transform, false);
                newElement.layer = 5;

                newElementRT.anchorMin = new Vector2 (0.5f, 0.5f);
                newElementRT.anchorMax = new Vector2 (0.5f, 0.5f);

                newElementRT.sizeDelta = new Vector2 (400f, 250f);

                newElement.AddComponent<CanvasRenderer> ();

                Image newElementImage = newElement.AddComponent<Image> ();
                newElementImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite> (bgSpritePath);
                newElementImage.color = Color.HSVToRGB (0.3f * i, 1f, 1f);
                newElementImage.type = Image.Type.Sliced;
            }

            MagneticScrollRect magneticScrollRect = scrollView.AddComponent<MagneticScrollRect> ();
            magneticScrollRect.viewport = viewportRT;
        }
    }
}
#endif