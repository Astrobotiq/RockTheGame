Shader "Sprites/LiquidLightBurst"
{
    // Bounce pad "kaleydoskop patlaması" (docs/ui-research/bounce-pad/oneri.md, N3): fırlatmada pedin SDF'ini takip ederek
    // her yöne dalga gibi açılan sıvı ışık. Açı _BurstSymmetry (3/4/5/6, enstrümandan) kadar katlanır; yüzey sıvı
    // alan + ses halkalarıyla desenlenir. Pedin gövdesinin içi çizilmez (gövde LiquidLightPad'in işi), bu quad arkada durur.
    //
    // Script (LiquidLightBurst) her kare _BurstAge, _BurstEnv (zarf × flaş şiddeti) ve _Flow verir. _BurstEnv flaş
    // sınırlayıcıdan gelir: WCAG 2.3.1 için aşırı sık patlamalar kısılır. Koordinatlar LiquidLightPad ile aynı (1x1 sprite).
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Shape)]
        _Shape ("Shape (0 rect, 1 triangle, 2 circle)", Float) = 0
        _QuadSize ("Quad Size (world)", Vector) = (16.8, 14.2, 0, 0)
        _BodySize ("Body Size (world)", Vector) = (3.2, 0.5647, 0, 0)
        _Round ("Corner Radius (world, 6 px)", Float) = 0.113
        _BodyOffset ("Body Center Offset (world, squash anchor; script)", Vector) = (0, 0, 0, 0)

        [Header(Burst)]
        _BurstSpeed ("Front Speed (world/s, 270 px/s)", Float) = 5.082
        _BurstLife ("Life (s)", Float) = 1.3
        _BurstStartRadius ("Start Distance (world, 18 px)", Float) = 0.339
        _BurstSymmetry ("Symmetry (3..6, script)", Float) = 3
        _BurstAge ("Age (s, script)", Float) = -1
        _BurstEnv ("Envelope x Intensity (script)", Range(0, 1)) = 0

        [Header(Script driven)]
        _Flow ("Flow Time (script)", Float) = 0
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
                float2 local  : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            float4 _QuadSize, _BodyOffset;
            float _BurstSpeed, _BurstLife, _BurstStartRadius, _BurstSymmetry, _BurstAge, _BurstEnv;
            float _Flow, _HueShift;

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
                float t = _BurstAge;
                if (t < 0.0 || t > _BurstLife || _BurstEnv <= 0.001) discard;

                float2 p = IN.local * _QuadSize.xy - _BodyOffset.xy;   // ezilmiş pedin merkezine göre
                float sd = bodySdf(p);                  // pedin kendi şekli: desen onu takip eder
                float front = _BurstStartRadius + t * _BurstSpeed;
                if (sd <= 0.0 || sd > front) discard;   // gövdenin içi LiquidLightPad'in

                // Açıyı pedin en-boyuna göre normalize edip simetri sayısı kadar katla (kaleydoskop).
                float2 half = _BodySize.xy * 0.5 + 0.376;   // mockup: yarım boyut + 20 px
                float th = atan2(p.y / half.y, p.x / half.x);
                float seg = TAU / max(_BurstSymmetry, 1.0);
                th = th - seg * floor(th / seg);
                th = abs(th - seg * 0.5);

                float u = sd * 1.859, v = th * 2.2;       // mockup: sd(px) * 0.035
                float val = liquid(float2(u * cos(v) * 3.0, u * sin(v) * 3.0 + v), _Flow * 1.5);
                val += 0.12 * sin(sd * 8.5 - t * 22.0);     // mockup: sd(px) * 0.16   // ses dalgası halkaları
                float3 col = hueRotate(liquidPalette(saturate(val)), _HueShift);

                float behind = front - sd;
                float band = min(1.0, behind / 0.565) * max(0.0, 1.0 - behind / 2.259);   // dışa giden dalga cephesi
                float halo = 0.7 * exp(-sd / 0.847);                                     // pedin çevresinde kalan parıltı
                float fall = exp(-sd / 4.141);
                float alpha = saturate(_BurstEnv * (band * fall + halo) * 1.3);

                // Mesh kenarında sertçe kesilmesin.
                float2 toEdge = _QuadSize.xy * 0.5 - abs(IN.local * _QuadSize.xy);
                alpha *= smoothstep(0.0, 0.4, min(toEdge.x, toEdge.y));
                if (alpha <= 0.002) discard;

                fixed4 o = fixed4(toProjectSpace(col), alpha);
                o *= IN.color;
                return o;
            }
            ENDCG
        }
    }
}
