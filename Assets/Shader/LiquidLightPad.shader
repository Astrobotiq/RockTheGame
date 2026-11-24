Shader "Sprites/LiquidLightPad"
{
    // Bounce pad görseli: "sıvı ışık" gövde + AKAN PALET KENAR (docs/ui-research/bounce-pad/oneri.md, N3, mockup
    // `bounce-pad-yonler.html` "Akan palet kenar"). Tek SDF shader, şekil _Shape ile seçilir (0 dikdörtgen, 1 üçgen, 2 daire).
    //
    // Katmanlar (mockup'taki çizim sırası): koyu mürekkep gövde → içte akan sıvı dolgu (domain-warp, mor → magenta →
    // kırmızı → turuncu → pembe-beyaz) → gövdeyi saran akan paletli çerçeve (palet pedin merkezi etrafında
    // dönen konik gradyan, vuruşta kalınlaşır) → en dışta ince koyu halka. Çerçevesiz dalgalı kenar YOK.
    // Patlama ve halo bu shader'da değil, LiquidLightBurst'tadır.
    //
    // Ölçek: mockup'ta ped 170 px = 3.2 birim (1 px = 0.018824 birim); tüm uzunluklar o oranla birime çevrilmiştir.
    // Koordinatlar: sprite 1x1 birim ve nesne uzayında ±0.5 olmalı (tam dikdörtgen mesh, atlas dışı). Konum
    // IN.vertex.xy * _QuadSize ile dünya birimine çevrilir; _QuadSize dinlenme boyutudur, squash/stretch
    // transform ölçeğiyle gelir ve çizimin tamamını birlikte ezer.
    // Script (LiquidLightPad) her kare _Flow, _Hit ve _HueShift'i MaterialPropertyBlock ile verir.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Shape)]
        _Shape ("Shape (0 rect, 1 triangle, 2 circle)", Float) = 0
        _QuadSize ("Quad Size (world, rest)", Vector) = (3.8, 1.17, 0, 0)
        _BodySize ("Body Size (world)", Vector) = (3.2, 0.5647, 0, 0)
        _Round ("Corner Radius (world, 6 px)", Float) = 0.113

        [Header(Liquid)]
        _LiquidSpan ("Liquid Span across fill (mockup 4.2 x 1.1)", Vector) = (4.2, 1.1, 0, 0)
        _Dim ("Idle Brightness", Range(0.3, 1)) = 0.72
        _HitBrightness ("Hit Value Boost", Range(0, 0.5)) = 0.15
        _InnerInset ("Fill Inset (world, 3 px)", Float) = 0.0565
        _InkColor ("Ink (gövde + dış halka)", Color) = (0.0784, 0.0471, 0.1098, 1)

        [Header(Frame)]
        _FrameOffset ("Frame Center Offset (world, 1 px)", Float) = 0.0188
        _FrameWidth ("Frame Width Idle (world, 5 px)", Float) = 0.0941
        _FrameWidthHit ("Frame Width Hit (world, 8 px)", Float) = 0.1506
        _FrameSpin ("Frame Palette Spin (rad per flow)", Float) = 0.8
        _OutlineOffset ("Outer Ring Offset (world, 4.5 px)", Float) = 0.0847
        _OutlineWidth ("Outer Ring Width (world, 1.5 px)", Float) = 0.0282

        [Header(Script driven)]
        _Flow ("Flow Time (script)", Float) = 0
        _Hit ("Hit Envelope 0..1 (script)", Range(0, 1)) = 0
        _HueShift ("Hue Shift Radians (script)", Float) = 0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False"
            // Nesne uzayı konumuna (IN.vertex.xy) dayanır: Unity yan yana pad'leri tek parçada birleştirirse (dynamic batching)
            // konum dünya uzayına döner ve pad çizilmez. Birleştirme kapalı olmalı.
            "DisableBatching"="True"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "LiquidLightCommon.cginc"

            #define TAU 6.28318530718

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 local  : TEXCOORD0; // nesne uzayı xy (±0.5)
                fixed4 color  : COLOR;
            };

            float4 _QuadSize, _LiquidSpan;
            float _Dim, _HitBrightness, _InnerInset;
            fixed4 _InkColor;
            float _FrameOffset, _FrameWidth, _FrameWidthHit, _FrameSpin, _OutlineOffset, _OutlineWidth;
            float _Flow, _Hit, _HueShift;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.local = IN.vertex.xy;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = IN.local * _QuadSize.xy;
                float sd = bodySdf(p);

                // Katman maskeleri (hepsi aynı SDF'in kaydırılmış eşikleri).
                float bodyMask = 1.0 - aastep(0.0, sd);
                float fillMask = 1.0 - aastep(0.0, sd + _InnerInset);

                float frameHalf = 0.5 * lerp(_FrameWidth, _FrameWidthHit, _Hit);
                float frameMask = 1.0 - aastep(frameHalf, abs(sd - _FrameOffset));
                float ringMask = 1.0 - aastep(_OutlineWidth * 0.5, abs(sd - _OutlineOffset));

                // Sıvı dolgu: vuruşta parlar, palet nota rengine kayar.
                // Desen mockup'taki gibi dolgu dikdörtgenine göre eşlenir (sol-üst köşeden, x: 4.2, y: 1.1 aralık);
                // pad boyutu değişirse desen de onunla gerilir.
                float2 fillSize = max(_BodySize.xy - 2.0 * _InnerInset, 1e-3);
                float2 liquidUv = float2((p.x + fillSize.x * 0.5) / fillSize.x, (fillSize.y * 0.5 - p.y) / fillSize.y) * _LiquidSpan.xy;
                float v = liquid(liquidUv, _Flow) + _Hit * _HitBrightness;
                float3 fill = hueRotate(liquidPalette(v), _HueShift) * (_Dim + (1.0 - _Dim) * _Hit);

                // Akan palet çerçeve: pedin merkezi etrafında dönen konik gradyan, palet iki yönde yansıtılmış (dikişsiz).
                float t = frac((atan2(p.y, p.x) - _Flow * _FrameSpin) / TAU);
                float s = 1.0 - abs(2.0 * t - 1.0);
                float3 frame = hueRotate(liquidPalette(s), _HueShift);

                // Çizim sırası: koyu gövde → dolgu → çerçeve → dış halka.
                float3 rgb = _InkColor.rgb;
                float alpha = bodyMask;
                rgb = lerp(rgb, fill, fillMask);
                rgb = lerp(rgb, frame, frameMask);
                alpha = max(alpha, frameMask);
                rgb = lerp(rgb, _InkColor.rgb, ringMask * (1.0 - frameMask));
                alpha = max(alpha, ringMask);

                if (alpha <= 0.002) discard;

                fixed4 col = fixed4(toProjectSpace(rgb), alpha);
                col *= IN.color;
                return col;
            }
            ENDCG
        }
    }
}
