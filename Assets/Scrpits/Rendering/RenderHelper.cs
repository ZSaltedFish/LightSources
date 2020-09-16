using System;
using UnityEngine;

namespace LightRef
{
    public static class RenderHelper
    {
        public const float COLOR_FLITER = 0.01f;
        public const int CAMERA_TRIANGLE_VALUE = -10;
        public const int INVALID_TRIANGLE_ID = -1;

        public static int Min(params int[] vs)
        {
            int value = vs[0];
            foreach (int v in vs)
            {
                if (v < value)
                {
                    value = v;
                }
            }
            return value;
        }

        public static int Max(params int[] vs)
        {
            int value = vs[0];
            foreach (int v in vs)
            {
                if (v > value)
                {
                    value = v;
                }
            }
            return value;
        }

        public static bool ColorFliter(Color color)
        {
            return !(color.r < COLOR_FLITER && color.g < COLOR_FLITER && color.b < COLOR_FLITER);
        }

        /// <summary>
        /// 判断参数在0到1之间
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="deviation">误差</param>
        /// <returns></returns>
        public static bool Saturate(float value, float deviation = 0.0012f)
        {
            bool down = value >= -deviation;
            bool up = value <= 1 + deviation;
            return down && up;
        }

        public static Vector3 RunTo(Vector3 v, float pow)
        {
            float x = (int)(v.x * pow) / pow;
            float y = (int)(v.y * pow) / pow;
            float z = (int)(v.z * pow) / pow;
            return new Vector3(x, y, z);
        }

        public static int ExMin(params float[] vs)
        {
            float value = vs[0];
            foreach (float v in vs)
            {
                if (v < value)
                {
                    value = v;
                }
            }
            return (int)value;
        }

        public static int ExMax(params float[] vs)
        {
            float value = float.MinValue;
            foreach (float v in vs)
            {
                if (v > value)
                {
                    value = v;
                }
            }
            return Mathf.FloorToInt(value);
        }

        public static RectInt GetBorder(int width, int height, Vector2 pointA, Vector2 pointB, Vector2 pointC)
        {
            int minX = Mathf.Clamp(ExMin(pointA.x, pointB.x, pointC.x), 0, width);
            int minY = Mathf.Clamp(ExMin(pointA.y, pointB.y, pointC.y), 0, height);

            int maxX = Mathf.Clamp(ExMax(pointA.x, pointB.x, pointC.x), 0, width);
            int maxY = Mathf.Clamp(ExMax(pointA.y, pointB.y, pointC.y), 0, height);

            RectInt rect = new RectInt(minX, minY, maxX - minX, maxY - minY);
            return rect;
        }

        public static RectInt ExGetBorder(int width, int height, Vector2 pointA, Vector2 pointB, Vector2 pointC, Vector2 yC)
        {
            int minX = Mathf.Clamp(ExMin(pointA.x, pointB.x, pointC.x), 0, width);
            int minY = Mathf.Clamp(ExMin(pointA.y, pointB.y, pointC.y, yC.y), 0, height);

            int maxX = Mathf.Clamp(ExMax(pointA.x, pointB.x, pointC.x), 0, width);
            int maxY = Mathf.Clamp(ExMax(pointA.y, pointB.y, pointC.y, yC.x), 0, height);

            RectInt rect = new RectInt(minX, minY, maxX - minX, maxY - minY);
            return rect;
        }

        public static bool EasyTriangleTest(Ray ray, Triangle triangle, out CastResult result)
        {
            result = CastResult.NULL_VALUE;
            if (Vector3.Dot(triangle.WeightNormal, ray.direction) > -0.00001f)
            {
                return false;
            }
            Vector3 localp = triangle.InverseTransformPoint(ray.origin);
            Vector3 dire = triangle.InverseTransformVector(ray.direction);
            Vector3 castPoint = localp - (dire * localp.z / dire.z);
            castPoint = TriangleMap.RoundToWidthMinSize(castPoint);
            Vector3 weight = TriangleMap.RoundToWidthMinSize(new Vector3(1 - castPoint.x - castPoint.y, castPoint.x, castPoint.y));

            float w1 = weight.x, w2 = weight.y, w3 = weight.z;
            if (Saturate(w1, 0) && Saturate(w2, 0) && Saturate(w3, 0))
            {
                result = new CastResult()
                {
                    CastPoint = triangle.GetWeightPoint(w1, w2, w3),
                    CastPointNormal = triangle.GetWeightNormal(w1, w2, w3).normalized,
                    CastTriangle = triangle,
                    WeightValue = new Vector3(w1, w2, w3),
                };
                result.CastDistance = (result.CastPoint - ray.origin).magnitude;
                return true;
            }
            return false;
        }

        public static Color ColorFixFunction(float lerp, Color lightColor, Color texColor)
        {
            return lightColor * lerp + texColor * lightColor * (1 - lerp);
        }

        /// <summary>
        /// 球体投影
        /// </summary>
        /// <param name="point">相对原点位置，自动单位化</param>
        /// <returns></returns>
        public static Vector2 SphereRenderTransform(Vector3 point, float anglePix = 1)
        {
            point = point.normalized;
            float altha;
            float theta = Mathf.Asin(point.y);
            float y = (theta * Mathf.Rad2Deg + 90) * anglePix;
            float distXOZ = Mathf.Cos(theta);
            if (distXOZ == 0)
            {
                altha = 0;
            }
            else
            {
                if (point.z < 0.0001f && point.z > -0.0001f)
                {
                    altha = point.x > 0 ? 0 : Mathf.PI;
                }
                else
                {
                    float delta = point.x / distXOZ;
                    altha = Mathf.Acos(delta);
                    if (point.z < 0)
                    {
                        altha = 2 * Mathf.PI - altha;
                    }
                }
            }
            float x = altha * Mathf.Rad2Deg * anglePix;
            //if (x > 10000)
            //{
            //    int i = 0;
            //}
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// 球形单位换算
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>返回朝向向量</returns>
        public static Vector3 SphereInverseTransform(int x, int y, float pixAngle = 1)
        {
            float r2d = Mathf.Rad2Deg * pixAngle;
            float dgY = y / r2d - Mathf.PI * 0.5f;
            float dgX = x / r2d;

            float py = Mathf.Sin(dgY);
            float distXOZ = Mathf.Cos(dgY);
            float px = Mathf.Cos(dgX) * distXOZ;
            float pz = Mathf.Sin(dgX) * distXOZ;

            Vector3 dire = new Vector3(px, py, pz);
            return -dire;
        }

        public static Vector2 YCalculate(Vector3 v1, Vector3 v2, Vector3 v3, float pixAngle = 1)
        {
            float lerp1 = VectorHelper.SphereYCalculate(v1, v2);
            float lerp2 = VectorHelper.SphereYCalculate(v1, v3);
            float lerp3 = VectorHelper.SphereYCalculate(v2, v3);

            float y1 = Vector3.Lerp(v1, v2, lerp1).normalized.y;
            float y2 = Vector3.Lerp(v1, v3, lerp2).normalized.y;
            float y3 = Vector3.Lerp(v2, v3, lerp3).normalized.y;

            float max = Mathf.Asin(Mathf.Max(y1, y2, y3));
            float min = Mathf.Asin(Mathf.Min(y1, y2, y3));
            float yMax = (max * Mathf.Rad2Deg + 90) * pixAngle;
            float yMin = (min * Mathf.Rad2Deg + 90) * pixAngle;
            return new Vector2(yMax, yMin);
        }
    }
}