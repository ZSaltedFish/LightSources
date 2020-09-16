using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public class ReflectionRenderType : IRenderType
    {
        private const float QUATE_ANGLE = 180f;
        private const float DEFAULT_NEAR = 1;

        private static ReflectionRenderType _ins;

        #region 计算用
        private ReflectionRenderTypeInitData _initData;
        private Vector3 _phoneReflectDire;
        private Vector3 _lightDire;
        private Vector3 _pos;
        private Triangle _sourceTriangle;
        private Color _srcColor;
        private float _lambertRefValue;
        private Color _texColor;

        private Matrix4x4 _w2c;
        #endregion

        public static ReflectionRenderType Instance
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new ReflectionRenderType();
                }
                return _ins;
            }
        }
        private ReflectionRenderType()
        {

        }
        private void CalculateReflectDire()
        {
            Vector3 nor = Vector3.Dot(_lightDire, _initData.Normal) * _initData.Normal;
            _phoneReflectDire = nor + _lightDire;
        }

        public Vector2Int InitRenderTypeData()
        {
            float angle2Pix = RenderingController.AnglePixel;

            int width = (int)(QUATE_ANGLE * angle2Pix);
            int height = (int)(QUATE_ANGLE * angle2Pix);
            return new Vector2Int(width, height);
        }

        private Vector3 InverseVector(float x, float y, int width, int height)
        {
            float xLerp = x / width, yLerp = y / height;
            return new Vector3(xLerp * 2 - 1, yLerp * 2 - 1, DEFAULT_NEAR);
        }

        public void Render(RenderData data)
        {
            List<Triangle> triangles = Cull();
            foreach (Triangle triangle in triangles)
            {
                Vector3 localPA = triangle.PointA - _pos;
                Vector3 localPB = triangle.PointB - _pos;
                Vector3 localPC = triangle.PointC - _pos;

                Vector2 pA = RenderHelper.SphereRenderTransform(_w2c.MultiplyVector(localPA));
                Vector2 pB = RenderHelper.SphereRenderTransform(_w2c.MultiplyVector(localPB));
                Vector2 pC = RenderHelper.SphereRenderTransform(_w2c.MultiplyVector(localPC));
                Vector2 yC = RenderHelper.YCalculate(localPA, localPB, localPC);

                RectInt rect = RenderHelper.ExGetBorder(data.Width - 1, data.Height - 1, pA, pB, pC, yC);
                for (int x = rect.xMin; x <= rect.xMax; ++x)
                {
                    for (int y = rect.yMin; y <= rect.yMax; ++y)
                    {
                        Vector3 dire = RenderHelper.SphereInverseTransform(x, y);
                        Vector3 weight = RenderHelper.RunTo(dire, 1000);
                        float w1 = 1 - weight.x - weight.y;
                        float w2 = weight.x;
                        float w3 = weight.y;

                        if (RenderHelper.Saturate(w1) && RenderHelper.Saturate(w2) && RenderHelper.Saturate(w3))
                        {
                            Vector3 point = _sourceTriangle.GetWeightPoint(w1, w2, w3);
                            float deep = (point - _pos).sqrMagnitude;
                            try
                            {
                                if (data.DeepTex[x, y] > deep)
                                {
                                    Color lamColor = _srcColor * _initData.LambertValue;
                                    Color vColor = RenderHelper.ColorFixFunction(_lambertRefValue, lamColor, _texColor);
                                    data.RenderTex[x, y] = vColor / data.PixCount;
                                    data.DeepTex[x, y] = deep;
                                    data.TriangleIndexes[x, y] = triangle.Index;
                                    data.LightDire[x, y] = dire;
                                    data.PointTex[x, y] = point;
                                    data.PointWeight[x, y] = new Vector3(w1, w2, w3);
                                }
                            }
                            catch (IndexOutOfRangeException err)
                            {
                                Debug.LogError($"({x}, {y})超越边界:({data.Height}, {data.Height}).\n{err}");
                            }
                        }
                    }
                }
            }

            RenderPhone(triangles, data);
        }

        private void RenderPhone(List<Triangle> list, RenderData data)
        {
            CastResult result = CastResult.NULL_VALUE;
            float sqrDist = float.MaxValue;
            bool isCast = false;
            Ray ray = new Ray(_pos, _phoneReflectDire);
            foreach (Triangle triangle in list)
            {
                if (RenderHelper.EasyTriangleTest(ray, triangle, out CastResult temp) && temp.CastDistance < sqrDist)
                {
                    isCast = true;
                    sqrDist = temp.CastDistance;
                    result = temp;
                }
            }

            if (isCast)
            {
                Vector2 vc = RenderHelper.SphereRenderTransform(_w2c.MultiplyVector(result.CastPoint - _pos));
                int x = (int)vc.x, y = (int)vc.y;
                try
                {
                    Color vColor = RenderHelper.ColorFixFunction(_lambertRefValue, _srcColor, _texColor) / data.PixCount;
                    data.RenderTex[x, y] += vColor;
                }
                catch (IndexOutOfRangeException err)
                {
                    Debug.LogError($"({x}, {y})超越边界:({data.Width}, {data.Width}).\n{err}");
                }
            }
        }

        private List<Triangle> Cull()
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (Triangle triangle in StaticRenderData.Triangles)
            {
                Vector3 normal = triangle.WeightNormal;
                if (Vector3.Dot(normal, _sourceTriangle.WeightNormal) < 0)
                {
                    triangles.Add(triangle);
                }
            }
            return triangles;
        }

        public void SourceInput(int x, int y, RenderData data, RenderTypeInitData initData)
        {
            _initData = initData as ReflectionRenderTypeInitData;
            Color lightColor = data.RenderTex[x, y];
            Vector3 lightDire = data.LightDire[x, y];
            _pos = data.PointTex[x, y];
            _sourceTriangle = StaticRenderData.Triangles[data.TriangleIndexes[x, y]];
            _srcColor = lightColor * (1 - _sourceTriangle.SourceObject.Absorbance);
            _lambertRefValue = _sourceTriangle.SourceObject.LambertReflectionValue;
            _lightDire = lightDire;
            _texColor = _sourceTriangle.GetWeightColor(data.PointWeight[x, y]);

            InitPrivateMatrix();
            CalculateReflectDire();
        }

        public void RenderInCamera(RenderData data, RenderObject camera)
        {
            Vector3 dire = camera.Posistion - _pos;
            Vector3 localDire = _w2c.MultiplyVector(camera.Posistion - _pos);
            Vector2 pos = RenderHelper.SphereRenderTransform(localDire);
            int x = (int)pos.x, y = (int)pos.y;

            if (x >= 0 && x < data.Width && y >= 0 && y < data.Height)
            {
                Color vColor = RenderHelper.ColorFixFunction(_lambertRefValue, _srcColor, _texColor) / data.PixCount;
                vColor.a = 1;
                data.RenderTex[x, y] = Color.white;
                data.DeepTex[x, y] = dire.sqrMagnitude;
                data.TriangleIndexes[x, y] = RenderHelper.CAMERA_TRIANGLE_VALUE;
                data.LightDire[x, y] = dire.normalized;
                data.CameraPointX = x;
                data.CameraPointY = y;
            }
        }

        private void InitPrivateMatrix()
        {
            Vector3 xAxis = (_sourceTriangle.PointA - _sourceTriangle.PointB).normalized;
            Vector3 yAxis = Vector3.Cross(xAxis, _sourceTriangle.WeightNormal) ;
            Matrix4x4 matr = new Matrix4x4(xAxis, yAxis, _sourceTriangle.WeightNormal, VectorHelper.V4_ZERO);

            _w2c = matr.inverse;
        }
    }
}
