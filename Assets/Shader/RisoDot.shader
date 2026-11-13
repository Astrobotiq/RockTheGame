// Riso levha iz parçacığı (docs/ui-research/halftone/implementation-plan.md H6).
// Analitik SDF daire, opak NORMAL karışım (multiply değil: gökyüzünü zeytine çevirir).
// Pembe ve teal parçacıkların yarısı, öbür mürekkepten çapın %22'si kayık bir "hayalet" daire taşır;
// kesişim rengi shader'da renkA * renkB. Mürekkep, parçacık rengi ile property'lerdeki renklerin eşleşmesinden bulunur.
// Vertex stream'ler (HalftoneTrailEmitter ayarlar): Position, Color, UV, StableRandom.x
// Quad boyutu = startSize * 1.25; çizilen daire yarıçapı quad yarısının 0.8'i (hayalet kayması için pay).
Shader "Particles/Custom/RisoDot"
{
    Properties
    {
        _TealInk ("Teal Ink", Color) = (0.157, 0.604, 0.643, 1)
        _YellowInk ("Yellow Ink", Color) = (0.988, 0.682, 0.016, 1)
        _PinkInk ("Pink Ink", Color) = (1, 0.318, 0.871, 1)
        _GhostChance ("Ghost Chance (pink/teal)", Range(0,1)) = 0.5
        _GhostOffset ("Ghost Offset x Diameter", Float) = 0.22
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                fixed4 color  : COLOR;
                float3 uv     : TEXCOORD0; // xy: UV, z: StableRandom.x (stream'ler tek texcoord'a paketlenir)
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
                float  rnd    : TEXCOORD1;
            };

            fixed4 _TealInk;
            fixed4 _YellowInk;
            fixed4 _PinkInk;
            float _GhostChance;
            float _GhostOffset;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color;
                OUT.uv = IN.uv.xy;
                OUT.rnd = IN.uv.z;
                return OUT;
            }

            float Circle(float2 p, float2 center, float radius)
            {
                float d = length(p - center) - radius;
                float aa = max(fwidth(d), 1e-4);
                return 1.0 - smoothstep(-aa, aa, d);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 p = (IN.uv - 0.5) * 2.0; // quad yarısı = 1
                const float R = 0.8;

                float dTeal = distance(IN.color.rgb, _TealInk.rgb);
                float dYellow = distance(IN.color.rgb, _YellowInk.rgb);
                float dPink = distance(IN.color.rgb, _PinkInk.rgb);
                bool isTeal = dTeal < dYellow && dTeal < dPink;
                bool isPink = dPink < dYellow && dPink < dTeal;

                float main = Circle(p, 0, R);
                float3 rgb = IN.color.rgb;
                float cover = main;

                bool hasGhost = (isTeal || isPink) && frac(IN.rnd * 7.31) < _GhostChance;
                if (hasGhost)
                {
                    // Hayalet: teal -> pembe, pembe -> sarı; yön parçacık başına sabit
                    float3 other = isTeal ? _PinkInk.rgb : _YellowInk.rgb;
                    float ang = frac(IN.rnd * 13.7) * 6.2831853;
                    float2 off = float2(cos(ang), sin(ang)) * (_GhostOffset * R);
                    float ghost = Circle(p, off, R);
                    float both = main * ghost;
                    // ana yalnız / hayalet yalnız / kesişim (renkA * renkB)
                    rgb = lerp(other, rgb, main);
                    rgb = lerp(rgb, IN.color.rgb * other, both);
                    cover = max(main, ghost);
                }

                float a = cover * IN.color.a;
                if (a <= 0.003) discard;
                return fixed4(rgb, a);
            }
        ENDCG
        }
    }
}
