Shader "Unlit/terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Color", Color) = (1,1,1,1)
        _SubColor ("Sub Color", Color) = (1,1,1,1)
        _WarpTex ("Warp Texture", 2D) = "black" {}
        [PowerSlider(4)]_Freq ("Frequency", Range(0.1, 5)) = 5
        [PowerSlider(3)]_Amp ("Amplitude", Range(0, 1.2)) = 0
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
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "cginc/utils.cginc"


            // properties
            half4 _MainColor;
            half4 _SubColor;
            sampler2D _MainTex;
            sampler2D _WarpTex;
            float4 _MainTex_ST;
            half _Freq;
            half _Amp;
            half _Seed;

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
                half3 nDirWS : TEXCOORD1;
                half3 lDirWS : TEXCOORD2;
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
                half var_WarpTex = tex2D(_WarpTex, float2(halfLambert, 0.2)).r;
                half3 finalRGB = half3(lerp(_SubColor.r, _MainColor.r, var_WarpTex), lerp(_SubColor.g, _MainColor.g, var_WarpTex), lerp(_SubColor.b, _MainColor.b, var_WarpTex));
                return half4(finalRGB, 1.0);
            }
            ENDCG
        }
    }
}
