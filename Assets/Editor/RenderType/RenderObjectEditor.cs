using System;
using UnityEditor;
using UnityEngine;

namespace LightRef
{
    [CustomEditor(typeof(RenderObject))]
    public class RenderObjectEditor : Editor
    {
        private RenderObject _target;

        public void Awake()
        {
            _target = target as RenderObject;
        }

        public override void OnInspectorGUI()
        {
            _target.ROType = EditorGUIHelper.EnumPopup("渲染类型", _target.ROType);

            switch (_target.ROType)
            {
                case RenderObjectType.PointLight:
                    DrawLightPoint();
                    break;
                case RenderObjectType.Mesh:
                    DrawRelfection();
                    break;
                case RenderObjectType.Camera:
                    DrawCamera();
                    break;
            }

            if (_target.TryGetRenderType(out Type type))
            {
                EditorGUILayout.LabelField($"当前渲染类型:{type}");
            }
            else
            {
                EditorGUILayout.LabelField($"没有初始化渲染方法");
            }
        }

        private void DrawCamera()
        {
            _target.SrcCamera = EditorGUIHelper.ObjectField("摄像机", _target.SrcCamera);
        }

        private void DrawLightPoint()
        {
            _target.SrcLight = EditorGUIHelper.ObjectField("光源", _target.SrcLight);
            _target.LightPower = EditorGUIHelper.NumericField("光强", _target.LightPower);
        }

        private void DrawRelfection()
        {
            _target.SrcMatrial = EditorGUIHelper.ObjectField("源渲染器", _target.SrcMatrial);
        }

        public void OnDestroy()
        {
            _target = null;
        }
    }
}
