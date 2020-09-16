using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    public class RenderData : IDisposable
    {
        public static void Init()
        {
            _SRC_INDEX = -1;
            _refList.Clear();
        }

        private static int _SRC_INDEX = -1;
        public int Index { get; private set; }

        public Color[,] RenderTex;
        public float[,] DeepTex;
        public Vector3[,] PointTex;
        public int[,] TriangleIndexes;
        public Vector3[,] LightDire;
        public Vector3[,] PointWeight;

        public int CameraPointX, CameraPointY;

        public int PixCount { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private static Dictionary<int, RenderData> _refList = new Dictionary<int, RenderData>();
        private static Queue<RenderData> _pool = new Queue<RenderData>();

        public static bool IsUsing(RenderData data)
        {
            return _refList.ContainsKey(data.Index);
        }

        public static RenderData GetDataWithIndex(int index)
        {
            return _refList[index];
        }

        public static RenderData Apply()
        {
            RenderData data;
            if (_pool.Count == 0)
            {
                data = new RenderData();
            }
            else
            {
                data = _pool.Dequeue();
            }
            _refList.Add(data.Index, data);
            return data;
        }

        private RenderData()
        {
            Index = ++_SRC_INDEX;
        }

        public void Init(int w, int h)
        {
            Init(w, h, Color.black);
        }

        public void Init(int w, int h, Color initColor)
        {
            Width = w;
            Height = h;
            InitArray();

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    RenderTex[x, y] = initColor;
                    DeepTex[x, y] = float.MaxValue;
                    TriangleIndexes[x, y] = -2;
                }
            }
            CameraPointX = CameraPointY = 0;
            PixCount = w * h;
        }

        private void InitArray()
        {
            RenderTex = new Color[Width, Height];
            DeepTex = new float[Width, Height];
            PointTex = new Vector3[Width, Height];
            TriangleIndexes = new int[Width, Height];
            LightDire = new Vector3[Width, Height];
            PointWeight = new Vector3[Width, Height];
        }

        public Texture2D GetRenderer()
        {
            Texture2D tex = new Texture2D(Width, Height);

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    tex.SetPixel(x, y, RenderTex[x, y]);
                }
            }
            tex.Apply();
            return tex;
        }

        public byte[] ToData()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Width));
            bytes.AddRange(BitConverter.GetBytes(Height));
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Color color = RenderTex[x, y];
                    bytes.AddRange(BitConverter.GetBytes(color.r));
                    bytes.AddRange(BitConverter.GetBytes(color.g));
                    bytes.AddRange(BitConverter.GetBytes(color.b));
                    bytes.AddRange(BitConverter.GetBytes(color.a));

                    bytes.AddRange(BitConverter.GetBytes(DeepTex[x, y]));

                    bytes.AddRange(BitConverter.GetBytes(PointTex[x, y].x));
                    bytes.AddRange(BitConverter.GetBytes(PointTex[x, y].y));
                    bytes.AddRange(BitConverter.GetBytes(PointTex[x, y].z));

                    bytes.AddRange(BitConverter.GetBytes(TriangleIndexes[x, y]));

                    bytes.AddRange(BitConverter.GetBytes(LightDire[x, y].x));
                    bytes.AddRange(BitConverter.GetBytes(LightDire[x, y].y));
                    bytes.AddRange(BitConverter.GetBytes(LightDire[x, y].z));

                    bytes.AddRange(BitConverter.GetBytes(PointWeight[x, y].x));
                    bytes.AddRange(BitConverter.GetBytes(PointWeight[x, y].y));
                    bytes.AddRange(BitConverter.GetBytes(PointWeight[x, y].z));
                }
            }
            return bytes.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            BytesDataReader reader = new BytesDataReader(bytes);
            Width = reader.GetInt32();
            Height = reader.GetInt32();
            InitArray();

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    RenderTex[x, y] = reader.GetColorRGBA();
                    DeepTex[x, y] = reader.GetSingle();
                    PointTex[x, y] = reader.GetVector3();
                    TriangleIndexes[x, y] = reader.GetInt32();
                    LightDire[x, y] = reader.GetVector3();
                    PointWeight[x, y] = reader.GetVector3();
                }
            }
        }

        public void Dispose()
        {
            _pool.Enqueue(this);
            _refList.Remove(Index);
        }
    }
}
