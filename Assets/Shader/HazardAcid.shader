// Yerçekimiyle havada tutulan asit: hareketli ölümcül tehlikelerin dış kabuğu.
// Çekirdek makine (HazardCore) bunun üstüne child sprite olarak çizilir.
// Taban şekil = collider (1 birimlik daire ya da kutu, transform ölçeğiyle). Dalga ve diken
// yalnızca DIŞA taşar, yani görsel hiçbir zaman collider'dan küçük görünmez.
// Sprite, collider'dan büyük dolgulu bir quad olmalı (HazardAcid_Quad, 1.5 birim).
Shader "RockTheGame/HazardAcid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Shape)]
        [Enum(Circle,0,Box,1)] _Shape ("Shape", Float) = 0
        _CornerRadius ("Box Corner Radius (x min half size)", Range(0,1)) = 0.35
        _CoreFraction ("Core Size (x min size)", Range(0.1,1)) = 0.55

        [Header(Colors)]
        _AcidColor ("Acid Body", Color) = (0.349, 0.878, 0.278, 1)      // #59E047
        _AcidShade ("Acid Shade", Color) = (0.122, 0.729, 0.161, 1)     // #1FBA29
        _AcidHighlight ("Acid Highlight", Color) = (0.780, 1.0, 0.439, 1) // #C7FF70
        _AcidDeep ("Acid Deep (core contact)", Color) = (0.059, 0.478, 0.133, 1) // #0F7A22
        _OutlineColor ("Outline", Color) = (0.055, 0.141, 0.078, 1)     // #0E2414

        [Header(Line and Shading (world units))]
        _OutlineWidth ("Outline Width", Float) = 0.07
        _ShadeOffset ("Shade Crescent Offset", Float) = 0.12
        _HighlightWidth ("Highlight Width", Float) = 0.05
        _HighlightInset ("Highlight Inset", Float) = 0.06
        _CoreShadow ("Core Contact Band", Float) = 0.10
        _CoreShadowStrength ("Core Contact Strength", Range(0,1)) = 0.55
        _CoreBodyRatio ("Core Body Size (x core sprite)", Range(0.3,1)) = 0.72

        [Header(Living Edge (world units, outward only))]
        _WobbleAmp ("Wobble Amplitude", Float) = 0.05
        _WobbleWavelength ("Wobble Wavelength", Float) = 0.7
        _WobbleSpeed ("Wobble Speed", Float) = 2.2
        _SpikeAmp ("Spike Height", Float) = 0.035
        _SpikeSpacing ("Spike Spacing", Float) = 0.45
        _SpikeDrift ("Spike Drift (turns/sec)", Float) = 0.03

        [Header(Flow)]
        _FlowSpeed ("Swirl Speed (rad/sec)", Float) = 0.8
        _FlowScale ("Swirl Noise Scale (1/world)", Float) = 3.0
        _FlowAmount ("Swirl Streak Amount", Range(0,1)) = 0.45
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
            "DisableBatching"="True" // boyut unity_ObjectToWorld'den okunuyor (RisoSlab ile aynı)
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 localPos : TEXCOORD0; // nesne uzayı, dünya biriminde (gerilmemiş)
                float2 size     : TEXCOORD1; // taban şeklin dünya boyutu
            };

            fixed4 _Color, _RendererColor;
            float _Shape, _CornerRadius, _CoreFraction;
            fixed4 _AcidColor, _AcidShade, _AcidHighlight, _AcidDeep, _OutlineColor;
            float _OutlineWidth, _ShadeOffset, _HighlightWidth, _HighlightInset, _CoreShadow, _CoreShadowStrength, _CoreBodyRatio;
            float _WobbleAmp, _WobbleWavelength, _WobbleSpeed, _SpikeAmp, _SpikeSpacing, _SpikeDrift;
            float _FlowSpeed, _FlowScale, _FlowAmount;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 scale = float2(length(unity_ObjectToWorld._m00_m10_m20),
                                      length(unity_ObjectToWorld._m01_m11_m21));
                o.localPos = v.vertex.xy * scale;
                o.size = scale;
                o.color = v.color * _Color * _RendererColor;
                return o;
            }

            float sdBox (float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            float sdBase (float2 p, float2 hs)
            {
                float m = min(hs.x, hs.y);
                if (_Shape < 0.5) return length(p) - m;
                return sdBox(p, hs, _CornerRadius * m);
            }

            float hash21 (float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float vnoise (float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i), b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1)), d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.localPos;
                float2 hs = i.size * 0.5;
                float m = min(hs.x, hs.y);
                float t = _Time.y;

                // Kenarı canlı tut: kenar boyunca tam sayı sayıda dalga/diken, böylece dikiş yok.
                // Daire: açı. Kutu: çevre boyunca konum (uzun çubuklarda dikenler eşit aralıklı kalır).
                float ang = atan2(p.y, p.x);
                float perimR = m;
                if (_Shape > 0.5)
                {
                    float P = 4.0 * (hs.x + hs.y);
                    float u;
                    if (abs(p.x) * hs.y > abs(p.y) * hs.x)
                        u = (p.x > 0.0) ? (p.y + hs.y) : (2.0 * hs.y + 2.0 * hs.x + (hs.y - p.y));
                    else
                        u = (p.y > 0.0) ? (2.0 * hs.y + (hs.x - p.x)) : (4.0 * hs.y + 2.0 * hs.x + (p.x + hs.x));
                    ang = u / P * 6.2831853;
                    perimR = P / 6.2831853;
                }
                float nW = max(3.0, round(6.2831853 * perimR / max(_WobbleWavelength, 0.05)));
                float w1 = 0.5 + 0.5 * sin(ang * nW + t * _WobbleSpeed);
                float w2 = 0.5 + 0.5 * sin(ang * (nW + 2.0) - t * _WobbleSpeed * 0.7 + 1.7);
                float wobble = _WobbleAmp * w1 * w2;

                float nS = max(4.0, round(6.2831853 * perimR / max(_SpikeSpacing, 0.05)));
                float tri = 1.0 - abs(frac(ang * nS / 6.2831853 + t * _SpikeDrift) * 2.0 - 1.0);
                float spike = _SpikeAmp * smoothstep(0.45, 1.0, tri);

                float base = sdBase(p, hs);
                float d = base - wobble - spike;
                float aa = max(fwidth(d), 1e-4);
                float alpha = 1.0 - smoothstep(-aa, aa, d);
                if (alpha <= 0.0) return 0;

                // Gövde + dönen girdap çizgileri (alan asidi döndürüyor)
                float c = cos(t * _FlowSpeed), s = sin(t * _FlowSpeed);
                float2 q = float2(c * p.x - s * p.y, s * p.x + c * p.y);
                float n = vnoise(q * _FlowScale) * 0.65 + vnoise(q * _FlowScale * 2.3 + 7.1) * 0.35;
                float3 col = _AcidColor.rgb;
                col = lerp(col, lerp(_AcidColor.rgb, _AcidHighlight.rgb, 0.55), smoothstep(0.58, 0.66, n) * _FlowAmount);

                // Sağ-alt gölge hilali (ışık sol üstten)
                float2 lightOff = float2(-0.7071, 0.7071) * _ShadeOffset;
                float shadeD = sdBase(p - lightOff, hs) - wobble;
                col = lerp(col, _AcidShade.rgb, smoothstep(-aa, aa, shadeD));

                // Sol-üst parlama yayı
                float e = 0.01;
                float2 g = float2(sdBase(p + float2(e, 0), hs) - base, sdBase(p + float2(0, e), hs) - base);
                float facing = dot(normalize(g + 1e-6), float2(-0.7071, 0.7071));
                float hiInner = -(_OutlineWidth + _HighlightInset + _HighlightWidth);
                float hiOuter = -(_OutlineWidth + _HighlightInset);
                float hiBand = smoothstep(hiInner - aa, hiInner + aa, d) * (1.0 - smoothstep(hiOuter - aa, hiOuter + aa, d));
                col = lerp(col, _AcidHighlight.rgb, hiBand * smoothstep(0.45, 0.75, facing));

                // Çekirdeğin değdiği yerde koyu bant
                float coreHalf = _CoreFraction * m * _CoreBodyRatio;
                float coreD = (_Shape < 0.5) ? length(p) - coreHalf : sdBox(p, coreHalf.xx, coreHalf * 0.2);
                col = lerp(col, _AcidDeep.rgb, _CoreShadowStrength * (1.0 - smoothstep(0.0, _CoreShadow, coreD)));

                // Kontur
                col = lerp(col, _OutlineColor.rgb, smoothstep(-_OutlineWidth - aa, -_OutlineWidth + aa, d));

                float a = alpha * i.color.a;
                return fixed4(col * i.color.rgb * a, a);
            }
        ENDCG
        }
    }
}
