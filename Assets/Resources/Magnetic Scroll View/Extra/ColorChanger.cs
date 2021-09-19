using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MagneticScrollView{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MagneticScrollRect))]
	public class ColorChanger : MonoBehaviour {

		public Color originalColor;
		private MagneticScrollRect mgScrollRect;
		private Image[] elementsImage;
		void Awake (){
			mgScrollRect = GetComponent(typeof(MagneticScrollRect)) as MagneticScrollRect;
		}

		void Start (){
			if(mgScrollRect == null)
				return;
			elementsImage = new Image[mgScrollRect.Elements.Length];
			for (int i = 0; i < elementsImage.Length; i++){
				elementsImage[i] = mgScrollRect.Elements[i].GetComponent(typeof(Image)) as Image;
			}

			ChangeColors();
		}

		public void ChangeColors(){
			if(mgScrollRect == null)
				return;
			for (int i = 0; i < elementsImage.Length; i++){
				// int index = mgScrollRect.CurrentSelectedIndex;
				float normalAng = mgScrollRect.GetNormalizedElementAngle(i, false);
				float hue, sat, val;
				Color.RGBToHSV(originalColor, out hue, out sat, out val);
				Color newColor = Color.HSVToRGB(Mathf.Repeat(hue + normalAng - mgScrollRect.NormalizedScrollAngle, 1), sat, val, false);
				newColor.a = originalColor.a;
				elementsImage[i].color = newColor;
			}
		}
	}
}