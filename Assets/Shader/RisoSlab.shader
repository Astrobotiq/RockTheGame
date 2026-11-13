// "Riso levha" hareketli platform görünümü (docs/ui-research/halftone/implementation-plan.md).
// H2: şekil (SDF levha), çizgi, kâğıt. H3: üç mürekkep katmanı (teal 15° / sarı 45° / pembe 75°), gren,
// temiz üst şerit, ince platform bantları, hıza bağlı kayık baskı.
// Sprite: 1x1 dünya birimlik tek beyaz kare (pivot merkez). Platform boyutu lossyScale'den okunur,
// desen platformla birlikte taşınır ve döner ama gerilmez. Piksel yuvarlama YOK (smoothstep + fwidth).
// Şekil ve nokta kafesi yerel uzayda; "üst" (temiz şerit, v rampası) dünya yukarısına göre hesaplanır.
Shader "Sprites/Custom/RisoSlab"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Colors)]
        _PaperColor ("Paper", Color) = (0.945, 0.902, 0.780, 1)   // #F1E6C7
        _LineColor ("Line Ink", Color) = (0.078, 0.047, 0.110, 1) // #140C1C
        _TealInk ("Teal Ink", Color) = (0.157, 0.604, 0.643, 1)   // #289AA4
        _YellowInk ("Yellow Ink", Color) = (0.988, 0.682, 0.016, 1) // #FCAE04
        _PinkInk ("Pink Ink", Color) = (1.0, 0.318, 0.871, 1)     // #FF51DE
        _InkOpacity ("Ink Opacity", Range(0,1)) = 0.9

        [Header(Shape)]
        _MinLineWidth ("Min Line Width (world)", Float) = 0.08
        _LineWidthFactor ("Line Width x Height", Float) = 0.08
        _TopRadiusMin ("Top Corner Radius Min (world)", Float) = 0.06
        _TopRadiusFactor ("Top Corner Radius x Height", Float) = 0.22
        _BottomRadiusFactor ("Bottom Corner Radius x Height", Float) = 0.5
        _EdgeWobble ("Riso Edge Deviation x Height", Float) = 0.03
        _EdgeWobbleWavelength ("Riso Edge Wavelength (world)", Float) = 0.6

        [Header(Halftone)]
        _DotPitch ("Dot Pitch P (world)", Float) = 0.42
        _DotMinDiameter ("Min Dot Diameter dMin (world)", Float) = 0.14
        _CleanTopMin ("Clean Top Band Min (world)", Float) = 0.18
        _CleanTopFactor ("Clean Top Band x Height", Float) = 0.22
        _ThinHeight ("Thin Platform Height (no dots below)", Float) = 0.45
        _Accelerating ("Accelerating (0/1)", Float) = 0
        _DenseBoost ("Accelerating Density Boost", Float) = 1.18

        [Header(Paper Grain)]
        _GrainAmount ("Dot Diameter Grain", Float) = 0.08
        _GrainScale ("Dot Grain Scale (world)", Float) = 0.25
        _PaperBlotch ("Paper Brightness Variation", Float) = 0.03
        _PaperBlotchScale ("Paper Blotch Scale (world)", Float) = 0.6

        [Header(Misregistration)]
        _PinkShift ("Pink Static Shift (world)", Vector) = (0.04, -0.03, 0, 0)
        _TealShift ("Teal Static Shift (world)", Vector) = (-0.04, 0.03, 0, 0)
        _MisregK ("Velocity Shift K (sec)", Float) = 0.012
        _MisregMax ("Velocity Shift Max (world)", Float) = 0.06
        _Velocity ("Velocity (world units/sec)", Vector) = (0, 0, 0, 0)

        [Header(Stage 2 placeholders, unused)]
        _InfluencePos ("Influence Pos", Vector) = (0, 0, 0, 0)
        _InfluenceRadius ("Influence Radius", Float) = 2.5
        _InfluenceAmount ("Influence Amount", Float) = 0
        _ShiftFront ("Shift Front", Float) = 0
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
            "DisableBatching"="True" // boyut unity_ObjectToWorld'den okunuyor; batch'te matris birim olur
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
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 localPos : TEXCOORD0; // platform yerel uzayı, dünya biriminde (gerilmemiş)
                float2 size     : TEXCOORD1; // platformun dünya boyutu (lossyScale * 1 birimlik sprite)
                float2 upLocal  : TEXCOORD2; // dünya yukarısı, localPos uzayında
                float2 velLocal : TEXCOORD3; // _Velocity, localPos uzayında
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _PaperColor;
            fixed4 _LineColor;
            fixed4 _TealInk;
            fixed4 _YellowInk;
            fixed4 _PinkInk;
            float _InkOpacity;
            float _MinLineWidth;
            float _LineWidthFactor;
            float _TopRadiusMin;
            float _TopRadiusFactor;
            float _BottomRadiusFactor;
            float _EdgeWobble;
            float _EdgeWobbleWavelength;
            float _DotPitch;
            float _DotMinDiameter;
            float _CleanTopMin;
            float _CleanTopFactor;
            float _ThinHeight;
            float _Accelerating;
            float _DenseBoost;
            float _GrainAmount;
            float _GrainScale;
            float _PaperBlotch;
            float _PaperBlotchScale;
            float4 _PinkShift;
            float4 _TealShift;
            float _MisregK;
            float _MisregMax;
            float4 _Velocity;
            // Aşama 2 yer tutucuları: şimdilik kullanılmıyor
            float4 _InfluencePos;
            float _InfluenceRadius;
            float _InfluenceAmount;
            float _ShiftFront;

            static const float PI = 3.14159265;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 c0 = float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20);
                float3 c1 = float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21);
                float2 size = float2(length(c0), length(c1));
                float2 r0 = c0.xy / max(size.x, 1e-5); // yerel x ekseni (dünyada)
                float2 r1 = c1.xy / max(size.y, 1e-5); // yerel y ekseni (dünyada)

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.localPos = IN.vertex.xy * size;
                OUT.size = size;
                // Dünya vektörü -> yerel uzay: dönüşün tersi (iç çarpım)
                OUT.upLocal = float2(r0.y, r1.y);
                OUT.velLocal = float2(dot(_Velocity.xy, r0), dot(_Velocity.xy, r1));
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Pürüzsüz değer gürültüsü, [0,1]
            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Köşe başına yarıçaplı yuvarlatılmış dikdörtgen SDF (merkez 0, yarı boyut b). Negatif = içeride.
            float sdSlab(float2 p, float2 b, float rTop, float rBottom)
            {
                float r = p.y > 0.0 ? rTop : rBottom;
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            // Tek mürekkep katmanının nokta alfası. Kafes `angle` kadar döner, `shift` kadar kayıktır.
            // Çap v'ye (0 üst, 1 alt) bağlıdır: P*(a0 + a1*v). v < vMin -> nokta yok.
            float InkDots(float2 p, float2 up, float support, float stripW, float angle, float2 shift,
                          float a0, float a1, float vMin, float seed)
            {
                float P = max(_DotPitch, 0.05);
                float c = cos(angle);
                float s = sin(angle);
                float2 sp = p - shift;
                float2 q = float2(c * sp.x + s * sp.y, -s * sp.x + c * sp.y);
                float2 cell = q / P;
                float2 id = floor(cell);
                float2 f = frac(cell) - 0.5;
                float dist = length(f) * P;

                // Hücre merkezi yerel uzayda (v ve gren hücre başına sabit -> nokta daire kalır)
                float2 cq = (id + 0.5) * P;
                float2 centerP = float2(c * cq.x - s * cq.y, s * cq.x + c * cq.y) + shift;

                float depthC = support - dot(centerP, up);
                float v = saturate(depthC / max(2.0 * support, 1e-4));
                v *= lerp(1.0, _DenseBoost, _Accelerating);
                if (v < vMin)
                    return 0.0;

                float diameter = P * (a0 + a1 * v);
                diameter *= 1.0 + _GrainAmount * (valueNoise(centerP / max(_GrainScale, 0.01) + seed) * 2.0 - 1.0);

                // dMin: altına inen nokta dMin çapında çizilir, alfa d/dMin (CRT'de titreyen alt-hücre nokta yok)
                float alphaScale = 1.0;
                if (diameter < _DotMinDiameter)
                {
                    alphaScale = diameter / _DotMinDiameter;
                    diameter = _DotMinDiameter;
                }

                float r = 0.5 * diameter;
                // Temiz üst şerit: şeride değen nokta hiç çizilmez (yarım yay kalmaz)
                if (depthC - r < stripW)
                    return 0.0;
                float aa = max(fwidth(dist), 1e-4);
                return (1.0 - smoothstep(r - aa, r + aa, dist)) * alphaScale;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = IN.localPos;
                float2 size = IN.size;
                float2 halfSize = size * 0.5;
                float h = min(size.x, size.y); // kalınlık: uzun dikey duvar platformlarında (1x17) çizgi/yarıçap/şerit yüksekliğe değil kalınlığa bağlı

                float maxR = min(halfSize.x, halfSize.y);
                float rTop = min(max(_TopRadiusMin, _TopRadiusFactor * h), maxR);
                float rBottom = min(_BottomRadiusFactor * h, maxR);

                float d = sdSlab(p, halfSize, rTop, rBottom);

                // Riso kenar sapması: yalnızca yanlarda ve altta, İÇE doğru, üstte 0. Statik.
                float wobbleWeight = smoothstep(0.0, 0.35 * h, halfSize.y - p.y);
                float wobble = valueNoise(p / max(_EdgeWobbleWavelength, 0.01) + 17.0);
                d += _EdgeWobble * h * wobble * wobbleWeight;

                float aa = max(fwidth(d), 1e-4);
                float body = 1.0 - smoothstep(-aa, aa, d);
                if (body <= 0.0)
                    discard;

                // Çizgi: collider sınırına oturur, kaymaz. Çok ince platformda dolguyu yutmasın diye 0.4h ile sınırlı.
                float lineW = min(max(_MinLineWidth, _LineWidthFactor * h), 0.4 * h);
                float fill = smoothstep(-lineW - aa, -lineW + aa, d); // 0: dolgu (içeride), 1: çizgi (kenarda)

                // Kâğıt + çok hafif düşük frekanslı lekelenme (statik)
                float blotch = valueNoise(p / max(_PaperBlotchScale, 0.01) + 91.7) * 2.0 - 1.0;
                float3 rgb = _PaperColor.rgb * (1.0 + _PaperBlotch * blotch);

                // Hıza bağlı kayma: teal hızın tersine 2k|v|, pembe hız yönünde k|v|; eklenen kısım üst sınırlı.
                float2 vel = IN.velLocal;
                float2 pinkShift = _PinkShift.xy + vel * _MisregK;
                float2 tealShift = _TealShift.xy - vel * (2.0 * _MisregK);
                float2 pinkAdd = pinkShift - _PinkShift.xy;
                float2 tealAdd = tealShift - _TealShift.xy;
                float pl = length(pinkAdd);
                float tl = length(tealAdd);
                if (pl > _MisregMax) pinkShift = _PinkShift.xy + pinkAdd * (_MisregMax / pl);
                if (tl > _MisregMax) tealShift = _TealShift.xy + tealAdd * (_MisregMax / tl);

                float2 up = normalize(IN.upLocal + 1e-6);
                float support = halfSize.x * abs(up.x) + halfSize.y * abs(up.y);

                float yellow, pink, teal;
                if (h < _ThinHeight)
                {
                    // İnce platform: nokta yok; iki kayık düz bant (sarı üstte, pembe altta), her biri gövdenin %60'ı
                    float band = 0.6 * h;
                    float ay = max(fwidth(p.y), 1e-4);
                    yellow = smoothstep(halfSize.y - band - ay, halfSize.y - band + ay, p.y);
                    pink = 1.0 - smoothstep(-halfSize.y + band - ay + pinkShift.y,
                                            -halfSize.y + band + ay + pinkShift.y, p.y);
                    teal = 0.0;
                }
                else
                {
                    // Temiz üst şerit (dünya yukarısına göre): nokta yok
                    float stripW = max(_CleanTopMin, _CleanTopFactor * h);

                    // Katman açıları: teal 15°, sarı 45°, pembe 75° (hiçbiri 0°/90° değil)
                    teal   = InkDots(p, up, support, stripW, radians(15.0), tealShift,        0.24, 0.34, 0.35, 3.1);
                    yellow = InkDots(p, up, support, stripW, radians(45.0), float2(0.0, 0.0), 0.50, 0.35, 0.00, 7.7);
                    pink   = InkDots(p, up, support, stripW, radians(75.0), pinkShift,        0.26, 0.40, 0.20, 5.3);
                }

                // Kâğıt üstünde multiply, çizginin içinde kırpılı
                float inkMask = (1.0 - fill) * _InkOpacity;
                rgb *= lerp(1.0, _TealInk.rgb,   teal   * inkMask);
                rgb *= lerp(1.0, _YellowInk.rgb, yellow * inkMask);
                rgb *= lerp(1.0, _PinkInk.rgb,   pink   * inkMask);

                rgb = lerp(rgb, _LineColor.rgb, fill);
                float alpha = body * IN.color.a;
                return fixed4(rgb * alpha, alpha);
            }
        ENDCG
        }
    }
}
