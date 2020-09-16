using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

namespace LightRef
{
    /// <summary>
    /// 全球面渲染
    /// </summary>
    public class LightingRendering
    {
        public readonly static Color IGNORE_COLOR = new Color(0, 0, 0, 1);
        public const int WIDTH_DIGREE = 360;
        public const int HEIGHT_DIGREE = 180;

        private Color _renderingColor;
        private Vector3 _srcPoint;
        private TriangleMap _srcTriangle;
        private Vector2Int _pos;
        private int _index;
        private float _pValue = 1 / Mathf.PI / Mathf.PI;
        private bool _isHalf = false;
        private Vector3 _halfNormal;

        public bool RunOutFlag = false;

        private bool _isZeroDistance = false;

        public Color TotalColor { get; private set; }
        public void SourceInput(Color color, Vector3 sourcePoint)
        {
            _pValue = 1 / Mathf.PI / Mathf.PI;
            TotalColor = Color.black;
            _renderingColor = color;
            _srcPoint = sourcePoint;
            _isHalf = false;
            _isZeroDistance = false;
        }

        public void HalfRendering(Vector3 pN, TriangleMap srcTriangle, int x, int y)
        {
            _srcTriangle = srcTriangle;
            _pos = new Vector2Int(x, y);
            _index = _srcTriangle.SrcTriangle.Index;
            _pValue = 4 / Mathf.PI / Mathf.PI;
            _isHalf = true;
            _halfNormal = pN;
        }

        public bool Render(List<TriangleMap> triangles)
        {
            bool hasValue = false;
            if (_isZeroDistance)
            {
                return hasValue;
            }
            foreach (TriangleMap triangleMap in triangles)
            {
                if (_isHalf && triangleMap.SrcTriangle.Index == _index)
                {
                    continue;
                }
                triangleMap.ForPosistion((x, y) =>
                {
                    IsJumpOut();
                    Vector3 tWP = triangleMap.GetPoint(x, y);
                    if (_isHalf)
                    {
                        Vector3 dire = tWP - _srcPoint;
                        if (dire.magnitude < 0.000001f && Vector3.Dot(triangleMap.SrcTriangle.WeightNormal, _halfNormal) > 0)
                        {
                            return;
                        }
                        float dot = Vector3.Dot(dire, _halfNormal);
                        if (dot < -0.0000001f)
                        {
                            return;
                        }
                    }
                    bool value = LineTrace(triangles, tWP, x, y, triangleMap.SrcTriangle.Index);
                    hasValue = value || hasValue;
                });
            }
            return hasValue;
        }

        private void IsJumpOut()
        {
            if (RunOutFlag)
            {
                throw new RunOut();
            }
        }

        private bool LineTrace(List<TriangleMap> maps, Vector3 targetPoint, int x, int y, int triangleIndex)
        {
            bool hasValue = false;
            Ray ray = new Ray(_srcPoint, targetPoint - _srcPoint);
            float dest = float.MaxValue;
            TriangleMap targetTriangle = null;
            foreach (TriangleMap triangle in maps)
            {
                //if (triangle.TriangleTrace(ray, out CastResult result))
                if (RenderHelper.EasyTriangleTest(ray, triangle.SrcTriangle, out CastResult result))
                {
                    if (result.CastDistance + 0.00001f < dest)
                    {
                        targetTriangle = triangle;
                        dest = result.CastDistance;
                    }
                    else if (triangleIndex == triangle.SrcTriangle.Index && Mathf.Abs(dest - result.CastDistance) < 0.0001f)
                    {
                        targetTriangle = triangle;
                        dest = result.CastDistance;
                    }
                }
            }

            if (targetTriangle != null && targetTriangle.SrcTriangle.Index == triangleIndex)
            {
                float r = dest;
                Color writeColor;
                if (r < 0.00001f)
                {
                    writeColor = _renderingColor;
                    _isZeroDistance = true;
                }
                else
                {
                    float tan = Mathf.Atan(TriangleMap.MIN_SIZE / 2 / r);
                    float tempValue = tan * tan * _pValue;
                    writeColor = _renderingColor * tempValue;
                }
                writeColor.a = 1;
                hasValue = true;
                targetTriangle.Buffer.GetNowRending()[x, y] += writeColor;
                TotalColor += writeColor;
            }

            return hasValue;
        }

        private const float MIN_VALUE = 0.0001f;
        private static bool ColorIsNull(Color color)
        {
            return color.r < MIN_VALUE && color.g < MIN_VALUE && color.b < MIN_VALUE;
        }
    }
}
