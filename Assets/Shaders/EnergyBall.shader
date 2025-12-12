Shader "Hidden/EnergyBall"
{
    Properties
    {
        _Color("Base Color", Color) = (0.2,0.8,1,1)
        _Opacity("Opacity", Range(0,1)) = 0.75

        _NoiseTex("Noise (tileable)", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 3.0
        _NoiseSpeed("Noise Speed", Float) = 0.4
        _DarkenStrength("Noise Darken Strength", Range(0,1)) = 0.7

        _DistortionStrength("Vertex Distortion", Float) = 0.12

        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.5,8)) = 3.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float4 _Color;
            float _Opacity;
            float _NoiseScale;
            float _NoiseSpeed;
            float _DarkenStrength;

            float _DistortionStrength;
            float4 _RimColor;
            float _RimPower;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 objPos : TEXCOORD0;
                float3 objNormal : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            // Fake procedural noise for vertex displacement (no texture sampling)
            float FakeNoise(float3 p)
            {
                return sin(p.x * 5 + _Time.y * _NoiseSpeed) *
                       sin(p.y * 5 + _Time.y * _NoiseSpeed * 1.3) *
                       sin(p.z * 5 + _Time.y * _NoiseSpeed * 0.7);
            }

            v2f vert(appdata v)
            {
                v2f o;
                
                float3 objPos = v.vertex.xyz;
                float3 objNormal = normalize(v.normal);

                // Simple procedural distortion
                float n = FakeNoise(objPos);
                float3 displaced = objPos + objNormal * (n * _DistortionStrength);

                float4 worldPos = mul(unity_ObjectToWorld, float4(displaced, 1));

                o.pos = UnityObjectToClipPos(float4(displaced, 1));
                o.objPos = displaced;
                o.objNormal = objNormal;

                o.worldNormal = mul((float3x3)unity_ObjectToWorld, objNormal);
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos.xyz);

                return o;
            }

            // Triplanar sampling — fragment only
            float SampleTri(sampler2D tex, float3 pos, float3 normal)
            {
                float3 n = abs(normal);
                n = n / (n.x + n.y + n.z);

                float2 uvX = pos.yz * _NoiseScale;
                float2 uvY = pos.zx * _NoiseScale;
                float2 uvZ = pos.xy * _NoiseScale;

                float sx = tex2D(tex, uvX).r;
                float sy = tex2D(tex, uvY).r;
                float sz = tex2D(tex, uvZ).r;

                return sx*n.x + sy*n.y + sz*n.z;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 objDir = normalize(i.objPos);
                float3 nrm = normalize(i.objNormal);

                float t = _Time.y * _NoiseSpeed;

                float n = SampleTri(_NoiseTex, objDir + t, nrm);

                float3 col = _Color.rgb * lerp(1.0, 1.0 - _DarkenStrength, n);

                float fresnel = pow(1 - saturate(dot(normalize(i.worldNormal), normalize(i.viewDir))), _RimPower);
                col += _RimColor.rgb * fresnel * 0.6;

                float alpha = _Opacity;

                return float4(col, alpha);
            }
            ENDCG
        }
    }

    FallBack Off
}
