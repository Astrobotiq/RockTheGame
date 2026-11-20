Shader "Flower/Center"
{
    // Çiçeğin ortası (cel): kameraya bakan bir plane üzerinde kubbe gibi shade edilmiş sarı disk, koyu dış çizgi ve tohum noktaları.
    // UV merkezi (0.5, 0.5); disk quad'ın içinde (r = 1 quad kenarı). FlowerBloom diski "pop" yayıyla ölçekler.
    // _Warn kapanış uyarısında diski sıcak turuncuya kaydırır.
    Properties
    {
        _BaseCol ("Base (sarı)", Color) = (1.0, 0.851, 0.239, 1)
        _HiCol ("Highlight", Color) = (1.0, 0.945, 0.651, 1)
        _ShadowCol ("Shadow", Color) = (0.949, 0.631, 0.165, 1)
        _DotCol ("Seed Dot", Color) = (0.722, 0.396, 0.106, 1)
        _DotDeepCol ("Seed Dot Deep", Color) = (0.431, 0.227, 0.071, 1)
        _OutlineCol ("Outline", Color) = (0.478, 0.243, 0.071, 1)
        _WarnCol ("Warn Tint", Color) = (1.0, 0.55, 0.3, 1)
        _BudCol ("Bud (kapalıyken çekirdek)", Color) = (0.878, 0.416, 0.651, 1)
        _BudOutlineCol ("Bud Outline", Color) = (0.620, 0.184, 0.431, 1)

        _CoreRadius ("Core Radius (0-1)", Range(0.2, 0.95)) = 0.9
        _OutlinePx ("Outline (piksel)", Range(0.5, 6)) = 2.4
        _DotRing ("Dot Ring Radius (0-1)", Range(0.1, 0.9)) = 0.58
        _DotCount ("Dot Count", Float) = 9
        _DotRadius ("Dot Radius", Range(0.01, 0.2)) = 0.075
        _LightDir ("Light (kubbe uzayı)", Vector) = (-0.45, 0.5, 0.75, 0)

        [HideInInspector] _Warn ("Warn (script)", Range(0, 1)) = 0
        [HideInInspector] _Reveal ("Reveal (script; 0 = tomurcuk çekirdeği, 1 = sarı merkez)", Range(0, 1.5)) = 1
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

            fixed4 _BaseCol, _HiCol, _ShadowCol, _DotCol, _DotDeepCol, _OutlineCol, _WarnCol, _BudCol, _BudOutlineCol;
            float4 _LightDir;
            float _CoreRadius, _OutlinePx, _DotRing, _DotCount, _DotRadius, _Warn, _Reveal;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = (IN.uv - 0.5) * 2.0;
                float r = length(p);

                // Pikselde kenara uzaklık (negatif = içeride)
                float d = r - _CoreRadius;
                float dpx = d / max(fwidth(d), 1e-5);
                clip(0.5 - dpx);

                // Sahte kubbe normali: kenarda yatar, ortada kameraya bakar
                float2 q = p / _CoreRadius;
                float h = sqrt(saturate(1.0 - dot(q, q)));
                float3 n = float3(q, h);
                float shade = dot(n, normalize(_LightDir.xyz));

                float3 col = lerp(_ShadowCol.rgb, _BaseCol.rgb, AAStep(0.3, shade, 0.01));
                col = lerp(col, _HiCol.rgb, AAStep(0.78, shade, 0.01));

                // Tohum noktaları: halka + ortada büyük bir nokta
                float a = atan2(p.y, p.x);
                float sector = TAU / max(_DotCount, 1.0);
                float local = frac(a / sector + 0.5) - 0.5;
                float2 g = float2(r - _DotRing, local * sector * _DotRing);
                float dotMask = 1.0 - AAStep(_DotRadius, length(g), 0.0);
                float dotCore = 1.0 - AAStep(_DotRadius * 0.45, length(g), 0.0);
                col = lerp(col, _DotCol.rgb, dotMask);
                col = lerp(col, _DotDeepCol.rgb, dotCore);
                float eye = 1.0 - AAStep(_DotRadius * 1.5, r, 0.0);
                col = lerp(col, _DotDeepCol.rgb, eye);

                float edge = smoothstep(-_OutlinePx - 0.5, -_OutlinePx + 0.5, dpx);
                col = lerp(col, _OutlineCol.rgb, edge);

                // Henüz açılmadıysa: yapraklarla aynı dilde düz, koyu pembe tomurcuk çekirdeği (delik kalmasın, "tutulamaz" okunsun)
                float bud = 1.0 - AAStep(0.5, _Reveal, 0.0);
                float3 budCol = lerp(_BudCol.rgb, _BudOutlineCol.rgb, edge);
                col = lerp(col, budCol, bud);

                col = lerp(col, col * _WarnCol.rgb * 1.2, _Warn * 0.5);
                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
}
