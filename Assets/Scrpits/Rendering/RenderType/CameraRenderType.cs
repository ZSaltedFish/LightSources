using UnityEngine;

namespace LightRef
{
    public class CameraRenderType : IRenderType
    {
        private CameraRenderTypeInitData _initData;
        private Matrix4x4 _w2c;
        private int _width;
        private int _height;
        private int _pixX;
        private int _pixY;
        private Color _srcColor;

        public void CameraInit(CameraRenderTypeInitData camera)
        {
            float fieldOfView = camera.Camera.fieldOfView;

            float lengthAngle = fieldOfView * 16 / 9;
            _width = (int)(lengthAngle * RenderingController.AnglePixel);
            _height = (int)(fieldOfView * RenderingController.AnglePixel);

            float heightLength = Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f) * camera.Near;
            Vector3 xV = camera.Camera.transform.right * heightLength * 16 / 9;
            Vector3 yV = camera.Camera.transform.up * heightLength;
            Vector3 zV = camera.Camera.transform.forward;
            Matrix4x4 matr = new Matrix4x4(xV, yV, zV, new Vector4(0, 0, 0, 1));
            _w2c = matr.inverse;
        }

        public Vector2Int InitRenderTypeData()
        {
            return new Vector2Int(_width, _height);
        }

        private Vector2 TransformVector(Vector3 point)
        {
            Vector3 vp = _w2c.MultiplyVector(point - _initData.Posistion);
            float distP = vp.z / _initData.Near;

            float x = (vp.x / distP + 1) * 0.5f * _width;
            float y = (vp.y / distP + 1) * 0.5f * _height;
            return new Vector2(x, y);
        }

        //public void Render(RenderData data)
        //{
        //    List<Triangle> triangles = Cull();

        //    foreach (Triangle triangle in triangles)
        //    {
        //        Vector2 pa = TransformVector(triangle.PointA);
        //        Vector2 pb = TransformVector(triangle.PointB);
        //        Vector2 pc = TransformVector(triangle.PointC);

        //        // 逐像素渲染
        //        int minX = Mathf.Clamp(RenderHelper.ExMin(pa.x, pb.x, pc.x), 0, _width);
        //        int minY = Mathf.Clamp(RenderHelper.ExMin(pa.y, pb.y, pc.y), 0, _height);

        //        int maxX = Mathf.Clamp(RenderHelper.ExMax(pa.x, pb.x, pc.x), 0, _width);
        //        int maxY = Mathf.Clamp(RenderHelper.ExMax(pa.y, pb.y, pc.y), 0, _height);

        //        Vector3 posOffset = triangle.InverseTransformPoint(_position);
        //        Matrix4x4 c2l = triangle.World2LocalMatr * _w2c.inverse;
        //        for (int x = minX; x < maxX + 1; ++x)
        //        {
        //            for (int y = minY; y < maxY + 1; ++y)
        //            {
        //                //换算成三角形内部点
        //                Vector3 cameraPoint = InverseVector(x, y);
        //                Vector3 dire = c2l.MultiplyVector(cameraPoint);
        //                Vector3 triangleLocalPoint = posOffset;
        //                Vector3 castPoint = RenderHelper.RunTo(triangleLocalPoint - dire * triangleLocalPoint.z / dire.z, 1000);

        //                float w1 = 1 - castPoint.x - castPoint.y;
        //                float w2 = castPoint.x;
        //                float w3 = castPoint.y;

        //                if (RenderHelper.Saturate(w1) && RenderHelper.Saturate(w2) && RenderHelper.Saturate(w3))
        //                {
        //                    Vector3 point = triangle.GetWeightPoint(w1, w2, w3);
        //                    Vector3 normal = triangle.GetWeightNormal(w1, w2, w3).normalized;
        //                    Color color = triangle.GetWeightColor(w1, w2, w3);

        //                    float deep = (point - _position).sqrMagnitude;

        //                    try
        //                    {
        //                        if (data.DeepTex[x, y] > deep)
        //                        {
        //                            Color vColor = (0.5f + Mathf.Clamp01(Vector3.Dot(normal, -_testLight)) * 0.5f) * color;
        //                            data.RenderTex[x, y] = vColor;
        //                            data.DeepTex[x, y] = deep;
        //                        }
        //                    }
        //                    catch (IndexOutOfRangeException err)
        //                    {
        //                        Debug.LogError($"({x}, {y})超越边界:({_width}, {_height}).\n{err}");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private Vector3 InverseVector(float x, float y)
        //{
        //    float xlerp = x / _width;
        //    float ylerp = y / _height;

        //    return new Vector3(xlerp * 2 - 1, ylerp * 2 - 1, _near);
        //}

        //private List<Triangle> Cull()
        //{
        //    List<Triangle> triangles = new List<Triangle>();

        //    foreach (Triangle triangle in StaticRenderData.Triangles)
        //    {
        //        Vector3 p = _forward;
        //        if (Vector3.Dot(p, triangle.WeightNormal) < 0.03f)
        //        {
        //            triangles.Add(triangle);
        //        }
        //    }
        //    return triangles;
        //}

        public void SourceInput(int x, int y, RenderData data, RenderTypeInitData initData)
        {
            _initData = initData as CameraRenderTypeInitData;

            Vector3 p = _initData.Posistion - data.LightDire[x, y] * Mathf.Sqrt(data.DeepTex[x, y]);
            Vector2 off = TransformVector(p);

            _pixX = (int)off.x;
            _pixY = (int)off.y;
            _srcColor = data.RenderTex[x, y];
        }

        public void Render(RenderData data)
        {
            if (IsOn(_pixX, _pixY))
            {
                Color color = _srcColor;
                Color oldColor = data.RenderTex[_pixX, _pixY] + color;
                oldColor.a = 1;
                data.RenderTex[_pixX, _pixY] = oldColor;
            }
        }

        private bool IsOn(int x, int y)
        {
            if (x < 0 || x >= _width)
            {
                return false;
            }

            if (y < 0 || y >= _height)
            {
                return false;
            }

            return true;
        }

        public void RenderInCamera(RenderData data, RenderObject type)
        {
            //CameraType will do nothing in this method.
        }
    }
}
