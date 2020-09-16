using UnityEditor;
using UnityEngine;

namespace LightRef
{
    [CustomEditor(typeof(MaterialScript))]
    public class MaterialScriptEditor : Editor
    {
        private MaterialScript _target;

        public void Awake()
        {
            _target = target as MaterialScript;
        }

        public override void OnInspectorGUI()
        {
            _target.MainTex = EditorGUILayout.ObjectField("主贴图", _target.MainTex, typeof(Texture2D), false) as Texture2D;
            _target.NormalTex = EditorGUILayout.ObjectField("法线贴图", _target.NormalTex, typeof(Texture2D), false) as Texture2D;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _target.LambertLerp = EditorGUILayout.Slider("兰伯特反射插值", _target.LambertLerp, 0, 1);
            _target.LambertReflectionValue = EditorGUILayout.Slider("漫反射返回值", _target.LambertReflectionValue, 0, 1);
            _target.Absorbance = EditorGUILayout.Slider("光吸收度", _target.Absorbance, 0, 1);
            EditorGUILayout.EndVertical();
        }
    }
}
