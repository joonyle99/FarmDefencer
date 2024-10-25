using UnityEngine;

namespace JoonyleGameDevKit
{
    public static class Mathematics
    {
        // EaseIn
        public static float EaseInQuad(float t) => t * t;
        public static float EaseInCubic(float t) => t * t * t;
        public static float EaseInQuart(float t) => t * t * t * t;
        public static float EaseInExpo(float t) => t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
        public static float EaseInCirc(float t) => 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));

        // EaseOut
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        public static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        public static float EaseOutQuart(float t) => 1 - Mathf.Pow(1 - t, 4);
        public static float EaseOutExpo(float t) => System.Math.Abs(t - 1) < float.Epsilon ? 1 : 1 - Mathf.Pow(2, -10 * t);
        public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
    }
}