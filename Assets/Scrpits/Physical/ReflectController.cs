using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    [ExecuteInEditMode]
    public class ReflectController : MonoBehaviour
    {
        public GlobalParams GlobalParams;
        private List<Triangle> _triangles;
        public CameraLightCatcher CameraSrc;

        public GameObject PosShow;
        public void Execute()
        {
            _triangles = new List<Triangle>();
            CameraSrc.InitPrivate();
            int pixCount = 0;
            MaterialScript[] materialScripts = GetComponentsInChildren<MaterialScript>();
            foreach (MaterialScript item in materialScripts)
            {
                List<Triangle> triangles = item.Initialize();
                _triangles.AddRange(triangles);
                pixCount += item.MainTex.width * item.MainTex.height;
            }
        }

        public bool Raycast(Ray light, out CastResult distResult)
        {
            float distance = float.MaxValue;
            bool cast = false;
            distResult = new CastResult();
            foreach (Triangle triangle in _triangles)
            {
                if (triangle.Raycast(light, out CastResult result))
                {
                    cast = true;
                    if (distance > result.CastDistance)
                    {
                        distResult = result;
                    }
                }
            }
            return cast;
        }

        public void TestSpecularReflection()
        {
        }
    }
}
