Shader "Unlit/terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Freq ("Frequency", Range(0.1, 10)) = 10
        _Amp ("Amplitude", Range(0.1, 10)) = 1
        _Seed ("Seed", Range(0, 100)) = 0
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "cginc/utils.cginc"


            // properties
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Freq;
            float _Amp;
            float _Seed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 nDirWS : TEXCOORD1;
                float3 lDirWS : TEXCOORD2;
            };


            v2f vert (appdata v)
            {
                v2f o;
                v.vertex += _Amp * float4(perlin(v.vertex.xyz, _Freq, _Seed) * normalize(v.normal), 1.0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.nDirWS = UnityObjectToWorldNormal( v.normal );
                o.lDirWS = _WorldSpaceLightPos0.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // vectors preparation
                half nDotl = dot(i.nDirWS, i.lDirWS);

                half halfLambert = pow(nDotl * 0.5 + 0.5, 2.0);

                // lighting
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return halfLambert;
            }
            ENDCG
        }
    }
}
