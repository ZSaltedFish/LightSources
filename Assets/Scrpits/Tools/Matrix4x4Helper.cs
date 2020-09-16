using UnityEngine;

namespace LightRef
{
    public static class Matrix4x4Helper
    {
        public static Ray MultiplyRay(this Matrix4x4 matr, Vector3 offset, Ray ray)
        {
            Vector3 origin = matr.MultiplyVector(ray.origin) + offset;
            Vector3 dire = matr.MultiplyVector(ray.direction);
            return new Ray(origin, dire);
        }
    }
}
