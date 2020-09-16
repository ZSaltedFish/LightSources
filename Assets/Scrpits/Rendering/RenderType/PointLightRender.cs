using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public class PointLightRender : IRenderType
    {
        public static Vector3 START_POINT, ENDING_POINT;
        private PointLightRenderInitData _initData;
        private static PointLightRender _instance;
        public static PointLightRender Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PointLightRender();
                }
                return _instance;
            }
        }

        public void Render(RenderData data)
        {
            List<Triangle> triangles = Cull();
            Ray ray = new Ray();
            foreach (Triangle triangle in triangles)
            {
                Vector3 direA = triangle.PointA - _initData.LightPoint;
                Vector3 direB = triangle.PointB - _initData.LightPoint;
                Vector3 direC = triangle.PointC - _initData.LightPoint;

                Vector2 pA = RenderHelper.SphereRenderTransform(direA);
                Vector2 pB = RenderHelper.SphereRenderTransform(direB);
                Vector2 pC = RenderHelper.SphereRenderTransform(direC);
                Vector2 yC = RenderHelper.YCalculate(direA, direB, direC);
                RectInt rect = RenderHelper.ExGetBorder(data.Width - 1, data.Height - 1, pA, pB, pC, yC);

                for (int x = rect.xMin; x <= rect.xMax; ++x)
                {
                    for (int y = rect.yMin; y <= rect.yMax; ++y)
                    {
                        Vector3 dire = RenderHelper.SphereInverseTransform(x, y);
                        ray.origin = _initData.LightPoint;
                        ray.direction = dire;

                        if (RenderHelper.EasyTriangleTest(ray, triangle, out CastResult result))
                        {
                            float deep = result.CastDistance;
                            try
                            {
                                if (data.DeepTex[x, y] > deep)
                                {
                                    float pow = _initData.LightPower;
                                    Color vColor = _initData.LightColor;
                                    data.RenderTex[x, y] = vColor * pow / data.PixCount;
                                    data.DeepTex[x, y] = deep;
                                    data.TriangleIndexes[x, y] = triangle.Index;
                                    data.LightDire[x, y] = dire;
                                    data.PointTex[x, y] = result.CastPoint;
                                    data.PointWeight[x, y] = result.WeightValue;
                                }
                            }
                            catch (IndexOutOfRangeException err)
                            {
                                Debug.LogError($"({x}, {y})超越边界:({data.Width}, {data.Height}).\n{err}");
                            }
                        }
                    }
                }
            }
        }

        public Vector2Int InitRenderTypeData()
        {
            float angle2Pix = RenderingController.AnglePixel;

            int width = (int)(360 * angle2Pix);
            int height = (int)(180 * angle2Pix);

            return new Vector2Int(width, height);
        }

        private List<Triangle> Cull()
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (Triangle triangle in StaticRenderData.Triangles)
            {
                Vector3 p = triangle.WeightPosition - _initData.LightPoint;
                if (Vector3.Dot(p, triangle.WeightNormal) < 0)
                {
                    triangles.Add(triangle);
                }
            }
            return triangles;
        }

        public void SourceInput(int x, int y, RenderData data, RenderTypeInitData initData)
        {
            _initData = initData as PointLightRenderInitData;
        }

        public void RenderInCamera(RenderData data, RenderObject camera)
        {
            Vector3 dire = camera.Posistion - _initData.LightPoint;
            Vector2 pos = RenderHelper.SphereRenderTransform(dire);
            int x = (int)pos.x, y = (int)pos.y;

            Color color = _initData.LightColor * _initData.LightPower / data.PixCount;
            color.a = 1;
            data.RenderTex[x, y] = color;
            data.DeepTex[x, y] = dire.sqrMagnitude;
            data.TriangleIndexes[x, y] = RenderHelper.CAMERA_TRIANGLE_VALUE;
            data.LightDire[x, y] = dire;
            data.CameraPointX = x;
            data.CameraPointY = y;
        }
    }
}
