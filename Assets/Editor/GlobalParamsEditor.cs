using UnityEditor;
using UnityEngine;

namespace LightRef
{
    [CustomEditor(typeof(GlobalParams), true)]
    [CanEditMultipleObjects]
    public class GlobalParamsEditor : Editor
    {
        private GlobalParams _targets;
        
        public void Awake()
        {
            _targets = target as GlobalParams;
        }

        public override void OnInspectorGUI()
        {
            _targets.DirectionalLight = EditorGUILayout.ObjectField("光源", _targets.DirectionalLight, typeof(Light), true) as Light;

            _targets.AirRel = EditorGUILayout.Slider("每米空气衰减", _targets.AirRel, 0, 1);
        }
    }
}
