using System;
using UnityEngine;

namespace LightRef
{
    public static class VectorHelper
    {
        public readonly static Vector4 V4_ZERO = new Vector4(0, 0, 0, 1);
        public static Color ToColor(this Vector3 v)
        {
            Color c = new Color(v.x, v.y, v.z);
            return c;
        }
        public static Color ToColor(this Vector4 v)
        {
            Color c = new Color(v.x, v.y, v.z, v.w);
            return c;
        }

        public static Color ToNormalColor(this Vector3 v)
        {
            float r = (v.x + 1) / 2;
            float g = (v.y + 1) / 2;
            float b = (v.z + 1) / 2;
            return new Color(r, g, b);
        }

        public static Color ToNormalColor(this Vector4 v)
        {
            float r = (v.x + 1) / 2;
            float g = (v.y + 1) / 2;
            float b = (v.z + 1) / 2;
            float a = (v.w + 1) / 2;
            return new Color(r, g, b, a);
        }

        public static float SphereYCalculate(Vector3 v1, Vector3 v2)
        {
            float xD = v1.x - v2.x, yD = v1.y - v2.y, zD = v1.z - v2.z;
            float A = (v1 - v2).sqrMagnitude;
            float B = 2 * (v2.x * xD + v2.y * yD + v2.z * zD);
            float C = v2.sqrMagnitude;
            float k = v1.y - v2.y;
            float b = v2.y;

            float up = B * b - 2 * k * C;
            float down = k * B - 2 * b * A;

            if (down == 0)
            {
                return 0;
            }
            float x = up / down;
            return x;
        }

        public static float CalculateY(Vector3 v1, Vector3 v2)
        {
            float xD = v1.x - v2.x, zD = v1.z - v2.z;
            float A = xD * xD + zD * zD;
            float B = 2 * (xD * v1.x + zD * v1.z);

            return B / (2 * A);
        }

        public static string GetDetialDataFromVector3(this Vector3 v, string l = "f7")
        {
            return $"({v.x.ToString(l)}, {v.y.ToString(l)}, {v.z.ToString(l)})";
        }

        public static Vector2Int Clamp01(Vector2Int uvInt, int mainWidth, int mainHeight)
        {
            uvInt.x = Mathf.Clamp(uvInt.x, 0, mainWidth - 1);
            uvInt.y = Mathf.Clamp(uvInt.y, 0, mainHeight - 1);
            return uvInt;
        }
    }
}
