Shader "Custom/WaterDropletParticle"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Color", Color) = (0.62, 0.9, 1.0, 0.58)
        _Softness ("Softness", Range(0.001, 0.5)) = 0.18
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Softness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUv = input.uv * 2.0 - 1.0;
                float distanceFromCenter = length(centeredUv);
                float alpha = 1.0 - smoothstep(1.0 - _Softness, 1.0, distanceFromCenter);
                alpha *= saturate(1.0 - distanceFromCenter * 0.35);

                float4 color = _BaseColor * input.color;
                color.a *= alpha;
                return half4(color);
            }
            ENDHLSL
        }
    }
}
