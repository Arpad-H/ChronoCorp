Shader "Custom/MultiBulgePipe"
{
    Properties
    {
        [Header(Appearance)]
        _Color("Rim Color", Color) = (1, 0.4, 0.6, 1)    // Based on graph top color
        _Color2("Center Color", Color) = (0, 0.8, 1, 1)  // Based on graph bottom color
        _FresnelPower("Fresnel Power", Float) = 1.59     // From your Float node
        
        [Header(Deformation)]
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
                float3 normalWS   : TEXCOORD0; // Needed for Fresnel
                float3 viewDirWS  : TEXCOORD1; // Needed for Fresnel
            };

            float _BulgePositions[20]; 
            int _BulgeCount;
            float _BulgeRadius;
            float _BulgeStrength;

            float4 _Color;
            float4 _Color2;
            float _FresnelPower;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // --- Vertex Displacement Logic ---
                float mask = 0;
                for (int i = 0; i < _BulgeCount; i++)
                {
                    float dist = abs(input.uv.y - _BulgePositions[i]);
                    float currentMask = 1.0 - smoothstep(0.0, _BulgeRadius, dist);
                    mask = max(mask, currentMask);
                }

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz + (input.normalOS * mask * _BulgeStrength));
                
                // --- Pass Data to Fragment ---
                output.positionCS = TransformWorldToHClip(posWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(posWS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Re-normalize vectors
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                // --- Fresnel Logic (Matching Shader Graph) ---
                // Fresnel = pow(1.0 - saturate(dot(Normal, ViewDir)), Power)
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);
                
                // One Minus Fresnel for the center color
                float inverseFresnel = 1.0 - fresnel;

                // Combine colors: (Color * Fresnel) + (Color2 * (1 - Fresnel))
                float4 finalColor = (_Color * fresnel) + (_Color2 * inverseFresnel);

                return finalColor;
            }
            ENDHLSL
        }
    }
}