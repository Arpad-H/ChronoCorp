Shader "Custom/URP_CRT"
{
    Properties
    {
        _Curvature ("Curvature", Float) = 5.0
        _VignetteWidth ("Vignette Softness", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // ✅ Correct texture for URP Full Screen Pass
            TEXTURE2D(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);

            float _Curvature;
            float _VignetteWidth;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Convert to -1..1 space
                float2 uv = i.uv * 2.0 - 1.0;

                // ---- CRT curvature ----
                float curvature = max(_Curvature, 0.01);
                float2 offset = uv.yx / curvature;
                uv += uv * offset * offset;

                // Back to 0..1
                uv = uv * 0.5 + 0.5;

                // Clamp instead of hard discard
                uv = saturate(uv);

                // ---- Sample screen ----
                half4 col = SAMPLE_TEXTURE2D(
                    _CameraColorTexture,
                    sampler_CameraColorTexture,
                    uv
                );

                // ---- Scanlines ----
                float scanline = sin(uv.y * _ScreenParams.y * 1.5);
                col.rgb *= (scanline * 0.1) + 0.9;

                // ---- Vignette ----
                float2 vignette =
                    smoothstep(0.0, _VignetteWidth, uv) *
                    smoothstep(0.0, _VignetteWidth, 1.0 - uv);

                col.rgb *= vignette.x * vignette.y;

                return col;
            }
            ENDHLSL
        }
    }
}
