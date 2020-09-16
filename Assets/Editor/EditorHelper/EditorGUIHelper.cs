using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LightRef
{
    public static class EditorGUIHelper
    {
        public static T EnumPopup<T>(string desc, T src) where T : Enum
        {
            string[] names = Enum.GetNames(typeof(T));
            string selName = src.ToString();
            int index = 0;
            for (int i = 0; i < names.Length; ++i)
            {
                if (selName == names[i])
                {
                    index = i;
                }
            }
            index = EditorGUILayout.Popup(desc, index, names);
            T value = (T)Enum.Parse(typeof(T), names[index]);
            return value;
        }

        public static T ObjectField<T>(string desc, T src, bool isScene = true) where T : Object
        {
            return EditorGUILayout.ObjectField(desc, src, typeof(T), isScene) as T;
        }

        public static float NumericField(string desc, float value)
        {
            return EditorGUILayout.FloatField(desc, value);
        }

        public static int NumericField(string desc, int value)
        {
            return EditorGUILayout.IntField(desc, value);
        }

        public static Vector3 Vector3Field(string desc, Vector3 v)
        {
            return EditorGUILayout.Vector3Field(desc, v);
        }
    }
}
