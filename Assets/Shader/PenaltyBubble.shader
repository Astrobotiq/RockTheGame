Shader "Sprites/PenaltyBubble"
{
    // Multiplayer ceza balonu (docs/multiplayer-ceza-balonu-plani.md → BB4). Sıvı-ışık bounce pad ailesinden:
    // aynı palet (LiquidLightCommon), ince parlak kenar ve nokta dolgu; piksel değil yumuşak baskı.
    //
    // Katmanlar: hafif saydam pus → kenara doğru büyüyen halftone noktalar (merkezde ince, kenarda birleşir) →
    // yanardöner ince kenar (palet daire etrafında akar) → üst-solda parlak yay. _Pop 0 → 1: pus hızla söner,
    // kenar halkası dışa yayılıp incelir ve solar (patlama halkası).
    //
    // Koordinatlar: sprite 1x1 birim ve nesne uzayında ±0.5 olmalı (tam dikdörtgen mesh). Quad, balon çapının
    // _QuadScale katıdır (patlama halkasına yer kalsın); balonun kenarı r = 1'dedir. Şişme ve esneme transform
    // ölçeğiyle gelir (script). Script _Flow, _Pop ve _Alpha'yı MaterialPropertyBlock ile verir.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [Header(Shape)]
        _QuadScale ("Quad / Bubble Diameter", Float) = 1.4
        _RimWidth ("Rim Half Width (r units)", Range(0.01, 0.12)) = 0.035

        [Header(Halftone)]
        _DotCell ("Dot Cell Size (r units)", Range(0.03, 0.2)) = 0.085
        _DotAngle ("Dot Grid Angle (rad)", Float) = 0.26
        _DotMin ("Dot Radius at Center (cell)", Range(0, 0.3)) = 0.06
        _DotMax ("Dot Radius at Rim (cell)", Range(0.2, 0.55)) = 0.46
        _Haze ("Haze Alpha (center / rim)", Vector) = (0.05, 0.16, 0, 0)
        _Pastel ("Pastel Mix (palette to white)", Range(0, 1)) = 0.3

        [Header(Script driven)]
        _Flow ("Flow Time (script)", Float) = 0
        _Rot ("Object Rotation Radians (script)", Float) = 0
        _Pop ("Pop 0..1 (script)", Range(0, 1)) = 0
        _Alpha ("Alpha (script)", Range(0, 1)) = 1

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
            // Nesne uzayı konumuna (IN.vertex.xy) dayanır; toplu çizim konumu dünya uzayına çevirir.
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

            float _QuadScale, _RimWidth, _DotCell, _DotAngle, _DotMin, _DotMax, _Pastel;
            float4 _Haze;
            float _Flow, _Rot, _Pop, _Alpha;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.local = IN.vertex.xy;
                OUT.color = IN.color;
                return OUT;
            }

            // Açıya göre dönen palet; iki yöne yansıtılmış, yani dikişsiz.
            float3 ringPalette(float angle01, float flow)
            {
                float t = frac(angle01 - flow * 0.08);
                float s = 1.0 - abs(2.0 * t - 1.0);
                float3 c = liquidPalette(0.15 + 0.85 * s);
                return lerp(c, float3(1, 1, 1), _Pastel);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Balonun yarıçapı 1: kenar r = 1.
                float2 q = IN.local * 2.0 * _QuadScale;
                // Nesne dönüp esnerken nokta ızgarası ve parlak yay dünyaya sabit kalsın.
                float cr = cos(_Rot), sr = sin(_Rot);
                q = float2(cr * q.x - sr * q.y, sr * q.x + cr * q.y);
                float r = length(q);
                float aa = max(fwidth(r), 1e-4) * 0.75;
                float angle01 = atan2(q.y, q.x) / TAU + 0.5;

                float inside = 1.0 - smoothstep(1.0 - aa, 1.0 + aa, r);
                float bodyLife = 1.0 - smoothstep(0.0, 0.3, _Pop); // patlamada pus ve noktalar hızla söner

                // Pus: merkezde neredeyse yok, kenara doğru koyulaşır.
                float edge = smoothstep(0.25, 1.0, r);
                float hazeA = lerp(_Haze.x, _Haze.y, edge * edge);
                float3 rgb = float3(0.86, 0.95, 1.0);
                float alpha = hazeA;

                // Halftone noktalar: döndürülmüş ızgara, yarıçapı kenara doğru büyür; kenarda birbirine değer.
                float ca = cos(_DotAngle), sa = sin(_DotAngle);
                float2 g = float2(ca * q.x - sa * q.y, sa * q.x + ca * q.y) / _DotCell;
                float2 cell = frac(g) - 0.5;
                float d = length(cell);
                float dotR = lerp(_DotMin, _DotMax, edge);
                float w = max(fwidth(d), 1e-4) * 0.75;
                float dotMask = 1.0 - smoothstep(dotR - w, dotR + w, d);
                float dotA = dotMask * lerp(0.28, 0.7, edge);
                float3 dotCol = lerp(liquidPalette(liquid(q * 1.6, _Flow)), float3(1, 1, 1), _Pastel);
                rgb = lerp(rgb, dotCol, dotA / max(dotA + alpha, 1e-4));
                alpha = max(alpha, dotA);

                // Yanardöner ince kenar.
                float3 rimCol = ringPalette(angle01, _Flow);
                float rimMask = 1.0 - smoothstep(_RimWidth - aa, _RimWidth + aa, abs(r - (1.0 - _RimWidth)));
                rgb = lerp(rgb, rimCol, rimMask);
                alpha = max(alpha, rimMask);

                // Üst-solda parlak yay (cam balon hissi): kenarın hemen içinde, ~135° civarında.
                float arc = smoothstep(0.55, 1.0, cos(angle01 * TAU - 2.35 - 3.14159265));
                float arcBand = 1.0 - smoothstep(0.05, 0.05 + 0.04, abs(r - 0.84));
                float highlight = arc * arcBand * 0.9;
                rgb = lerp(rgb, float3(1, 1, 1), highlight);
                alpha = max(alpha, highlight);

                alpha *= inside * bodyLife;

                // Patlama halkası: kenardan dışa yayılır, incelir ve solar.
                if (_Pop > 0.0)
                {
                    float spread = 1.0 - pow(1.0 - _Pop, 3.0);
                    float ringR = 1.0 + 0.32 * spread;
                    float ringHalf = lerp(0.05, 0.008, _Pop);
                    float ring = (1.0 - smoothstep(ringHalf - aa, ringHalf + aa, abs(r - ringR))) * (1.0 - _Pop);
                    rgb = lerp(rgb, rimCol, ring);
                    alpha = max(alpha, ring);
                }

                alpha *= _Alpha;
                if (alpha <= 0.002) discard;

                fixed4 col = fixed4(toProjectSpace(rgb), alpha);
                col.a *= IN.color.a;
                return col;
            }
            ENDCG
        }
    }
}
