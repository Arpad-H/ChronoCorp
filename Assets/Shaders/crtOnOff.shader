Shader "Hidden/CRT_ShutOff_Star"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Collapse ("Collapse Amount", Range(0, 1)) = 0.0
        _Flash ("Flash Brightness", Range(0, 5)) = 0.0

        _StarSharpness ("Star Sharpness", Range(0.5, 8)) = 3.0
        _StarBoost ("Star Boost", Range(0, 2)) = 1.0

        _EdgeSoftness ("Edge Softness", Range(0.0005, 0.05)) = 0.01
    }

    SubShader
    {
        // Transparent so alpha actually matters
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            // Alpha blending + avoid writing depth for transparent pixels
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Collapse;
            float _Flash;
            float _StarSharpness;
            float _StarBoost;
            float _EdgeSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Centered UV in [-0.5..0.5]
                float2 p = i.uv - 0.5;
                float2 a = abs(p);

                // --- STAR MASK (spikes along X/Y) ---
                // Base "axis-aligned square" distance (0..0.5)
                float dSquare = max(a.x, a.y);

                // 0 on axes, 1 on diagonals
                float diag = min(a.x, a.y) / max(1e-5, max(a.x, a.y));

                // Raw star distance (can exceed 0.5 with boost)
                float starRaw = dSquare * (1.0 + _StarBoost * pow(diag, _StarSharpness));

                // Normalize so max remains ~0.5 (prevents mid-screen leftover at Collapse=0)
                float star = starRaw / (1.0 + _StarBoost);

                // Visible radius shrinks as Collapse increases
                float threshold = 0.5 * (1.0 - _Collapse);

                // Classic CRT horizontal squeeze (sample only)
                float2 sampleUV = p;
                sampleUV.x /= max(0.001, (1.0 - _Collapse * 0.9));

                fixed4 col = tex2D(_MainTex, sampleUV + 0.5);

                // --- Alpha-based collapse: outside becomes transparent ---
                float edge = _EdgeSoftness;

                // 1 = visible, 0 = transparent
                float alphaMask =  smoothstep(threshold - edge, threshold + edge, star);

                // Apply mask
                col.a *= alphaMask;

                // Optional: dim RGB with alpha so the image disappears instead of floating
                col.rgb *= alphaMask;

                // Flash affects RGB only (keep alpha driven by mask)
                col.rgb += (_Flash * (1.0 - _Collapse)).xxx;

                // Optional: discard fully transparent pixels (can reduce sorting artifacts)
                if (col.a <= 0.001)
                    discard;
col.rgb = 1-col.rgb;
                
                return col;
            }
            ENDCG
        }
    }
}
