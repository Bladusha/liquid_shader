Shader "Custom/WaterPipeSimple"
{
    Properties
    {
        [MainColor] _WaterColor ("Water Color", Color) = (0.18, 0.72, 0.78, 0.82)
        _DeepWaterColor ("Deep Water Color", Color) = (0.06, 0.23, 0.33, 0.9)
        _SurfaceColor ("Surface Highlight", Color) = (0.82, 0.95, 1.0, 0.95)
        _BubbleColor ("Bubble Color", Color) = (0.92, 0.97, 1.0, 0.92)
        _FillLevel ("Fill Level", Range(0, 1)) = 0.5
        _min ("Path Start", Float) = 0
        _max ("Path End", Float) = 1
        _FrontTilt ("Front Tilt", Range(-1, 1)) = 0.12
        _FlowSpeed ("Flow Speed", Range(0, 5)) = 0.35
        _Temperature ("Water Temperature", Range(0, 1)) = 0.35
        _SurfaceOscillation ("Surface Oscillation", Range(0, 1)) = 0.3
        _BubbleSize ("Bubble Size", Range(0.5, 8)) = 1.25
        _BubbleAmount ("Bubble Amount", Range(0, 1)) = 0.35
        _SurfaceBand ("Surface Band", Range(0.001, 0.2)) = 0.03
        _FrontFade ("Front Fade", Range(0.001, 0.2)) = 0.06
        _EdgeFade ("Edge Fade", Range(0, 6)) = 2.0
        _FresnelPower ("Fresnel Power", Range(0.25, 8)) = 3.6
        _Alpha ("Alpha", Range(0, 1)) = 0.82
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
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _WaterColor;
            float4 _DeepWaterColor;
            float4 _SurfaceColor;
            float4 _BubbleColor;
            float _FillLevel;
            float _min;
            float _max;
            float _FrontTilt;
            float _FlowSpeed;
            float _Temperature;
            float _SurfaceOscillation;
            float _BubbleSize;
            float _BubbleAmount;
            float _SurfaceBand;
            float _FrontFade;
            float _EdgeFade;
            float _FresnelPower;
            float _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
            };

            float3 SafeNormalize3(float3 v, float3 fallback)
            {
                float lenSq = dot(v, v);
                return lenSq > 0.00001 ? v * rsqrt(lenSq) : fallback;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float BubbleField(float2 uv, float bubbleScale, float timeOffset, float radiusScale)
            {
                float2 gridUv = uv * bubbleScale;
                float2 baseCell = floor(gridUv);
                float2 fracUv = frac(gridUv);
                float bubble = 0.0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 cell = baseCell + float2(x, y);
                        float2 jitter = float2(
                            Hash21(cell + 11.37),
                            Hash21(cell.yx + 29.13));

                        float2 center = frac(float2(x, y) + jitter + float2(0.0, timeOffset));

                        float2 delta = fracUv - center;
                        delta -= round(delta);
                        float radius = lerp(0.10, 0.42, Hash21(cell + 57.91)) * radiusScale;
                        float dist = length(delta);
                        float body = smoothstep(radius, radius * 0.82, dist);
                        float rim = smoothstep(radius * 1.05, radius * 0.55, dist) * 0.65;
                        bubble = max(bubble, max(body * 0.55, rim));
                    }
                }

                return bubble;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normal = GetVertexNormalInputs(input.normalOS);

                output.positionCS = pos.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.positionWS = pos.positionWS;
                output.normalWS = SafeNormalize3(normal.normalWS, float3(0.0, 1.0, 0.0));
                output.uv = input.uv;
                output.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float pathMin = min(_min, _max);
                float pathMax = max(_min, _max);
                float pathCoord = saturate(input.uv.y);
                float fillPos = lerp(pathMin, pathMax, saturate(_FillLevel));
                float frontTilt = ((frac(input.uv.x) - 0.5) * 2.0) * _FrontTilt;
                float temperature = saturate(_Temperature);
                float oscillation = saturate(_SurfaceOscillation);
                float bubbleAmount = saturate(_BubbleAmount);
                float bubbleSize = max(_BubbleSize, 0.5);

                float riseSpeed = lerp(0.08, 1.4, temperature);
                float oscillationAmplitude = lerp(0.002, 0.02, oscillation);
                float frontWave = sin(pathCoord * 18.0 + _Time.y * lerp(0.7, 3.5, oscillation)) * oscillationAmplitude;
                float sideWave = sin(input.uv.x * 6.2831853 * 2.0 + _Time.y * lerp(0.5, 2.4, oscillation)) * (oscillationAmplitude * 0.6);
                float shimmer = (Noise2D(input.uv * float2(6.0, 24.0) + float2(_Time.y * _FlowSpeed * 0.25, _Time.y * 0.3)) - 0.5) * oscillationAmplitude;

                float2 waterUv = float2(input.uv.x, input.uv.y * 1.6 + _Time.y * _FlowSpeed * 0.18);
                float lowerBias = saturate(1.0 - pathCoord) * 0.02;
                float waterFront = fillPos + frontTilt + frontWave + sideWave + shimmer + lowerBias;
                float signedDistance = waterFront - pathCoord;

                clip(signedDistance + max(_FrontFade, 0.001));

                float frontMask = smoothstep(0.0, _FrontFade, signedDistance);
                float surfaceMask = 1.0 - smoothstep(0.0, _SurfaceBand, signedDistance);
                float depthMask = saturate(signedDistance / max(pathMax - pathMin, 0.0001));
                float edgeMask = pow(saturate(1.0 - depthMask), max(_EdgeFade, 0.001));

                float bubbleSize01 = saturate((bubbleSize - 0.5) / 7.5);
                float largeBubbleScale = lerp(16.0, 2.8, bubbleSize01);
                float mediumBubbleScale = lerp(22.0, 5.0, bubbleSize01);
                float smallBubbleScale = lerp(30.0, 8.0, bubbleSize01);

                float2 bubbleUvA = float2(input.uv.x * 1.0, input.uv.y * 2.8 - _Time.y * riseSpeed);
                float2 bubbleUvB = float2(frac(input.uv.x + 0.37), input.uv.y * 3.7 - _Time.y * (riseSpeed * 1.28 + 0.08));
                float2 bubbleUvC = float2(frac(input.uv.x * 1.7 + 0.19), input.uv.y * 4.6 - _Time.y * (riseSpeed * 1.55 + 0.16));

                float bubbleLayerLarge = BubbleField(bubbleUvA, largeBubbleScale, _Time.y * 0.11, lerp(0.65, 1.9, bubbleSize01));
                float bubbleLayerMedium = BubbleField(bubbleUvB, mediumBubbleScale, _Time.y * 0.16, lerp(0.42, 1.2, bubbleSize01));
                float bubbleLayerSmall = BubbleField(bubbleUvC, smallBubbleScale, _Time.y * 0.22, lerp(0.22, 0.6, bubbleSize01));

                float bubbleMask = bubbleLayerLarge * 0.9 + bubbleLayerMedium * 0.55 + bubbleLayerSmall * 0.22;
                bubbleMask *= lerp(0.12, 1.15, bubbleAmount);
                bubbleMask *= frontMask;
                bubbleMask *= smoothstep(0.02, 0.68 + temperature * 0.2, depthMask);
                bubbleMask = saturate(bubbleMask);

                float foamTrigger = saturate((bubbleAmount - 0.72) * 3.6 + (temperature - 0.55) * 1.8 + bubbleSize01 * 0.8);
                float2 foamUv = float2(input.uv.x * lerp(10.0, 5.0, bubbleSize01), input.uv.y * 6.5 - _Time.y * (riseSpeed * 0.65));
                float foamNoiseA = Noise2D(foamUv);
                float foamNoiseB = Noise2D(foamUv * 1.9 + float2(3.1, -2.4));
                float foamField = saturate(foamNoiseA * 0.75 + foamNoiseB * 0.65 - 0.58 + foamTrigger * 0.32);
                float foamMask = foamField;
                foamMask *= smoothstep(0.35, 0.98, bubbleAmount);
                foamMask *= smoothstep(0.25, 0.95, temperature + bubbleSize01 * 0.25);
                foamMask *= smoothstep(0.0, 0.24, signedDistance + _FrontFade * 0.65);
                foamMask = saturate(foamMask);

                float3 viewDirWS = SafeNormalize3(input.viewDirWS, float3(0.0, 0.0, 1.0));
                float fresnel = pow(1.0 - saturate(dot(viewDirWS, input.normalWS)), _FresnelPower);
                float bodyMix = Noise2D(waterUv * float2(2.0, 6.0) + float2(_Time.y * _FlowSpeed * 0.2, _Time.y * 0.15));
                float temperatureTint = smoothstep(0.3, 1.0, temperature);

                float3 baseColor = lerp(_DeepWaterColor.rgb, _WaterColor.rgb, saturate(0.15 + depthMask * 1.1));
                baseColor = lerp(baseColor, _WaterColor.rgb * 1.08, bodyMix * 0.35 + edgeMask * 0.15);
                baseColor = lerp(baseColor, _SurfaceColor.rgb, fresnel * 0.45 + surfaceMask * 0.2);
                baseColor = lerp(baseColor, _BubbleColor.rgb, bubbleMask * lerp(0.35, 0.92, temperatureTint));
                baseColor = lerp(baseColor, _BubbleColor.rgb, foamMask * 0.95);
                baseColor = lerp(baseColor * 0.88, baseColor, frontMask);

                float alpha = _Alpha;
                alpha += fresnel * 0.06;
                alpha += surfaceMask * 0.08;
                alpha += bubbleMask * 0.04;
                alpha += foamMask * 0.12;
                alpha *= frontMask;
                alpha = saturate(alpha);

                return half4(baseColor, alpha);
            }
            ENDHLSL
        }
    }
}
