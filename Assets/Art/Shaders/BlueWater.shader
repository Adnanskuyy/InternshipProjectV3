Shader "Custom/BlueWater"
{
    Properties
    {
        _Color1("Color 1", Color) = (0.05, 0.15, 0.4, 1.0)
        _Color2("Color 2", Color) = (0.1, 0.3, 0.6, 1.0)
        _Color3("Color 3", Color) = (0.15, 0.5, 0.8, 1.0)
        _Speed("Speed", Float) = 0.6
        _Scale("Scale", Float) = 4.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Background" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color1;
                float4 _Color2;
                float4 _Color3;
                float _Speed;
                float _Scale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv * _Scale;
                float t = _Time.y * _Speed;
                
                // Create a swirling water effect similar to Balatro's hypnotic background
                uv.x += sin(uv.y * 1.5 + t) * 0.4;
                uv.y += cos(uv.x * 1.5 + t * 0.8) * 0.4;
                
                float val = sin(uv.x * 2.0 + t) * cos(uv.y * 2.0 + t) * 0.5 + 0.5;
                
                half4 col;
                if (val < 0.5) {
                    col = lerp(_Color1, _Color2, val * 2.0);
                } else {
                    col = lerp(_Color2, _Color3, (val - 0.5) * 2.0);
                }
                
                return col;
            }
            ENDHLSL
        }
    }
}
