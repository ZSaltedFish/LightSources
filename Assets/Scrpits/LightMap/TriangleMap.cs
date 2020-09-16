using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LightRef
{
    public class RenderingMapBuffer
    {
        private readonly List<Color[,]> _buffers;
        private int _nowRendering = 0;
        public readonly int Width, Height;
        public RenderingMapBuffer(int width, int height)
        {
            Width = width;
            Height = height;

            _buffers = new List<Color[,]>
            {
                new Color[Width, Height],
                new Color[Width, Height]
            };
        }

        public Color[,] GetNowRending()
        {
            return _buffers[_nowRendering];
        }

        public Color[,] GetReading()
        {
            return _buffers[1 - _nowRendering];
        }

        public void Exchange()
        {
            _nowRendering = 1 - _nowRendering;
        }
    }

    public class TriangleMap
    {
        public const float MIN_SIZE = 0.01f;
        public readonly Triangle SrcTriangle;
        public RenderingMapBuffer Buffer;

        public Color[,] Map;

        public readonly int Width, Height;

        #region MaterialScript数据
        public Color[,] MainTexDic, NorTexDic;
        public float LambertLerp;
        public float BlindPow;
        public float Absorbance;
        #endregion

        private float _distB, _distC;
        private Vector3 _direB, _direC;
        public TriangleMap(Triangle srcTriangle)
        {
            SrcTriangle = srcTriangle;
            _direB = srcTriangle.PointB - srcTriangle.PointA;
            _direC = srcTriangle.PointC - srcTriangle.PointA;

            _distB = _direB.magnitude;
            _distC = _direC.magnitude;
            Width = Mathf.Max((int)(_distB / MIN_SIZE), 1);
            Height = Mathf.Max((int)(_distC / MIN_SIZE), 1);

            Map = new Color[Width, Height];
            Buffer = new RenderingMapBuffer(Width, Height);
        }

        public void RendingForEach(Func<int, int, Color> func)
        {
            Color[,] tempMap = Buffer.GetNowRending();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (OutofBound(x, y))
                    {
                        break;
                    }
                    tempMap[x, y] += func(x, y);
                }
            }
        }

        public void ForPosistion(Action<int, int> func)
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (OutofBound(x, y))
                    {
                        break;
                    }
                    func(x, y);
                }
            }
        }

        public void MapForEach(Func<int, int, Color> func)
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (OutofBound(x, y))
                    {
                        break;
                    }
                    Map[x, y] = func(x, y);
                }
            }
        }

        public void ReadingForEach(Action<int, int, Color> func)
        {
            Color[,] tempMap = Buffer.GetReading();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (OutofBound(x, y))
                    {
                        break;
                    }
                    func(x, y, tempMap[x, y]);
                }
            }
        }

        public void ApplyReadingMap()
        {
            Color[,] rendering = Buffer.GetReading();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Color color = Map[x, y];
                    color += rendering[x, y];
                    color.a = 1;
                    rendering[x, y] = Color.black;
                    Map[x, y] = color;
                }
            }

            Buffer.Exchange();
        }

        /// <summary>
        /// 计算发射出去的颜色值
        /// </summary>
        /// <param name="x">反射贴图X坐标</param>
        /// <param name="y">反射贴图Y坐标</param>
        /// <returns>颜色</returns>
        public Color GetReflectedColor(int x, int y)
        {
            Color color = Buffer.GetReading()[x, y];
            Vector3 weight = GetWeight(x, y);
            float w2 = weight.y;
            float w3 = weight.z;
            float w1 = weight.x;

            Color texColor = SrcTriangle.GetWeightColor(w1, w2, w3);
            color *= 1 - Absorbance;
            Color result = RenderHelper.ColorFixFunction(LambertLerp, color, texColor);
            result.a = 1;
            return result;
        }

        public Vector3 GetWeight(int x, int y)
        {
            float w2 = RoundToWidthMinSize(x * MIN_SIZE / _distB);
            float w3 = RoundToWidthMinSize(y * MIN_SIZE / _distC);
            float w1 = 1 - w2 - w3;
            return new Vector3(w1, w2, w3);
        }

        public Vector3 GetPoint(int x, int y)
        {
            Vector3 weight = GetWeight(x, y);
            return SrcTriangle.GetWeightPoint(weight.x, weight.y, weight.z);
        }

        /// <summary>
        /// 判断像素是否出界
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool OutofBound(int x, int y)
        {
            float w2 = Width == 1 ? RoundToWidthMinSize(x * MIN_SIZE / _distB) : RoundToWidthMinSize((x + 1) * MIN_SIZE / _distB);
            float w3 = Height == 1 ? RoundToWidthMinSize(y * MIN_SIZE / _distC) : RoundToWidthMinSize((y + 1) * MIN_SIZE / _distC);
            return !RenderHelper.Saturate(1 - w2 - w3, MIN_V);
        }

        /// <summary>
        /// 内部点获取权重
        /// </summary>
        /// <param name="castPoint"></param>
        /// <returns></returns>
        public Vector3 GetWeightWidthInnerPoint(Vector3 castPoint)
        {
            float w2 = RoundToWidthMinSize(castPoint.x);
            float w3 = RoundToWidthMinSize(castPoint.y);
            return new Vector3(1 - w2 - w3, w2, w3);
        }

        public Vector2Int GetInnerPos(float w2, float w3)
        {
            float distB = w2 * _distB;
            float distC = w3 * _distC;
            int x = Mathf.Clamp(Mathf.RoundToInt(distB / MIN_SIZE), 0, Width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(distC / MIN_SIZE), 0, Height - 1);
            return new Vector2Int(x, y);
        }

        public Vector2Int GetInnerPosStable(float w2, float w3)
        {
            bool runX = true;
            Vector2Int pos = GetInnerPos(w2, w3);
            while (OutofBound(pos.x, pos.y))
            {
                if (pos.x == 0 && pos.y == 0)
                {
                    return pos;
                }
                if (runX)
                {
                    if (pos.x != 0)
                    {
                        --pos.x;
                    }
                }
                else
                {
                    if (pos.y != 0)
                    {
                        --pos.y;
                    }
                }
                runX = !runX;
            }
            return pos;
        }

        public bool PointOutofBoundWithInnerPoint(Vector3 point)
        {
            float w2 = point.x, w3 = point.y;

            int w2Int = (int)(w2 / MIN_V);
            int w3Int = (int)(w3 / MIN_V);
            if (w2Int < 0 || w3Int < 0)
            {
                return true;
            }
            float newW2, newW3;
            if (Width == 0)
            {
                newW2 = ((int)(w2 * _distB / MIN_SIZE)) * MIN_SIZE / _distB;
            }
            else
            {
                newW2 = ((int)(w2 * _distB / MIN_SIZE) + 1) * MIN_SIZE / _distB;
            }
            if (Height == 0)
            {
                newW3 = ((int)(w3 * _distC / MIN_SIZE)) * MIN_SIZE / _distC;
            }
            else
            {
                newW3 = ((int)(w3 * _distC / MIN_SIZE) + 1) * MIN_SIZE / _distC;
            }

            float w1 = RoundToWidthMinSize(1 - newW2 - newW3);
            return !RenderHelper.Saturate(w1, MIN_V);
        }

        private static readonly float MIN_V = MIN_SIZE * 0.1f;
        public static Vector3 RoundToWidthMinSize(Vector3 v)
        {
            v.x = Mathf.RoundToInt(v.x / MIN_V) * MIN_V;
            v.y = Mathf.RoundToInt(v.y / MIN_V) * MIN_V;
            v.z = Mathf.RoundToInt(v.z / MIN_V) * MIN_V;
            return v;
        }

        public static float RoundToWidthMinSize(float v)
        {
            return Mathf.RoundToInt(v / MIN_V) * MIN_V;
        }

        public bool TriangleTrace(Ray ray, out CastResult result)
        {
            result = CastResult.NULL_VALUE;
            if (Vector3.Dot(SrcTriangle.WeightNormal, ray.direction) > -MIN_V)
            {
                return false;
            }

            Vector3 localp = SrcTriangle.InverseTransformPoint(ray.origin);
            Vector3 localDire = SrcTriangle.InverseTransformVector(ray.direction);
            Vector3 castPoint = localp - (localDire * localp.z / localDire.z);

            if (!PointOutofBoundWithInnerPoint(castPoint))
            {
                float w2 = RoundToWidthMinSize(castPoint.x);
                float w3 = RoundToWidthMinSize(castPoint.y);
                float w1 = 1 - w2 - w3;
                Vector3 point = SrcTriangle.GetWeightPoint(w1, w2, w3);
                result = new CastResult
                {
                    CastPoint = SrcTriangle.GetWeightPoint(w1, w2, w3),
                    WeightValue = new Vector3(w1, w2, w3),
                    CastDistance = (point - ray.origin).magnitude
                };
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{SrcTriangle} 的贴图三角形";
        }
    }
}
