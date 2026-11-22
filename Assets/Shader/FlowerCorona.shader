Shader "Flower/Corona"
{
    // İplik tacı: pasiflora çiçeğinin ortasındaki kalın, yuvarlak uçlu şeritler (kâğıt çiçek). Bir halkanın bütün şeritleri tek
    // mesh'tir (FlowerCoronaMesh); bükme/rulo yaprakla aynıdır (FlowerBend.cginc), şerit yerleşimi FlowerCorona.cginc'de.
    // Boya düz: boyuna üç bant (iç renk / açık boşluk / uç rengi), bullseye halkaları gibi dışa doğru okunur. Gölgesi ikinci
    // materyal olarak çizilir (Flower/CoronaShadow). Bükme ve renkleri FlowerBloom MaterialPropertyBlock ile yazar.
    Properties
    {
        _InnerCol ("İç bant", Color) = (0.082, 0.212, 0.910, 1)
        _GapCol ("Boşluk bandı", Color) = (0.898, 0.914, 1.0, 1)
        _TipCol ("Uç bandı", Color) = (0.188, 0.216, 0.471, 1)
        _Band1 ("İç bant sonu (0-1)", Range(0.05, 0.95)) = 0.5
        _Band2 ("Boşluk bandı sonu (0-1)", Range(0.05, 0.98)) = 0.68
        _BaseRadius ("Şeritlerin eksenden uzaklığı", Float) = 0.2
        _BaseHeight ("Şeritlerin yaprakların üstündeki yüksekliği", Float) = 0.3

        [HideInInspector] _Shape ("Shape (kullanılmaz)", Float) = 1
        [HideInInspector] _Roundness ("Roundness (kullanılmaz)", Float) = 1
        [HideInInspector] _Soft ("Reach Softness", Range(0.05, 0.6)) = 0.25
        [HideInInspector] _TipSkew ("Tip Skew (kullanılmaz)", Float) = 0
        [HideInInspector] _Length ("Length (script)", Float) = 0.6
        [HideInInspector] _Width ("Width (script)", Float) = 0.14
        [HideInInspector] _Pitch ("Pitch rad (script)", Float) = 0.25
        [HideInInspector] _Reach ("Reach (script)", Float) = 1
        [HideInInspector] _Turn ("Turn rad per length (script)", Float) = 0
        [HideInInspector] _BaseCurl ("Base Curl rad (script)", Float) = 0.4
        [HideInInspector] _Roll ("Roll rad (script)", Float) = 0.1
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

        // Yapraklarla aynı kural: derinlik yazılır, kenar clip ile kesilir.
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
            #include "FlowerCorona.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            fixed4 _InnerCol, _GapCol, _TipCol;
            float _Band1, _Band2;

            v2f vert(appdata_t IN)
            {
                float3 pos, nObj;
                FlowerCoronaVertex(IN.texcoord, IN.texcoord1, pos, nObj);

                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(float4(pos, 1.0));
                OUT.uv = IN.texcoord;
                return OUT;
            }

            float AAStep(float edge, float x)
            {
                float w = fwidth(x) + 1e-5;
                return smoothstep(edge - w, edge + w, x);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                clip(0.5 - FlowerCoronaEdgeDpx(IN.uv));

                float u = saturate(IN.uv.x);
                float3 col = lerp(_InnerCol.rgb, _GapCol.rgb, AAStep(_Band1, u));
                col = lerp(col, _TipCol.rgb, AAStep(_Band2, u));
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
