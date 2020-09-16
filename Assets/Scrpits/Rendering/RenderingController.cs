using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LightRef
{
    public class RenderingController : MonoBehaviour
    {
        private static RenderingController Instance;
        public static float AnglePixel => Instance.AnglePixels;
        public float AnglePixels = 10;

        public Camera TextCamera;
        public GameObject SrcObj;

        private List<RenderObject> _lightRO;
        public Task RenderTask;
        private CancellationTokenSource _token;
        private RenderObject _camera;
        private Color[,] _lightColor;
        private int _width;
        private int _height;

        public bool? IsCancel => _token?.IsCancellationRequested;
        public void Run()
        {
            Instance = this;
            RenderData.Init();
            InitTriangles();
            InitROTypes();
            InitTask();
            RenderTask.Start();
        }

        public void Wait()
        {
            RenderTask?.Wait();
        }

        public void Stop()
        {
            _token.Cancel();
        }

        /// <summary>
        /// 初始化三角形
        /// </summary>
        private void InitTriangles()
        {
            List<Triangle> triangles = new List<Triangle>();
            Triangle.ResetCounter();
            MaterialScript[] scripts = SrcObj.GetComponentsInChildren<MaterialScript>();
            foreach (MaterialScript script in scripts)
            {
                var v = script.SrcObject;
                List<Triangle> initTriangles = script.Initialize();
                triangles.AddRange(initTriangles);
            }

            StaticRenderData.Triangles = triangles.ToArray();
        }

        /// <summary>
        /// 初始化渲染原件
        /// </summary>
        private void InitROTypes()
        {
            _lightRO = new List<RenderObject>();
            RenderObject[] roes = SrcObj.GetComponentsInChildren<RenderObject>();
            foreach (RenderObject ro in roes)
            {
                ro.TryInitialize(ro.ROType);
                if (ro.ROType == RenderObjectType.PointLight)
                {
                    _lightRO.Add(ro);
                }

                if (ro.ROType == RenderObjectType.Camera)
                {
                    _camera = ro;
                }
            }
        }

        private void InitTask()
        {
            _token = new CancellationTokenSource();
            RenderTask = new Task(() =>GoRender(_token.Token), _token.Token);
        }

        public void Write()
        {
            byte[] str = GetRenderer(_lightColor, _width, _height).EncodeToTGA();
            using (FileStream file = new FileStream("Assets/Editor/Resources/RenderImage/light.tga", FileMode.Create))
            {
                file.Write(str, 0, str.Length);
            }
            byte[] cmrBytes = _camera.Data.GetNow().GetRenderer().EncodeToTGA();
            using (FileStream file = new FileStream("Assets/Editor/Resources/RenderImage/camera.tga", FileMode.Create))
            {
                file.Write(cmrBytes, 0, cmrBytes.Length);
            }
        }

        private Texture2D GetRenderer(Color[,] color, int widht, int height)
        {
            Texture2D tex = new Texture2D(widht, height);

            for (int x = 0; x < widht; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    tex.SetPixel(x, y, color[x, y]);
                }
            }
            tex.Apply();
            return tex;
        }

        private void GoRender(CancellationToken token)
        {
            try
            {
                foreach (RenderObject renderObject in _lightRO)
                {
                    renderObject.InputSource(0, 0, null);
                    renderObject.Render(_camera);
                    _lightColor = renderObject.Data.GetNow().RenderTex;
                    _width = renderObject.Data.GetNow().Width;
                    _height = renderObject.Data.GetNow().Height;
                    _camera.CameraRender();
                    RenderDataMethod.ExcuteDeeply(renderObject.Data.GetNow(), _camera);
                    renderObject.Data.DisposeNow();
                }
            }
            catch(Exception err)
            {
                Debug.LogError(err);
            }
            finally
            {
                Stop();
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(PointLightRender.START_POINT, PointLightRender.ENDING_POINT);
        }
    }
}
