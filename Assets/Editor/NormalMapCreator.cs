using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace LightRef
{
    public class NormalMapCreator : EditorWindow
    {
        private static Color _normalColor = new Color(0.5f, 0.5f, 1);
        private GameObject _nObj;
        public void OnGUI()
        {
            _nObj = EditorGUILayout.ObjectField("取法线对象", _nObj, typeof(GameObject), true) as GameObject;
            if (_nObj == null)
            {
                return;
            }
            MeshFilter meshR = _nObj.GetComponent<MeshFilter>();
            if (meshR == null)
            {
                EditorGUILayout.LabelField("不能获取模型");
                return;
            }

            if (GUILayout.Button("Do"))
            {
                Do();
            }
        }

        private void Do()
        {
            MeshFilter meshR = _nObj.GetComponent<MeshFilter>();
            MeshRenderer renderer = _nObj.GetComponent<MeshRenderer>();
            Material mat = renderer.sharedMaterial;
            Texture tex = mat.mainTexture;

            int width = tex.width, height = tex.height;

            Mesh mesh = meshR.sharedMesh;

            Texture2D nTex = new Texture2D(width, height);
            Vector2[] uv2pixs = new Vector2[mesh.vertexCount];

            for (int i = 0; i < mesh.vertexCount; ++i)
            {
                Vector2 v = mesh.uv[i];
                Vector3 normal = mesh.normals[i];

                int xPix = (int)(width * v.x);
                int yPix = (int)(height * v.y);
                uv2pixs[i] = new Vector2(xPix, yPix);
                Color color = normal.ToNormalColor();
                nTex.SetPixel(xPix, yPix, color);
            }
            nTex.Apply();

            #region 填充
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    Vector2 cur = new Vector2(x, y);
                    Vector2[] mins = MinDistance(cur, uv2pixs, out int[] indexs);
                    Vector2 ab = mins[1] - mins[0], ac = mins[2] - mins[0], bp = cur - mins[1], cp = cur - mins[2];

                    float sMain = TrangleS(ab, ac);
                    float wB = TrangleS(ac, cp);
                    float wC = TrangleS(ab, bp);

                    float b = wB / sMain, c = wC / sMain;
                    float a = 1 - b - c;

                    Color newColor;
                    if (a >= 0 && b >= 0 && c >= 0)
                    {
                        Vector2 pixA = uv2pixs[indexs[0]], pixB = uv2pixs[indexs[1]], pixC = uv2pixs[indexs[2]];
                        Color colorA = nTex.GetPixel((int)pixA.x, (int)pixA.y);
                        Color colorB = nTex.GetPixel((int)pixB.x, (int)pixB.y);
                        Color colorC = nTex.GetPixel((int)pixC.x, (int)pixC.y);
                        newColor = colorA * a + colorB * b + colorC * c;
                    }
                    else
                    {
                        newColor = _normalColor;
                    }
                    nTex.SetPixel(x, y, newColor);
                }
            }
            nTex.Apply();
            #endregion

            #region 写入
            byte[] bytes = nTex.EncodeToTGA();
            using (var w = new FileStream("Assets/Textures/normal.tga", FileMode.Create))
            {
                w.Write(bytes, 0, bytes.Length);
            }
            #endregion
        }

        private static float TrangleS(Vector2 v1, Vector2 v2)
        {
            float dot = Vector2.Dot(v1, v2.normalized);
            float lengthV1 = v1.magnitude;
            float height = Mathf.Sqrt(lengthV1 * lengthV1 - dot * dot);
            return v2.magnitude * height / 2;
        }

        private static Vector2[] MinDistance(Vector2 src, Vector2[] v2s, out int[] indexs)
        {
            Vector2 uv0 = v2s[0], uv1 = v2s[1], uv2 = v2s[2];
            float d0, d1, d2;
            d0 = d1 = d2 = float.MaxValue;
            indexs = new int[3];
            for (int i = 0; i < v2s.Length; ++i)
            {
                Vector2 cur = v2s[i];
                float dist = Vector2.Distance(src, cur);

                if (dist < d0)
                {
                    d2 = d1;
                    uv2 = uv1;
                    indexs[2] = indexs[1];

                    d1 = d0;
                    uv1 = uv0;
                    indexs[1] = indexs[0];

                    d0 = dist;
                    uv0 = cur;
                    indexs[0] = i;
                    continue;
                }
                if (dist < d1)
                {
                    d2 = d1;
                    uv2 = uv1;
                    indexs[2] = indexs[1];

                    d1 = dist;
                    uv1 = cur;
                    indexs[1] = i;
                    continue;
                }
                if (dist < d2)
                {
                    d2 = dist;
                    uv2 = cur;
                    indexs[2] = i;
                    continue;
                }
            }
            return new Vector2[]{ uv0, uv1, uv2};
        }

        [MenuItem("工具/模型/法线生成器")]
        public static void Init()
        {
            var win = GetWindow<NormalMapCreator>();
            win.Show();
        }

        private void OnDestroy()
        {
            _nObj = null;
        }
    }
}
