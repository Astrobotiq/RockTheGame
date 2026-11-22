Shader "Flower/Center"
{
    // Çiçeğin ortası: pasiflora kulesi. Kameraya bakan bir plane üzerinde düz koyu disk (kule), ortasında üçgen düzende 3 beyaz
    // stigma noktası, diskin dışında yaprak aralarına bakan 5 krem organ ucu. Düz renk (kâğıt kesmesi dili): kubbe gölgesi,
    // kontür, tohum noktaları yok; diskin üstüne düşen kesim gölgesini iplik tacı şeritleri verir (Flower/CoronaShadow).
    // UV merkezi (0.5, 0.5); quad yarıçapı 1. Disk _CoreRadius kadar, organlar disk dışında _OrganRing'de durur (quad'a sığmalı:
    // _OrganRing * R + _OrganDot * R <= 1). FlowerBloom quad'ı "pop" yayıyla ölçekler.
    // _Reveal < 0.5 iken (açılmadı) yalnızca düz _BudCol diski görünür: "tutulamaz" okunur, organlar ve noktalar saklanır.
    // _TowerAngle (derece, script): çiçeğin yön dönüşünü iptal eder, stigma üçgeninin tepesi hep yukarı bakar.
    Properties
    {
        _BaseCol ("Kule diski", Color) = (0.188, 0.216, 0.471, 1)
        _DotCol ("Stigma noktaları", Color) = (1.0, 1.0, 1.0, 1)
        _OrganCol ("Organ uçları", Color) = (1.0, 0.953, 0.769, 1)
        _BudCol ("Bud (kapalıyken çekirdek)", Color) = (0.431, 0.435, 0.471, 1)

        _CoreRadius ("Disk yarıçapı (quad oranı)", Range(0.2, 0.95)) = 0.5
        _TriRing ("Stigma halka yarıçapı (diskin oranı)", Range(0.05, 0.9)) = 0.53
        _TriDot ("Stigma nokta yarıçapı (diskin oranı)", Range(0.05, 0.6)) = 0.42
        _OrganRing ("Organ halka yarıçapı (diskin oranı)", Range(0.3, 1.8)) = 1.44
        _OrganDot ("Organ nokta yarıçapı (diskin oranı)", Range(0.03, 0.6)) = 0.34
        _OrganCount ("Organ sayısı", Float) = 5

        [HideInInspector] _TowerAngle ("Tower Angle deg (script)", Float) = 0
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

            fixed4 _BaseCol, _DotCol, _OrganCol, _BudCol;
            float _CoreRadius, _TriRing, _TriDot, _OrganRing, _OrganDot, _OrganCount, _TowerAngle, _Reveal;

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

                // Çiçeğin yön dönüşünü iptal et: tepe hep yukarı.
                float sa, ca;
                sincos(radians(_TowerAngle), sa, ca);
                float2 q = float2(p.x * ca - p.y * sa, p.x * sa + p.y * ca);

                float disc = Disc(p, float2(0, 0), R);

                // Stigma: üçgen düzende 3 beyaz nokta (biri yukarı bakar).
                float tri = 0.0;
                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    float a = TAU * (i / 3.0 + 0.25);
                    tri = max(tri, Disc(q, float2(cos(a), sin(a)) * _TriRing * R, _TriDot * R));
                }

                // Organ uçları: diskin dışında halka üzerinde 5 krem nokta; yaprakların arasına (tepeden yarım adım kayık) bakar.
                float count = max(_OrganCount, 1.0);
                float sector = TAU / count;
                float a0 = atan2(q.y, q.x) - TAU * 0.25 - sector * 0.5;
                float local = frac(a0 / sector + 0.5) - 0.5;
                float2 g = float2(length(q) - _OrganRing * R, local * sector * _OrganRing * R);
                float organ = 1.0 - AAStep(_OrganDot * R, length(g), 0.0);

                // Açılmadıysa organlar ve noktalar saklanır; yalnızca düz tomurcuk diski kalır.
                float bud = 1.0 - AAStep(0.5, _Reveal, 0.0);
                organ *= 1.0 - bud;
                tri *= 1.0 - bud;

                clip(max(disc, organ) - 0.5);

                float3 col = _BaseCol.rgb;
                col = lerp(col, _DotCol.rgb, tri * disc);
                col = lerp(col, _OrganCol.rgb, organ);
                col = lerp(col, _BudCol.rgb, bud);
                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
}
