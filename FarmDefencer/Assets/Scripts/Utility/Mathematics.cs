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
        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return c3 * t * t * t - c1 * t * t;
        }

        // EaseOut
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        public static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
        public static float EaseOutQuart(float t) => 1 - Mathf.Pow(1 - t, 4);
        public static float EaseOutExpo(float t) => System.Math.Abs(t - 1) < float.Epsilon ? 1 : 1 - Mathf.Pow(2, -10 * t);
        public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
        }

        // Bezier
        public static Vector3 CalcBezier2Point(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);

            Vector3 c = Vector3.Lerp(a, b, t);

            return c;
        }
    }
}