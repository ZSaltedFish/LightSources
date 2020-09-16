using System;
using UnityEngine;

namespace LightRef
{
    public class DicTexture<T>
    {
        private readonly T[,] _map;
        public readonly int Width, Height;

        public T this[int x, int y]
        {
            get
            {
                try
                {
                    return _map[x, y];
                }
                catch (ArgumentOutOfRangeException err)
                {
                    throw new Exception($"点({x}, {y})越界", err);
                }
            }

            set
            {
                try
                {
                    _map[x, y] = value;
                }
                catch (ArgumentOutOfRangeException err)
                {
                    throw new Exception($"点({x}, {y})越界", err);
                }
            }
        }

        public DicTexture(int width, int height)
        {
            Width = width;
            Height = height;
            _map = new T[width, height];
        }
    }
}
