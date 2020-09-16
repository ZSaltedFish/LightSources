using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LightRef
{
    [CustomEditor(typeof(ReflectController))]
    public class ReflectControllerEditor : Editor
    {
        private ReflectController _target;
        private Task _task = null;

        private bool _taskGo = false;
        private float _pres = 0;

        public void Awake()
        {
            _target = target as ReflectController;
        }

        public override void OnInspectorGUI()
        {
            _target.GlobalParams = EditorGUILayout.ObjectField("全局参数", _target.GlobalParams, typeof(GlobalParams), true) as GlobalParams;
            _target.PosShow = EditorGUILayout.ObjectField("位置显示器", _target.PosShow, typeof(GameObject), true) as GameObject;
            _target.CameraSrc = EditorGUILayout.ObjectField("摄像机参数", _target.CameraSrc, typeof(CameraLightCatcher), true) as CameraLightCatcher;

            if (_taskGo)
            {
                _taskGo = false;
                _task.Dispose();
                _task = null;
                _target.CameraSrc.ApplyTexture();
            }
            if (_task == null)
            {
                EditorUtility.ClearProgressBar();
                if (GUILayout.Button("执行"))
                {
                    _target.Execute();
                    Color srcColor = _target.GlobalParams.DirectionalLight.color;
                    Vector3 pos = _target.GlobalParams.DirectionalLight.transform.position;
                    RoundPoint((v) =>
                    {
                        float atte = _target.GlobalParams.AirRel;
                        Color color = srcColor;
                        Ray light = new Ray(pos, v);
                        while (ColorEqure(color, Color.black))
                        {
                            if (!_target.CameraSrc.Cast(light, atte, color))
                            {
                                if (_target.Raycast(light, out CastResult result))
                                {
                                    float dot = Vector3.Dot(result.CastPointNormal, -light.direction);
                                    Vector3 newDirection = dot * 2 * result.CastPointNormal + light.direction;
                                    light = new Ray(result.CastPoint, newDirection.normalized);
                                    float atreValue = result.CastDistance * atte;
                                    Color atteColor = new Color(atreValue, atreValue, atreValue, 0);
                                    color = result.CastTriangle.GetWeightColor(result.WeightValue) * (color - atteColor);
                                }
                                else
                                {
                                    Debug.LogWarning($"光照{light} cast 失败");
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    });
                }
            }
            else
            {
                EditorUtility.DisplayProgressBar("执行中", "当前进度", _pres);
            }
        }

        private static bool ColorEqure(Color a, Color b)
        {
            return !(a.r <= b.r || a.b <= b.b || a.g <= b.g);
        }

        public void RoundPoint(Action<Vector3> act)
        {
            _task = new Task(() =>
            {
                try
                {
                    Vector3 vX = new Vector3(1, 0, 0);
                    act(vX);
                    act(-vX);
                    for (int z = 0; z < 180; ++z)
                    {
                        float zRad = z * Mathf.Deg2Rad;
                        float xValue = Mathf.Cos(zRad);
                        for (int x = 0; x < 360; ++x)
                        {
                            float xRad = x * Mathf.Deg2Rad;
                            float dist = Mathf.Sin(zRad);
                            float yValue = dist * Mathf.Sin(xRad);
                            float zValue = dist * Mathf.Cos(xRad);

                            Vector3 v = new Vector3(xValue, yValue, zValue);
                            act(v);
                            _pres = (z * 360 + x) / (180 * 360f);
                        }
                    }
                }
                catch(Exception err)
                {
                    Debug.LogError(err);
                }
                finally
                {
                    _taskGo = true;
                }
            });

            _task.Start();
        }
    }
}
