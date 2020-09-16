using UnityEngine;

namespace LightRef
{
    public class TriangleInitData
    {
        public Vector3 Ve1, Ve2, Ve3;
        public Vector2 Uv1, Uv2, Uv3;
        public Vector3 No1, No2, No3;
        public Vector4 Ta1, Ta2, Ta3;
        public Matrix4x4 LocalTriax;
        public MaterialScript Src;
    }
}
