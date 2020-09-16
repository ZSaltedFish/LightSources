using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public class Triangle : ICastObject
    {
        private static int TRIANGLE_COUNT = -1;
        public readonly Vector3 PointA, PointB, PointC;
        public readonly Vector2 UvA, UvB, UvC;
        public readonly int Index;

        private Matrix4x4 _privateMatr;
        private Vector3 _offset;
        private static Vector4 ZERO_V4 = new Vector4(0, 0, 0, 1);
        private readonly float _area;
        private int _mainWidth, _mainHeight, _nWight, _nHeight;

        public Vector3 WeightNormal { get; private set; }
        public Vector3 WeightPosition { get; private set; }

        private static readonly float VALUE_3 = 1f / 3f;
        public MaterialScript SourceObject { get; }
        public RenderObject SourceRenderObject { get { return SourceObject.SrcObject; } }

        public readonly Matrix4x4 PointATangent2World, PointBTangent2World, PointCTangent2World;
        public Matrix4x4 World2LocalMatr => _privateMatr;
        private string _name;
        public Triangle(TriangleInitData data)
        {
            PointA = data.Ve1; PointB = data.Ve2; PointC = data.Ve3;
            UvA = data.Uv1; UvB = data.Uv2; UvC = data.Uv3;
            SourceObject = data.Src;
            _mainWidth = SourceObject.MainTex.width;
            _mainHeight = SourceObject.MainTex.height;
            _nWight = SourceObject.NormalTex.width;
            _nHeight = SourceObject.NormalTex.height;

            PointATangent2World = InitMatrix(data.No1, data.Ta1, data.LocalTriax);
            PointBTangent2World = InitMatrix(data.No2, data.Ta2, data.LocalTriax);
            PointCTangent2World = InitMatrix(data.No3, data.Ta3, data.LocalTriax);

            WeightNormal = (data.LocalTriax.MultiplyVector(data.No1) * VALUE_3 + data.LocalTriax.MultiplyVector(data.No2) * VALUE_3 + data.LocalTriax.MultiplyVector(data.No3) * VALUE_3).normalized;
            WeightPosition = data.Ve1 * VALUE_3 + data.Ve2 * VALUE_3 + data.Ve3 * VALUE_3;

            GetPrivateMatr();
            Index = ++TRIANGLE_COUNT;
            _name = $"{SourceObject.name}的三角形.{Index}";
        }

        private void GetPrivateMatr()
        {
            Vector3 localB = PointB - PointA;
            Vector3 localC = PointC - PointA;
            Vector3 cross = WeightNormal;

            _privateMatr = new Matrix4x4(localB, localC, cross, ZERO_V4).inverse;
            _offset = -_privateMatr.MultiplyPoint(PointA);
        }

        /// <summary>
        /// Local2World
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 point)
        {
            return _privateMatr.inverse.MultiplyVector(point - _offset);
        }

        /// <summary>
        /// 转换向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public Vector3 TransformVector(Vector3 vector)
        {
            return _privateMatr.inverse.MultiplyVector(vector);
        }

        /// <summary>
        /// World2Local
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector3 InverseTransformPoint(Vector3 point)
        {
            return _privateMatr.MultiplyVector(point) + _offset;
        }

        public Vector3 InverseTransformVector(Vector3 vector)
        {
            return _privateMatr.MultiplyVector(vector);
        }

        public Matrix4x4 InitMatrix(Vector3 pN, Vector4 pT, Matrix4x4 l2w)
        {
            Vector3 normal = pN;
            Vector3 tangent = pT;
            Vector3 newVector = Vector3.Cross(normal, tangent) * pT.w;

            Matrix4x4 m2t = new Matrix4x4(tangent, newVector, normal, ZERO_V4);
            Matrix4x4 result = l2w * m2t.inverse;
            return result;
        }

        /// <summary>
        /// 射线碰撞测试
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="result">结果</param>
        /// <returns></returns>
        public bool Raycast(Ray ray, out CastResult result)
        {
            result = new CastResult();
            Vector3 localPoint = _privateMatr.MultiplyVector(ray.origin) + _offset;
            Vector3 localDirection = _privateMatr.MultiplyVector(ray.direction).normalized;

            Vector3 castPoint = localDirection * localPoint.z / -localDirection.z + localPoint;
            result.CastPoint = _privateMatr.inverse.MultiplyVector(castPoint - _offset);
            Vector3 wValue = castPoint;
            if (!IsZeroToOne(wValue.x) || !IsZeroToOne(wValue.y) || !IsZeroToOne(wValue.z) || localDirection.z * localPoint.z > 0)
            {
                return false;
            }

            Vector3 castNormal = GetWeightNormal(wValue.x, wValue.y, wValue.z);

            if (Vector3.Dot(castNormal, ray.direction) > 0)
            {
                return false;
            }
            result.CastPointNormal = castNormal.normalized;
            result.CastTriangle = this;
            result.WeightValue = wValue;
            result.CastDistance = Vector3.Magnitude(result.CastPoint - ray.origin);
            return true;
        }

        public Ray WorldRay2Local(Ray ray)
        {
            return _privateMatr.MultiplyRay(_offset, ray);
        }

        public Color GetWeightColor(Vector3 weightValue)
        {
            return GetWeightColor(weightValue.x, weightValue.y, weightValue.z);
        }

        public Color GetWeightColor(float a, float b, float c)
        {
            Color[,] mainTexTexture = SourceObject.MainTexDic;
            Vector2 uv = UvA * a + UvB * b + UvC * c;
            Vector2Int uvInt = GetV((int)(_mainWidth * uv.x), (int)(_mainHeight * uv.y), _mainWidth, _mainHeight);
            uvInt = VectorHelper.Clamp01(uvInt, _mainWidth, _mainHeight);
            try
            {
                return mainTexTexture[uvInt.x, uvInt.y];
            }
            catch (IndexOutOfRangeException err)
            {
                Debug.LogError($"日TM的瞎JB越界:{uvInt}/({_nWight}, {_nHeight}) weight:({a}, {b}, {c})");
                throw err;
            }
        }

        private bool IsZeroToOne(float p)
        {
            return p > -.01f && p < 1.01f;
        }

        private float Area(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float a = (v1 - v2).magnitude, b = (v1 - v3).magnitude, c = (v2 - v3).magnitude;
            float s = (a + b + c) * .5f;
            float sq = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
            return sq;
        }

        private float[] GetWeight(Vector3 p)
        {
            float[] ws = new float[3];

            ws[0] = 1 - p.x - p.y;
            ws[1] = p.x;
            ws[2] = p.y;
            return ws;
        }

        public Vector3 GetWeightNormal(float w1, float w2, float w3)
        {
            Color[,] normalTexture = SourceObject.NorTexDic;
            Vector2 uv = UvA * w1 + UvB * w2 + UvC * w3;
            Vector2Int vCv = GetV((int)(_nWight * uv.x), (int)(_nHeight * uv.y), _nWight, _nHeight);
            try
            {
                Vector3 tN = Color2Normal(normalTexture[vCv.x, vCv.y]);
                return PointATangent2World.MultiplyVector(tN);
            }
            catch (IndexOutOfRangeException err)
            {
                Debug.Log($"越界:{vCv}, ({_nWight}, {_nHeight})");
                throw err;
            }
        }

        public Vector3 GetWeightPoint(float w1, float w2, float w3)
        {
            return PointA * w1 + PointB * w2 + PointC * w3;
        }

        private Vector2Int GetV(int x, int y, int nWight, int nHeight)
        {
            x %= nWight;
            y %= nHeight;
            Vector2Int v = new Vector2Int(x, y);
            return v;
        }

        private static Vector3 Color2Normal(Color color)
        {
            Vector3 v = new Vector3(color.b, color.g, color.r);
            return v * 2 - Vector3.one;
        }

        public override string ToString()
        {
            return _name;
        }
        public static void ResetCounter()
        {
            TRIANGLE_COUNT = -1;
        }
    }
}
