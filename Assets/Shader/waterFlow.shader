Shader "Unlit/waterFlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _FlowMap ("FlowMap", 2D) = "white" {}
        _FlowSpeed ("FlowSpeed", Range(0, 1)) = 0.1
        [HDR]_FoamCol ("Foam Color", Color) = (1,1,1,1)
        [HDR]_WaterCol ("Water Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"    
            "RenderType"="Transparent"
        }

        LOD 100


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma shader_feature _REVERSE_FLOW_ON

            #include "UnityCG.cginc"

            // Properties
            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _FlowMap;
            half _FlowSpeed;
            half3 _FoamCol;
            half3 _WaterCol;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv0 : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half2 var_Flowmap = tex2D(_FlowMap, i.uv0).rg * 2.0 - 1.0; // remap to -1 to 1
                float phase0 = frac(_Time.y * _FlowSpeed);
                float phase1 = frac(_Time.y * _FlowSpeed + 0.5);
                half3 var_Maintex0 = tex2D(_MainTex, i.uv0 - var_Flowmap.xy * phase0).rgb;
                half3 var_Maintex1 = tex2D(_MainTex, i.uv0 - var_Flowmap.xy * phase1).rgb;
                half flowLerp = abs((0.5 - phase0) * 2.0);
                half3 finalRGB = lerp(var_Maintex0, var_Maintex1, flowLerp);
                
                return half4(lerp(_WaterCol, _FoamCol, finalRGB), 1);
            }
            ENDCG
        }
    }
}
