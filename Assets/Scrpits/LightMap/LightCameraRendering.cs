using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    [RequireComponent(typeof(Camera))]
    public class LightCameraRendering : MonoBehaviour
    {
        private float _near;
        private float _feildOfView;
        private Matrix4x4 _w2c;

        public Vector3 Posistion { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector3 Forward { get; private set; }
        public void Init()
        {
            Camera camera = GetComponent<Camera>();
            _feildOfView = camera.fieldOfView;
            _near = camera.nearClipPlane;

            Width = camera.pixelWidth;
            Height = camera.pixelHeight;
            Posistion = transform.position;

            float heightLenght = Mathf.Tan(_feildOfView * Mathf.Deg2Rad * 0.5f) * _near;
            Vector3 xAsix = camera.transform.right * heightLenght * Width / Height;
            Vector3 yAsix = camera.transform.up * heightLenght;
            Vector3 zAsix = camera.transform.forward;
            Forward = zAsix;
            Matrix4x4 matr = new Matrix4x4(xAsix, yAsix, zAsix, new Vector4(0, 0, 0, 1));
            _w2c = matr.inverse;
        }

        private Vector2 TransformVector(Vector3 point)
        {
            Vector3 vp = _w2c.MultiplyVector(point - Posistion);
            float distP = vp.z / _near;

            float x = (vp.x / distP + 1) * 0.5f * Width;
            float y = (vp.y / distP + 1) * 0.5f * Height;
            return new Vector2(x, y);
        }

        private Vector3 InverserTransformVector(int x, int y)
        {
            float dx = x / (float)Width * 2 - 1;
            float dy = y / (float)Height * 2 - 1;
            float dz = _near;
            return new Vector3(dx, dy, dz);
        }

        public RenderData Render(List<TriangleMap> triangles)
        {
            RenderData data = RenderData.Apply();
            data.Init(Width, Height, new Color(0.1f, 0.1f, 0.3f));

            foreach (TriangleMap triangleMap in triangles)
            {
                //if (Vector3.Dot(triangleMap.SrcTriangle.WeightNormal, triangleMap.SrcTriangle.WeightPosition - Posistion) > 0)
                //{
                //    continue;
                //}
                Vector2 pa = TransformVector(triangleMap.SrcTriangle.PointA);
                Vector2 pb = TransformVector(triangleMap.SrcTriangle.PointB);
                Vector2 pc = TransformVector(triangleMap.SrcTriangle.PointC);

                RectInt border = RenderHelper.GetBorder(Width, Height, pa, pb, pc);
                Vector3 posOffset = triangleMap.SrcTriangle.InverseTransformPoint(Posistion);
                Matrix4x4 c2l = triangleMap.SrcTriangle.World2LocalMatr * _w2c.inverse;

                for (int x = border.xMin; x <= border.xMax; ++x)
                {
                    for (int y = border.yMin; y <= border.yMax; ++y)
                    {
                        Vector3 cameraPoint = InverserTransformVector(x, y);
                        Vector3 dire = c2l.MultiplyVector(cameraPoint);
                        Vector3 triangleLocalPoint = posOffset;
                        Vector3 castPoint = triangleLocalPoint - dire * triangleLocalPoint.z / dire.z;
                        Vector3 weight = triangleMap.GetWeightWidthInnerPoint(castPoint);
                        if (RenderHelper.Saturate(weight.x) && RenderHelper.Saturate(weight.y) && RenderHelper.Saturate(weight.z))
                        {
                            Vector2Int pos = triangleMap.GetInnerPosStable(weight.y, weight.z);
                            Vector3 point = triangleMap.SrcTriangle.GetWeightPoint(weight.x, weight.y, weight.z);
                            Color color = triangleMap.Map[pos.x, pos.y];
                            float deep = Vector3.Distance(point, Posistion);

                            if (data.DeepTex[x, y] > deep)
                            {
                                data.RenderTex[x, y] = color;
                                data.DeepTex[x, y] = deep;
                            }
                        }
                    }
                }
            }

            return data;
        }

        private static string ShowDebug(int x, int y, TriangleMap tri, float w1, float w2, float w3)
        {
            return $"点({x}, {y})属于三角形{tri.SrcTriangle.Index} 权重:({w1:f4}, {w2:f4}, {w3:f4}) 的颜色居然是0";
        }
    }
}
