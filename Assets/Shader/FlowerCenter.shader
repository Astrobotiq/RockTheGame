Shader "Flower/Center"
{
    // Çiçeğin ortası: pasiflora kulesi. Kameraya bakan bir plane üzerinde düz kobalt disk, ortasında üçgen düzende 3 beyaz nokta
    // (stigma), çevresinde 5 krem organ ucu. Düz renk (kâğıt kesmesi dili): kubbe gölgesi, kontür, tohum noktaları yok; diskin
    // üstüne düşen kesim gölgesini iplik tacı şeritleri verir (Flower/CoronaShadow), diskin kendi gölgesi yaprakların altında kalır.
    // UV merkezi (0.5, 0.5); disk quad'ın içinde (r = 1 quad kenarı). FlowerBloom diski "pop" yayıyla ölçekler.
    // _Reveal < 0.5 iken disk tomurcuk çekirdeği olarak düz _BudCol tonundadır (açılmadı, "tutulamaz" okunur).
    // _Warn kapanış uyarısında diski kobalttan amber'e kaydırır (ikincil ipucu; asıl uyarı titreme ve büzülme).
    Properties
    {
        _BaseCol ("Disk (kobalt)", Color) = (0.082, 0.212, 0.910, 1)
        _DotCol ("Stigma noktaları", Color) = (1.0, 1.0, 1.0, 1)
        _OrganCol ("Organ uçları", Color) = (1.0, 0.953, 0.769, 1)
        _WarnCol ("Warn (amber)", Color) = (1.0, 0.788, 0.302, 1)
        _BudCol ("Bud (kapalıyken çekirdek)", Color) = (0.431, 0.435, 0.471, 1)

        _CoreRadius ("Core Radius (0-1)", Range(0.2, 0.95)) = 0.9
        _TriRing ("Stigma halka yarıçapı (diskin oranı)", Range(0.05, 0.6)) = 0.3
        _TriDot ("Stigma nokta yarıçapı (diskin oranı)", Range(0.05, 0.4)) = 0.2
        _OrganRing ("Organ halka yarıçapı (diskin oranı)", Range(0.3, 0.95)) = 0.7
        _OrganDot ("Organ nokta yarıçapı (diskin oranı)", Range(0.03, 0.25)) = 0.11
        _OrganCount ("Organ sayısı", Float) = 5

        [HideInInspector] _Warn ("Warn (script)", Range(0, 1)) = 0
        [HideInInspector] _Reveal ("Reveal (script; 0 = tomurcuk çekirdeği, 1 = açık merkez)", Range(0, 1.5)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite On
        ZTest LEqual
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            fixed4 _BaseCol, _DotCol, _OrganCol, _WarnCol, _BudCol;
            float _CoreRadius, _TriRing, _TriDot, _OrganRing, _OrganDot, _OrganCount, _Warn, _Reveal;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                return OUT;
            }

            float AAStep(float edge, float x, float soft)
            {
                float w = fwidth(x) + soft + 1e-5;
                return smoothstep(edge - w, edge + w, x);
            }

            // Daire maskesi (1 içeride): merkez c, yarıçap rad.
            float Disc(float2 p, float2 c, float rad)
            {
                return 1.0 - AAStep(rad, length(p - c), 0.0);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = (IN.uv - 0.5) * 2.0;
                float R = _CoreRadius;

                // Pikselde kenara uzaklık (negatif = içeride): disk dışı kesilir.
                float d = length(p) - R;
                float dpx = d / max(fwidth(d), 1e-5);
                clip(0.5 - dpx);

                float3 col = _BaseCol.rgb;

                // Stigma: üçgen düzende 3 beyaz nokta (biri yukarı bakar).
                float tri = 0.0;
                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    float a = TAU * (i / 3.0 + 0.25);
                    tri = max(tri, Disc(p, float2(cos(a), sin(a)) * _TriRing * R, _TriDot * R));
                }
                col = lerp(col, _DotCol.rgb, tri);

                // Organ uçları: halka üzerinde 5 krem nokta (biri yukarı bakar).
                float count = max(_OrganCount, 1.0);
                float a0 = atan2(p.y, p.x) - TAU * 0.25;
                float sector = TAU / count;
                float local = frac(a0 / sector + 0.5) - 0.5;
                float2 g = float2(length(p) - _OrganRing * R, local * sector * _OrganRing * R);
                float organ = 1.0 - AAStep(_OrganDot * R, length(g), 0.0);
                col = lerp(col, _OrganCol.rgb, organ);

                // Uyarı: disk kobalttan amber'e (noktalar aynı kalır).
                float dotsMask = max(tri, organ);
                col = lerp(col, lerp(col, _WarnCol.rgb, 0.85), _Warn * (1.0 - dotsMask));

                // Henüz açılmadıysa: düz kapalı ton (delik kalmasın, "tutulamaz" okunsun).
                float bud = 1.0 - AAStep(0.5, _Reveal, 0.0);
                col = lerp(col, _BudCol.rgb, bud);

                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
}
