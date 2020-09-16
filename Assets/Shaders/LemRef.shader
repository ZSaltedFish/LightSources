Shader "LIGHT_SOURCES/LemRef"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalTex("NormalTex", 2D) = "white" {}
        _LightRa ("LightRatex", range(0, 1)) = 1
        _GrassRefPow("镜面发射率", float) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull back
        ZWrite on
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 lightDir : TEXCOORED1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalTex;
            uniform fixed4 _LightColor0;
            uniform fixed _LightRa;
            uniform fixed _GrassRefPow;

            v2f vert (appdata_full v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed3 normalColor = UnpackNormal(tex2D(_NormalTex, i.uv));
                fixed3 normal = normalize(normalColor);
                //return fixed4(normalColor, 1);
                //fixed3 normal = i.normal;
                fixed4 lightColor = _LightColor0;
                fixed3 worldLight = normalize(i.lightDir);
                fixed3 camera = UNITY_MATRIX_V[2].xyz;
                
                //Lambert
                fixed d = max(0, dot(worldLight, normal));

                //BPhone
                fixed3 outLight =  dot(worldLight, normal) * 2 * normal - worldLight;
                fixed Bp = pow(saturate(-dot(camera, outLight)), _GrassRefPow);
                //return fixed4(worldLight, 1);

                return col * lightColor * (d * _LightRa + Bp * (1 - _LightRa));
            }
            ENDCG
        }
    }
}
