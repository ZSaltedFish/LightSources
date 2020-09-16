using UnityEditor;
using UnityEngine;

namespace LightRef
{
    [CustomEditor(typeof(RenderingController))]
    public class RenderingControllerEditor : Editor
    {
        private Vector3 _t1, _t2;

        private RenderingController _target;

        public void Awake()
        {
            _target = target as RenderingController;
        }

        private bool IsRunning()
        {
            return _target.RenderTask?.Status == System.Threading.Tasks.TaskStatus.Running;
        }

        public override void OnInspectorGUI()
        {
            _target.AnglePixels = EditorGUIHelper.NumericField("角度像素", _target.AnglePixels);

            _target.SrcObj = EditorGUIHelper.ObjectField("反射组", _target.SrcObj);
            _target.TextCamera = EditorGUIHelper.ObjectField("摄像机", _target.TextCamera);

            if (IsRunning())
            {
                if (GUILayout.Button("停止"))
                {
                    _target.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("执行"))
                {
                    _target.Run();
                }
            }

            if (GUILayout.Button("写出"))
            {
                _target.Write();
                AssetDatabase.Refresh();
            }
        }
    }
}
