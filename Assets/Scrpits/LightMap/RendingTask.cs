using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LightRef
{
    public class RunOut : Exception
    {
        public readonly object ThrowObject;

        public RunOut() { }
        public RunOut(object obj)
        {
            ThrowObject = obj;
        }
    }
    public class RendingTask : IDisposable
    {
        private Action<object> _succeed;
        public readonly static Color IGNORE_COLOR = new Color(0, 0, 0, 1);
        private LightingRendering _rendering;
        private Task _runTask;
        private readonly CancellationTokenSource _token;
        private readonly List<TriangleMap> _maps;
        private readonly List<LightData> _lights;
        private int _totalPix;

        public float CountPro { get; private set; }

        public Color NowRunningColor { get; private set; }
        public Color TempColor { get; private set; }
        public int RunningTime { get; private set; } = 0;
        public TaskStatus Status => _runTask.Status;

        public RendingTask(List<LightData> lights, List<TriangleMap> maps, Action<object> callBack)
        {
            _rendering = new LightingRendering();
            _token = new CancellationTokenSource();
            _maps = maps;
            _lights = lights;
            _runTask = new Task(TaskStart, _token.Token);
            _succeed = callBack;
        }

        public void Start()
        {
            _runTask.Start();
        }

        public void Stop()
        {
            _token.Cancel();
        }

        private void TaskStart()
        {
            _totalPix = 0;
            foreach (TriangleMap map in _maps)
            {
                int count = map.Width * map.Height;
                _totalPix += count;
            }
            RunningTime = 0;
            try
            {
                foreach (LightData light in _lights)
                {
                    Color color = light.LightColor * light.LightPower;
                    color.a = 1;
                    _rendering.SourceInput(color, light.Posiston);
                    _rendering.Render(_maps);
                    NowRunningColor = _rendering.TotalColor;
                }
                AllApply();
                bool run = false;
                do
                {
                    run = Run(_maps);
                    ++RunningTime;
                }
                while (run);
                _succeed?.Invoke(null);
            }
            catch (RunOut err)
            {
                Debug.LogWarning($"被强制暂停了");
                _succeed?.Invoke(err.ThrowObject);
            }
            catch (Exception err)
            {
                Debug.LogError(err);
            }
            finally
            {
                Debug.Log($"调用次数:{RunningTime}");
            }
        }

        private bool Run(List<TriangleMap> maps)
        {
            bool hasValue = false;
            Color tempColor = Color.black;
            int pixCount = 0;
            foreach (TriangleMap map in maps)
            {
                map.ReadingForEach((x, y, color) =>
                {
                    ++pixCount;
                    CountPro = pixCount / (float)_totalPix;
                    if (ColorIsNull(color))
                    {
                        return;
                    }
                    Color reflectedColor = map.GetReflectedColor(x, y);
                    Vector3 worldPoint = map.GetPoint(x, y);
                    if (_token.Token.IsCancellationRequested)
                    {
                        _rendering.RunOutFlag = true;
                    }

                    _rendering.SourceInput(reflectedColor, worldPoint);
                    _rendering.HalfRendering(map.SrcTriangle.WeightNormal, map, x, y);
                    hasValue = _rendering.Render(maps) || hasValue;
                    tempColor += _rendering.TotalColor;
                    tempColor.a = 1;
                    TempColor = tempColor;
                });
            }
            NowRunningColor = tempColor;
            AllApply();
            return hasValue;
        }

        private void AllApply()
        {
            foreach (TriangleMap map in _maps)
            {
                map.ApplyReadingMap();
            }
        }

        public void Dispose()
        {
            try
            {
                _runTask.Dispose();
            }
            catch (InvalidOperationException) { }
            catch (Exception err)
            {
                Debug.LogError($"{_runTask.Status}\n{err}");
            }
        }

        private const float MIN_VALUE = 0.0001f;

        private static bool ColorIsNull(Color color)
        {
            return color.r < MIN_VALUE && color.g < MIN_VALUE && color.b < MIN_VALUE;
        }
    }
}
