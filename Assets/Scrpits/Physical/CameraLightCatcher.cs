using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public class CameraLightCatcher : MonoBehaviour
    {
        public Camera SrcCamera;
        public Texture2D RenderTarget;
        public float Radius = 0.5f;

        private Matrix4x4 _privateMatrix;
        private Vector3 _offset;
        private static Vector4 V4_0 = new Vector4(0, 0, 0, 1);
        private static Vector3 _forward, _position;

        public int Height, Width;
        private Dictionary<Vector2Int, Color> _kvColor;

        public void InitPrivate()
        {
            Vector3 zero = SrcCamera.transform.position - SrcCamera.transform.forward * SrcCamera.nearClipPlane;
            float angleX = SrcCamera.fieldOfView * Width / Height;
            float xLenght = Mathf.Tan(angleX / 2 * Mathf.Deg2Rad) * 2 * SrcCamera.nearClipPlane / 2;
            Vector3 xAxis = SrcCamera.transform.right * xLenght;
            Vector3 yAxis = SrcCamera.transform.up * xLenght * Height / Width;
            Vector3 zAxis = -SrcCamera.transform.forward;

            Matrix4x4 matr = new Matrix4x4(xAxis, yAxis, zAxis, V4_0);
            _privateMatrix = matr.inverse;
            _offset = _privateMatrix.MultiplyVector(-zero);
            _forward = SrcCamera.transform.forward;
            _position = SrcCamera.transform.position;

            _kvColor = new Dictionary<Vector2Int, Color>();
        }

        public bool Cast(Ray light, float atte, Color color)
        {
            if (Vector3.Dot(light.direction, _forward) > 0)
            {
                return false;
            }

            Vector3 p = _position - light.origin;
            float cos = Vector3.Dot(p.normalized, light.direction);
            if (cos < 0)
            {
                return false;
            }
            float dist = p.magnitude * Mathf.Sqrt(1 - cos * cos);
            if (dist > Radius)
            {
                return false;
            }
            Ray localRay = _privateMatrix.MultiplyRay(_offset, light);
            float distance = localRay.origin.z / localRay.direction.z;
            Vector3 cp = distance * localRay.direction + localRay.origin;
            if (localRay.direction.z == 0)
            {
                return false;
            }
            if (cp.x > 1 || cp.x < -1 || cp.y > 1 || cp.y < -1)
            {
                return false;
            }
            cp = (cp + Vector3.one) * .5f;
            float atteValue = atte * distance;
            Color newColor = new Color(Mathf.Clamp01(color.r - atteValue), Mathf.Clamp01(color.g - atteValue), Mathf.Clamp01(color.b - atteValue), 1);
            int width = (int)(cp.x * Width);
            int height = (int)(cp.y * Height);

            RefreshColorData(width, height, newColor);
            return true;
        }

        private void RefreshColorData(int width, int height, Color newColor)
        {
            Vector2Int v = new Vector2Int(width, height);
            Color setColor;
            if (_kvColor.TryGetValue(v, out Color color))
            {
                setColor = color + newColor;
            }
            else
            {
                setColor = newColor;
            }
            _kvColor[v] = setColor;
        }

        public void ApplyTexture()
        {
            for (int width = 0; width < Width; width++)
            {
                for (int height = 0; height < Height; height++)
                {
                    Vector2Int v = new Vector2Int(width, height);
                    Color setColor;
                    if (_kvColor.TryGetValue(v, out Color color))
                    {
                        setColor = color;
                        setColor.a = 1;
                    }
                    else
                    {
                        setColor = new Color(0, 0, 0, 0);
                    }
                    RenderTarget.SetPixel(width, height, setColor);
                }
            }
            RenderTarget.Apply();
        }
    }
}
