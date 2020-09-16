using UnityEditor;
using UnityEngine;

namespace LightRef
{
    public class DataViewer : EditorWindow
    {
        private GameObject _go;
        public void OnGUI()
        {
            _go = EditorGUILayout.ObjectField("渲染模型", _go, typeof(GameObject), true) as GameObject;

            if (_go == null)
            {
                return;
            }
            MeshFilter renderer = _go.GetComponent<MeshFilter>();
            if (renderer == null)
            {
                return;
            }

            Mesh mesh = renderer.sharedMesh;
            
            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                Vector3 normal = new Vector3(mesh.normals[i].x, mesh.normals[i].y, mesh.normals[i].z);
                Vector3 tangent = new Vector3(mesh.tangents[i].x, mesh.tangents[i].y, mesh.tangents[i].z);
                Vector3 newVector = Vector3.Cross(normal, tangent) * mesh.tangents[i].w;
                Vector4 v4 = new Vector4(0, 0, 0, 1);

                Matrix4x4 newMatr = new Matrix4x4(tangent, newVector, tangent, v4);
                Vector3 lightDir = new Vector3(0, -1, 0);

                Vector3 endPoint = newMatr.MultiplyVector(lightDir);
                EditorGUILayout.Vector3Field($"{i + 1}:", endPoint);
            }
        }

        [MenuItem("工具/模型/数据浏览器")]
        public static void Init()
        {
            var win = GetWindow<DataViewer>();
            win.Show();
        }
    }
}
