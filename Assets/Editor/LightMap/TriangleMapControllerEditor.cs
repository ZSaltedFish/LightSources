using UnityEditor;
using UnityEngine;

namespace LightRef
{
    [CustomEditor(typeof(TriangleMapController))]
    public class TriangleMapControllerEditor : Editor
    {
        private TriangleMapController _target;

        public void Awake()
        {
            _target = target as TriangleMapController;
            _target.InitTask();
        }

        public override void OnInspectorGUI()
        {
            _target.PointLight = EditorGUIHelper.ObjectField("光源", _target.PointLight);
            _target.Camera = EditorGUIHelper.ObjectField("摄像机", _target.Camera);

            if (!_target.IsRunning())
            {
                if (GUILayout.Button("执行"))
                {
                    _target.GoStart();
                }
            }
            else
            {
                if (GUILayout.Button("停止"))
                {
                    _target.Stop();
                }
            }

            if (GUILayout.Button("写出"))
            {
                _target.Write();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.HelpBox(_target.OutputData(), MessageType.Info);
        }
    }
}
