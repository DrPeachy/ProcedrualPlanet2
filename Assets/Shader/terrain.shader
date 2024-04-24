Shader "Unlit/terrain"
{
    Properties
    {

        _MainColor ("Color", Color) = (1,1,1,1)
        _SubColor ("Sub Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MatCapGrass ("Grass", 2D) = "grey" {}
        _MatCapStone ("Stone", 2D) = "grey" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _WarpTex ("Warp Texture", 2D) = "black" {}
        [PowerSlider(4)]_Freq ("Frequency", Range(0.1, 5)) = 5
        [PowerSlider(3)]_Amp ("Amplitude", Range(0, 1.2)) = 0
        _Seed ("Seed", Range(0, 100)) = 0
        _Octaves ("Octaves", Range(1, 8)) = 1
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 1
        _EnvSpecIntensity ("Env Spec Intensity", Range(0, 1)) = 0.5
        
        
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
            uniform half4 _MainColor;
            uniform half4 _SubColor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _MatCapStone;
            uniform sampler2D _NormalMap;
            uniform sampler2D _WarpTex;
            uniform half _Freq;
            uniform half _Amp;
            uniform half _Seed;
            uniform int _Octaves;
            uniform half _FresnelPower;
            uniform half _EnvSpecIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 posWS : TEXCOORD1;
                half3 nDirWS : TEXCOORD2;
                half3 tDirWS : TEXCOORD3;
                half3 bDirWS : TEXCOORD4;
                half3 lDirWS : TEXCOORD5;
                half vertHeight : TEXCOORD6;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.nDirWS = UnityObjectToWorldNormal( v.normal );
                o.tDirWS = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                o.bDirWS = normalize(cross( o.nDirWS, o.tDirWS ) * v.tangent.w);
                o.lDirWS = _WorldSpaceLightPos0.xyz;

                o.vertHeight = length(v.vertex);
                for (int i = 0; i < _Octaves; i++){
                    v.vertex += (1.0 / pow(2, i)) * _Amp * float4(perlin(v.vertex.xyz, _Freq + pow(2.0, i), _Seed) * normalize(v.normal), 1.0);
                }
                o.vertHeight = length(v.vertex) - o.vertHeight;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.posWS = mul( unity_ObjectToWorld, v.vertex );
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // vectors preparation
                half3 var_MainTex = tex2D(_MainTex, i.uv).rgb;
                half3 nDirTS = UnpackNormal(tex2D(_NormalMap, i.uv)).rgb;
                half3x3 tbn = float3x3(i.tDirWS, i.bDirWS, i.nDirWS);
                half3 nDirWS = normalize( mul(nDirTS, tbn));
                half3 nDirVS = normalize(mul(UNITY_MATRIX_V, float4(nDirWS, 0.0)));
                half3 vDirWS = normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz);
                half3 vrDirWS = reflect(-vDirWS, nDirWS);
                half2 matcapUV = nDirVS.rg * 0.5 + 0.5;
                half nDotv = dot(nDirWS, vDirWS);
                // product
                half nDotl = dot(i.nDirWS, i.lDirWS);


                half halfLambert = pow(nDotl * 0.5 + 0.5, 2.0);
                // half3 var_MatcapGrass = tex2D(_MatCapGrass, matcapUV).rgb;
                half3 var_MatcapStone = tex2D(_MatCapStone, matcapUV).rgb;
                half fresnel = pow(1.0- max(0, nDotv), _FresnelPower);
                half3 envSpecLight = var_MainTex.rgb * _EnvSpecIntensity * fresnel;
                // lighting
                // sample the texture
                half var_WarpTex = tex2D(_WarpTex, float2(halfLambert, 0.2)).r;
                half3 finalRGB = half3(lerp(_SubColor.r, _MainColor.r, var_WarpTex), lerp(_SubColor.g, _MainColor.g, var_WarpTex), lerp(_SubColor.b, _MainColor.b, var_WarpTex));
                return half4(envSpecLight, 1.0);
            }
            ENDCG
        }
    }
}
