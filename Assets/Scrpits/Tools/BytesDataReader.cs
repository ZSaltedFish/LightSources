using System;
using UnityEngine;

namespace LightRef
{
    public class BytesDataReader
    {
        public int Offset;
        private byte[] _bytes;
        public BytesDataReader(byte[] bytes)
        {
            _bytes = bytes;
            Offset = 0;
        }

        public int GetInt32()
        {
            int value = BitConverter.ToInt32(_bytes, Offset);
            Offset += 4;
            return value;
        }

        public float GetSingle()
        {
            float value = BitConverter.ToSingle(_bytes, Offset);
            Offset += 4;
            return value;
        }

        public long GetInt64()
        {
            long value = BitConverter.ToInt64(_bytes, Offset);
            Offset += 8;
            return value;
        }

        public double GetDouble()
        {
            double value = BitConverter.ToDouble(_bytes, Offset);
            Offset += 8;
            return value;
        }

        public char GetChar()
        {
            char c = BitConverter.ToChar(_bytes, Offset);
            Offset += 4;
            return c;
        }

        public Color GetColorRGBA()
        {
            float r = GetSingle();
            float g = GetSingle();
            float b = GetSingle();
            float a = GetSingle();
            return new Color(r, g, b, a);
        }

        public Vector3 GetVector3()
        {
            float x = GetSingle();
            float y = GetSingle();
            float z = GetSingle();
            return new Vector3(x, y, z);
        }
    }
}
