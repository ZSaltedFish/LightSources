using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LightRef
{
    public struct LightData
    {
        public float LightPower;
        public Color LightColor;
        public Vector3 Posiston;
    }
    public class TriangleMapController : MonoBehaviour
    {
        public RenderObject PointLight;
        public LightCameraRendering Camera;

        private readonly List<TriangleMap> _maps = new List<TriangleMap>();
        private readonly List<LightData> _lights = new List<LightData>();
        private RendingTask _task;
        public void InitData()
        {
            _lights.Clear();
            Triangle.ResetCounter();
            _maps.Clear();
            var sps = GetComponentsInChildren<MaterialScript>();
            foreach (var item in sps)
            {
                var triangles = item.Initialize();
                foreach (var triangle in triangles)
                {
                    TriangleMap map = new TriangleMap(triangle)
                    {
                        Absorbance = item.Absorbance,
                        BlindPow = item.BlindPow,
                        MainTexDic = item.MainTexDic,
                        NorTexDic = item.NorTexDic
                    };

                    _maps.Add(map);
                }
            }

            LightData data = new LightData()
            {
                LightPower = PointLight.LightPower,
                LightColor = PointLight.GetComponent<Light>().color,
                Posiston = PointLight.transform.position
            };
            _lights.Add(data);

            Camera.Init();
        }

        public void InitTask(bool force = false)
        {
            if (_task == null || force)
            {
                _task = new RendingTask(_lights, _maps, CallBack);
            }
        }

        private RenderData _testRenderData;
        private void CallBack(object obj)
        {
            Debug.Log($"执行Callback");
            _testRenderData = obj as RenderData;
            _task.Dispose();
            _task = null;
            InitTask();
        }

        public bool IsRunning()
        {
            if (_task == null)
            {
                InitTask();
            }
            return _task.Status == System.Threading.Tasks.TaskStatus.Running;
        }

        public void Write()
        {
            //Texture2D tex = new Texture2D(_maps[2].Width * 2, _maps[2].Height);
            //for (int i = 2; i < 4; ++i)
            //{
            //    int xBase = tex.width / 2 * (i - 2);
            //    var map = _maps[i];
            //    map.MapForEach((x, y) =>
            //    {
            //        float w2 = x / (float)map.Width;
            //        float w3 = y / (float)map.Height;
            //        Color color = map.GetColor(w2, w3);
            //        tex.SetPixel(x + xBase, y, color);
            //        return color;
            //    });
            //    tex.Apply();
            //}
            //byte[] bytes = tex.EncodeToTGA();

            byte[] bytes = _testRenderData != null ? _testRenderData.GetRenderer().EncodeToTGA() :
                Camera.Render(_maps).GetRenderer().EncodeToTGA();
            using (FileStream file = new FileStream("Assets/Editor/Resources/RenderImage/camera.tga", FileMode.Create))
            {
                file.Write(bytes, 0, bytes.Length);
            }
            _testRenderData = null;
        }

        public void GoStart()
        {
            InitData();
            InitTask(true);
            _task.Start();
        }

        public void Stop()
        {
            _task.Stop();
        }

        public void OnDisable()
        {
            _task.Dispose();
            _task = null;
        }

        public string OutputData()
        {
            try
            {
                StringBuilder builder = new StringBuilder($"第{_task.RunningTime}轮进度{_task.CountPro * 100:f2}%\n总颜色量为\nR:{_task.NowRunningColor.r}\nG:{_task.NowRunningColor.g}\nB:{_task.NowRunningColor.b}");
                builder.Append($"\n临时颜色:\n(R:{_task.TempColor.r}, G:{_task.TempColor.g}, B:{_task.TempColor.b}, A:{_task.TempColor.a})");
                return builder.ToString();
            }
            catch (Exception err)
            {
                return $"{err}";
            }
        }
    }
}
