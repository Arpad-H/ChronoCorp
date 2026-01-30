Shader "Custom/UI/SpriteGradientTransparent"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color  ("Left Color",  Color) = (1,1,1,1)
        _Color2 ("Right Color", Color) = (1,1,1,1)
        _Scale ("Scale", Float) = 1

        // Required for Unity UI masking/stencil
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // UI also uses these internally (kept for compatibility)
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // UI-style render state
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // UI masking variants
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            fixed4 _Color2;
            float _Scale;

            // Provided by UnityUI.cginc
            float4 _ClipRect;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;      // IMPORTANT: includes CanvasGroup alpha
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv    : TEXCOORD0;
                float4 worldPos : TEXCOORD1; // for UI clip rect
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);

                // Gradient factor across X (optionally scaled)
                float t = saturate(v.uv.x * _Scale);

                // Gradient color
                fixed4 grad = lerp(_Color, _Color2, t);

                // Multiply by UI vertex color (tint + CanvasGroup alpha)
                o.color = grad * v.color;

                // For RectMask2D / Clip Rect
                o.worldPos = v.vertex;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample sprite alpha (and color if you want to keep sprite RGB too)
                fixed4 tex = tex2D(_MainTex, i.uv);

                // If you want ONLY gradient (like your original), use tex.a only:
                fixed4 c = i.color;
                c.a *= tex.a;

                // If you want gradient tinted by sprite RGB, do:
                // fixed4 c = i.color * tex;  // (this uses sprite RGB too)

                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a - 0.001);
                #endif

                return c;
            }
            ENDCG
        }
    }
}
