using UnityEngine;

namespace LightRef
{
    public class ReflectionRenderTypeInitData : RenderTypeInitData
    {
        public float ReflectionRange;
        public Triangle SourceTriangle;

        public Vector3 Normal;
        public Vector3 WorldPoint;
        public float LambertValue;
    }
}
