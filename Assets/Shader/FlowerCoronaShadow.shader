Shader "Flower/CoronaShadow"
{
    // İplik tacı şeritlerinin kesim gölgesi: Flower/PetalShadow ile aynı hesap (FlowerShadow.cginc), şerit şekliyle. Şeritlerin
    // ikinci materyali olarak aynı mesh'e çizilir; kuyruk AlphaTest+1, ZWrite kapalı.
    Properties
    {
        _CutShadowCol ("Kesim gölgesi", Color) = (0.188, 0.216, 0.471, 1)
        _CutShadowPx ("Ofset (piksel)", Range(0, 12)) = 3
        _CutShadowWarnMul ("Uyarıda ek ofset (çarpan)", Range(0, 3)) = 1
        _CutShadowDepth ("Derinlik itmesi (dünya birimi)", Range(0, 0.5)) = 0.02
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
        [HideInInspector] _Warn ("Warn (script)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest+1"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest LEqual
        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "FlowerCorona.cginc"
            #include "FlowerShadow.cginc"

            struct appdata_t
            {
                float4 vertex    : POSITION;
                float2 texcoord  : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                float3 pos, nObj;
                FlowerCoronaVertex(IN.texcoord, IN.texcoord1, pos, nObj);

                v2f OUT;
                OUT.vertex = FlowerShadowClip(pos, nObj);
                OUT.uv = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                clip(0.5 - FlowerCoronaEdgeDpx(IN.uv));
                return fixed4(_CutShadowCol.rgb, 1.0);
            }
            ENDCG
        }
    }
}
