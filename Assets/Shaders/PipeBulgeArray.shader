Shader "Custom/MultiBulgePipe"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _BulgeRadius("Bulge Radius", Float) = 0.1
        _BulgeStrength("Bulge Strength", Float) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            // These are set via C# Script
            float _BulgePositions[20]; 
            int _BulgeCount;
            
            float _BulgeRadius;
            float _BulgeStrength;
            float4 _BaseColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float mask = 0;
                // Loop through all active bulges
                for (int i = 0; i < _BulgeCount; i++)
                {
                    float dist = abs(input.uv.y - _BulgePositions[i]);
                    // Create a smooth falloff for the bulge
                    float currentMask = 1.0 - smoothstep(0.0, _BulgeRadius, dist);
                    mask = max(mask, currentMask);
                }

                // Displace position along the normal
                float3 worldPos = input.positionOS.xyz + (input.normalOS * mask * _BulgeStrength);
                
                output.positionCS = TransformObjectToHClip(worldPos);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}