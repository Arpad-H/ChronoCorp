Shader "Custom/RadialSphereHP_WorldY_Centered"
{
    Properties
    {
        _HP ("HP", Range(0,1)) = 1

        [HDR]_FillColor ("Fill Color", Color) = (0,1,0,1)
        [HDR]_EmptyColor ("Empty Color", Color) = (1,0,0,1)

        _EdgeSoftness ("Edge Softness", Range(0.0001, 0.1)) = 0.01
        _StartOffset ("Start Offset", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 centerWS    : TEXCOORD1;
            };

            float _HP;
            float4 _FillColor;
            float4 _EmptyColor;
            float _EdgeSoftness;
            float _StartOffset;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS  = TransformObjectToWorld(v.positionOS.xyz);

                // Object world-space center (pivot)
                o.centerWS = TransformObjectToWorld(float3(0,0,0));

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Direction from sphere center in WORLD space
                float3 dirWS = normalize(i.positionWS - i.centerWS);

                // Project to XZ plane (rotate around WORLD Y)
                float2 xz = normalize(dirWS.xz);

                // Angle around world Y axis
                float angle = atan2(xz.x, xz.y);

                // -PI..PI → 0..1
                float angle01 = (angle + PI) / (2.0 * PI);

                // Rotate start direction
                angle01 = frac(angle01 + _StartOffset);

                // HP mask
                float slice = 1.0 - smoothstep(
                    _HP - _EdgeSoftness,
                    _HP + _EdgeSoftness,
                    angle01
                );

                return lerp(_EmptyColor, _FillColor, slice);
            }
            ENDHLSL
        }
    }
}
