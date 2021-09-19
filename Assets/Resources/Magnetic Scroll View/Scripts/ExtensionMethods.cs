using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagneticScrollView
{
    public enum Sign
    {
        Positive = 1,
        Negative = -1
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Checks if this member is equal to at least one of the possibilities.
        /// </summary>
        /// <param name="possibles">The parameters used to compare.</param>
        public static bool IsEqual<T> (this T @this, params T [] possibles)
        {
            return possibles.Contains (@this);
        }        

        public static int Nearest (this float @this, float [] values, Sign direction)
        {
            List <float> distances = new List<float> ();
            List <float> Positive = new List <float> ();
            List <float> Negative = new List <float> ();
            float lowerDist = 0;
            int index = 0;
            //Debug.Log (direction);            

            for (int i = 0; i < values.Length; i++)
            {
                distances.Add (values [i] - @this);

                if (distances [i] >= 0f)
                    Positive.Add (distances [i]);
                else if (distances [i] < 0f)
                    Negative.Add (distances [i]);

                //Debug.Log (distances [i]);
            }

            if ((direction == Sign.Positive || Positive.Count == 0 ) && Negative.Count > 0)
                lowerDist = Negative.Max ();
            else
                lowerDist = Positive.Min ();

            index = distances.IndexOf (lowerDist);
            //Debug.Log (lowerDist);
            return (int)Mathf.Repeat(index, values.Length);
        }

        public static float NearestAbsolute (this float @this, float [] values, out int index)
        {            
            float [] distances = new float [values.Length];
            
            for (int i = 0; i < distances.Length; i++)
            {
                distances [i] = Mathf.Abs(values [i] - @this);
            }

            index = Array.IndexOf (distances, distances.Min ());
            return distances [index];
        }

        public static bool isApproximately(this float @this, float [] values, float tolerance, out int index)
        {
            for (int i = 0; i < values.Length; i++)
            {
                //Debug.Log ("index: " + i);
                if (Mathf.Abs (@this - values [i]) < tolerance)
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }

        public static bool isApproximately (this float[] @this, float value, float tolerance, out int index)
        {
            for (int i = 0; i < @this.Length; i++)
            {
                if (Mathf.Abs (value - @this [i]) < tolerance)
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }


        /// <summary>
        /// Checks if this member is different from all the possibilities.
        /// </summary>
        /// <param name="possibles">The parameters used to compare.</param>
        public static bool IsDifferent<T> (this T @this, params T [] possibles)
        {
            return !possibles.Contains (@this);
        }        
    }

    public class MyMath
    {
        public static float EnhancedRepeat (float value, float maxValue)
        {
            float dir = Mathf.Sign (value);
            value = (value + dir * maxValue) % (maxValue * 2) - (dir * maxValue);
            return value;
        }

        public static float AngleFromLength (float leg1, float hypotenuse)
        {
            float leg2 = -Mathf.Sqrt (Mathf.Pow (hypotenuse, 2) + Mathf.Pow (leg1, 2));
            float angle = Mathf.Atan2 (leg2, leg1) * Mathf.Rad2Deg + 90;
            return angle;
        }
    }    
}

