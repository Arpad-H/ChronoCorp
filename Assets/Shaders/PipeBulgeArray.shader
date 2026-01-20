Shader "Custom/MultiBulgePipeTransparent"
{
    Properties
    {
        [Header(Appearance)]
        _Color("Rim Color", Color) = (1, 0.4, 0.6, 1)    
        _Color2("Center Color", Color) = (0, 0.8, 1, 1)  
        _FresnelPower("Fresnel Power", Float) = 1.59     
        
        // Add an Opacity slider
        _Opacity("Master Opacity", Range(0, 1)) = 0.5

        [Header(Deformation)]
        _BulgeRadius("Bulge Radius", Float) = 0.1
        _BulgeStrength("Bulge Strength", Float) = 0.2
    }

    SubShader
    {
        // 1. Change RenderType and Queue to Transparent
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline" 
        }

        Pass
        {
            // 2. Set the Blend Mode (SrcAlpha OneMinusSrcAlpha is standard transparency)
            // ZWrite Off is typical for transparent objects to avoid clipping issues
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

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
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
            };

            float _BulgePositions[20]; 
            int _BulgeCount;
            float _BulgeRadius;
            float _BulgeStrength;

            float4 _Color;
            float4 _Color2;
            float _FresnelPower;
            float _Opacity; // Defined from Properties

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float mask = 0;
                for (int i = 0; i < _BulgeCount; i++)
                {
                    float dist = abs(input.uv.y - _BulgePositions[i]);
                    float currentMask = 1.0 - smoothstep(0.0, _BulgeRadius, dist);
                    mask = max(mask, currentMask);
                }

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz + (input.normalOS * mask * _BulgeStrength));
                
                output.positionCS = TransformWorldToHClip(posWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(posWS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);
                float inverseFresnel = 1.0 - fresnel;

                // 3. Calculate final color and apply alpha
                float4 finalColor = (_Color * fresnel) + (_Color2 * inverseFresnel);
                
                // Use the alpha from your colors multiplied by the Master Opacity
                float alpha = finalColor.a * _Opacity;

                return half4(finalColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}