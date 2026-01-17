Shader "Robo shaders/Structures_URP"
{
    Properties
    {
        [Header(Base Maps)]
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Base Color Tint", Color) = (1,1,1,1)
        _Brightness("Brightness", Range(0, 2)) = 1
        
        [Header(Painted Logic)]
        _PaintedMask("Painted Mask (R)", 2D) = "black" {}
        _PaintedColor("Painted Color", Color) = (1,1,1,1)

        [Header(Surface Attributes)]
        [Normal] _NormalMap("Normal", 2D) = "bump" {}
        _MaskMap("Metallic(R) Smoothness(A)", 2D) = "white" {}
        _Smoothness("Smoothness Scale", Range(0, 1)) = 1
        
        [Header(Emission and Flicker)]
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionMap("Emission", 2D) = "black" {}
        _FlickerIntensity("Flicker Intensity", Range(0, 20)) = 1
        _FlickerSpeed("Flicker Speed", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST; // The TRANSFORM_TEX macro uses this
                float4 _BaseColor;
                float4 _PaintedColor;
                float4 _EmissionColor;
                float _Brightness;
                float _Smoothness;
                float _FlickerIntensity;
                float _FlickerSpeed;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_PaintedMask); SAMPLER(sampler_PaintedMask);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap); // Fixed: Removed _ST here
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Albedo & Brightness
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                albedo.rgb *= _Brightness;

                // 2. Painted Mask Logic (as per your original shader)
                float paintedMask = SAMPLE_TEXTURE2D(_PaintedMask, sampler_PaintedMask, input.uv).r;
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * _PaintedColor.rgb, paintedMask);

                // 3. Metallic & Smoothness
                half4 mask = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv);
                half metallic = mask.r;
                half smoothness = mask.a * _Smoothness;

                // 4. Flicker Logic
                float flicker = max(sin(_Time.y * _FlickerSpeed) * _FlickerIntensity, 1.0);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;
                emission *= _EmissionColor.rgb * flicker;

                // 5. Final Lighting setup
                InputData inputData = (InputData)0;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - inputData.normalWS); // Basic view dir
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.emission = emission;
                surfaceData.alpha = 1.0;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}