using UnityEditor;
using UnityEngine;

namespace LightRef
{
    public class CubeMeshCreator : EditorWindow
    {
        private GameObject _go;
        private Mesh _srcMesh;
        public void OnGUI()
        {
            _go = EditorGUILayout.ObjectField("网格对象", _go, typeof(GameObject), true) as GameObject;
            if (_go == null)
            {
                return;
            }

            MeshFilter filter = _go.GetComponent<MeshFilter>();
            if (filter == null)
            {
                EditorGUILayout.LabelField("没有网格对象");
                return;
            }

            _srcMesh = filter.sharedMesh;
            if (GUILayout.Button("创建"))
            {
                Mesh newMesh = CreateCube();

                #region 写入
                //string data = newMesh.MeshToString(new Vector3(-1, 1, 1));
                //using (var w = new StreamWriter("Assets/Mesh/NewCube.obj"))
                //{
                //    w.Write(data);
                //}
                AssetDatabase.CreateAsset(newMesh, "Assets/Mesh/NewCube.asset");
                #endregion
                AssetDatabase.Refresh();
            }
        }

        private Mesh CreateCube()
        {
            Mesh mesh = Instantiate(_srcMesh);
            Vector2[] uv2 = new Vector2[24]
            {
                new Vector2(0, 0),
                new Vector2(0, 0.29f),
                new Vector2(0.49f, 0),
                new Vector2(0.49f, 0.29f),

                new Vector2(0.49f, 0.3f),
                new Vector2(0.49f, 0.59f),
                new Vector2(0.5f, 0),
                new Vector2(0.5f, 0.29f),

                new Vector2(0, 0.3f),
                new Vector2(0, 0.59f),
                new Vector2(1, 0),
                new Vector2(1, 0.29f),

                new Vector2(0.5f, 0.3f),
                new Vector2(0.5f, 0.59f),
                new Vector2(1, 0.3f),
                new Vector2(1, 0.59f),

                new Vector2(0, 0.6f),
                new Vector2(0, 0.9f),
                new Vector2(0.49f, 0.6f),
                new Vector2(0.49f, 0.9f),

                new Vector2(0.5f, 0.6f),
                new Vector2(0.5f, 0.9f),
                new Vector2(1, 0.6f),
                new Vector2(1, 0.9f),
            };

            mesh.SetUVs(1, uv2);
            return mesh;
        }

        [MenuItem("工具/模型/创建Mesh")]
        public static void Init()
        {
            GetWindow<CubeMeshCreator>().Show();
        }
    }
}
