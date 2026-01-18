Shader "Custom/URP_CRT_Fullscreen"
{
    Properties
    {
        _Curvature ("Curvature", Range(1, 20)) = 6
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.15
        _VignetteSoftness ("Vignette Softness", Range(0.01, 0.5)) = 0.15
        _ChromaticOffset ("Chromatic Offset", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "CRT"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // -------- Fullscreen triangle --------
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(uint vertexID : SV_VertexID)
            {
                Varyings o;

                float2 pos[3] =
                {
                    float2(-1, -1),
                    float2(-1, 3),
                    float2(3, -1)
                };

                float2 uv[3] =
                {
                    float2(0, 0),
                    float2(0, 2),
                    float2(2, 0)
                };

                o.positionCS = float4(pos[vertexID], 0, 1);
                o.uv = uv[vertexID];

                return o;
            }

            // -------- Inputs --------
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _Curvature;
            float _ScanlineStrength;
            float _VignetteSoftness;
            float _ChromaticOffset;

            // -------- CRT Distortion --------
            float2 CRTCurve(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = uv.yx / _Curvature;
                uv += uv * offset * offset;
                return uv * 0.5 + 0.5;
            }

            // -------- Fragment --------
            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = CRTCurve(i.uv);
                uv.y = 1.0 - uv.y;
                uv = saturate(uv);

                // ---- Chromatic aberration ----
                float2 pixel = 1.0 / _ScreenParams.xy;
                float chroma = _ChromaticOffset * pixel.x;

                half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + float2(chroma, 0)).r;
                half g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv).g;
                half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv - float2(chroma, 0)).b;

                half3 col = half3(r, g, b);

                // ---- Scanlines ----
                float scan = sin(uv.y * _ScreenParams.y * 1.5);
                col *= lerp(1.0, scan * 0.5 + 0.5, _ScanlineStrength);

                // ---- Vignette ----
                float2 v = smoothstep(0.0, _VignetteSoftness, uv)
                    * smoothstep(0.0, _VignetteSoftness, 1.0 - uv);

                col *= v.x * v.y;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}