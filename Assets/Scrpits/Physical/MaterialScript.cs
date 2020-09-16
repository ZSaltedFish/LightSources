using System.Collections.Generic;
using UnityEngine;

namespace LightRef
{
    //[ExecuteInEditMode]
    public class MaterialScript : MonoBehaviour
    {
        public Texture2D MainTex;
        public Texture2D NormalTex;
        public float LambertLerp;
        public float BlindPow;

        private Mesh _mesh;
        private Material _mat;

        private RenderObject _srcObj;

        public float LambertReflectionValue;
        public RenderObject SrcObject
        {
            get
            {
                if (_srcObj == null)
                {
                    _srcObj = GetComponent<RenderObject>();
                }
                return _srcObj;
            }
        }

        /// <summary>
        /// 吸光率
        /// </summary>
        public float Absorbance = 0;

        public Color[,] MainTexDic, NorTexDic;
        public void Awake()
        {
            Renderer render = GetComponent<Renderer>();
            _mat = render.sharedMaterial;
            
            if (render is MeshRenderer)
            {
                _mesh = render.GetComponent<MeshFilter>().sharedMesh;
            }

            if (render is SkinnedMeshRenderer)
            {
                _mesh = (render as SkinnedMeshRenderer).sharedMesh;
            }
        }
        public void Update()
        {
            _mat.SetTexture("_MainTex", MainTex);
            _mat.SetTexture("_NormalTex", NormalTex);
            _mat.SetFloat("_LightRa", LambertLerp);
            _mat.SetFloat("_GrassRefPow", BlindPow);
        }

        public List<Triangle> Initialize()
        {
            Awake();
            List<Triangle> _triangles = new List<Triangle>();

            MainTexDic = Tex2Dic(MainTex);
            NorTexDic = Tex2Dic(NormalTex);
            for (int i = 0; i < _mesh.triangles.Length; i += 3)
            {
                int pA = _mesh.triangles[i];
                int pB = _mesh.triangles[i + 1];
                int pC = _mesh.triangles[i + 2];
                Vector3 v1 = transform.TransformPoint(_mesh.vertices[pA]);
                Vector3 v2 = transform.TransformPoint(_mesh.vertices[pB]);
                Vector3 v3 = transform.TransformPoint(_mesh.vertices[pC]);

                Vector3 wN1 = _mesh.normals[pA];
                Vector3 wN2 = _mesh.normals[pB];
                Vector3 wN3 = _mesh.normals[pC];

                TriangleInitData data = new TriangleInitData()
                {
                    Ve1 = v1, Ve2 = v2, Ve3 = v3,
                    No1 = wN1, No2 = wN2, No3 = wN3,
                    Uv1 = _mesh.uv[pA], Uv2 = _mesh.uv[pB], Uv3 = _mesh.uv[pC],
                    Ta1 = _mesh.tangents[pA], Ta2 = _mesh.tangents[pB], Ta3 = _mesh.tangents[pC],
                    LocalTriax = transform.localToWorldMatrix,
                    Src = this,
                };

                _triangles.Add(new Triangle(data));
            }
            return _triangles;
        }

        private static Color[,] Tex2Dic(Texture2D tex)
        {
            Color[,] colors = new Color[tex.width, tex.height];
            for (int x = 0; x < tex.width; ++x)
            {
                for (int y = 0; y < tex.height; ++y)
                {
                    Color color = tex.GetPixel(x, y);
                    colors[x, y] = color;
                }
            }
            return colors;
        }
    }
}
