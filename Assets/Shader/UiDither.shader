// Pause karartması (uGUI): mockup'taki dither + %50 ink örtüsü tek yüzeyde. Doku yok, CPU'dan mesh/uv yok.
//  - Desen: sanat piksellerinde 2x2'lik bloklar, dama düzeni; koyu bloklar tam örtü, aralar _Dim kadar örtü.
//    (mockup: önce 2x2 koyu dama, üstüne rgba(ink, .5): dama = 1, geri kalan = .5)
//  - Konum: mesh köşesi kök canvas uzayında gelir (UiConcrete gibi); _ArtPixel bir sanat pikselinin canvas birimi.
//  - Kayma: _Scroll (sanat pikseli, sağa ve aşağı). CPU her kare yalnızca bu bir vektörü yazar; Graphic'in rengi/mesh'i değişmez.
//  - Görünürlük (giriş/çıkış solması) Graphic.color alfasıyla gelir.
Shader "RockTheGame/UI/Dither"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint (ink)", Color) = (0.08,0.05,0.11,1)

        _Dim ("Dama disindaki ortu", Range(0,1)) = 0.5
        _ArtPixel ("Bir sanat pikselinin yerel birimi", Float) = 4
        _Scroll ("Kayma (sanat pikseli)", Vector) = (0,0,0,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 localPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Dim;
            float _ArtPixel;
            float4 _Scroll;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                OUT.localPos = v.vertex.xy;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sanat pikseli hücresi; kayma içerik sağa ve aşağı gitsin diye örnekleme noktası ters yöne kayar (canvas'ta y yukarı).
                float2 cell = floor(IN.localPos / max(_ArtPixel, 0.0001) - float2(_Scroll.x, -_Scroll.y));
                float2 block = floor(cell * 0.5);
                float checker = fmod(abs(block.x + block.y), 2.0) > 0.5 ? 1.0 : 0.0;

                fixed4 color = IN.color;
                color.a *= max(_Dim, checker);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
