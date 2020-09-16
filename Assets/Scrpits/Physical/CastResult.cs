using UnityEngine;

namespace LightRef
{
    public struct CastResult
    {
        public static CastResult NULL_VALUE { get; } = new CastResult();
        public Vector3 CastPoint;
        public Vector3 WeightValue;
        public Triangle CastTriangle;
        public Vector3 CastPointNormal;
        public float CastDistance;

        public Color GetWeightColor()
        {
            return CastTriangle.GetWeightColor(WeightValue);
        }
    }
}
