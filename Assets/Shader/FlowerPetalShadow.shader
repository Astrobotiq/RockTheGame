Shader "Flower/PetalShadow"
{
    // Kesim gölgesi: yaprağın ekran uzayında sağ-alta ofsetlenmiş koyu kopyası (kâğıt kesmesinin altına düşen gölge).
    // Yaprağın ikinci materyali olarak aynı mesh'e çizilir (FlowerBloom.petalShadowMaterial). Bükme ve şekil FlowerBend.cginc'den,
    // yani yaprakla birebir aynı yüzey; bükme parametrelerini yaprakla aynı MaterialPropertyBlock verir.
    //
    // Kuyruk AlphaTest+1: bütün yapraklar çizildikten sonra gelir. ZWrite kapalı, ZTest LEqual; kopya biraz geriye itilir
    // (_CutShadowDepth), böylece kendi yaprağının üstüne çıkmaz ama altındaki başka yaprağın ve gökyüzünün üstüne düşer.
    // Ofset piksel cinsindendir ve ekran çözünürlüğünden bağımsız aynı kalınlıkta görünür; _Warn uyarıda ofseti büyütür
    // (dolgu ile gölgenin birbirinden "kayması", renge güvenmeyen uyarı ipucu).
    Properties
    {
        _CutShadowCol ("Kesim gölgesi", Color) = (0.188, 0.216, 0.471, 1)
        _CutShadowPx ("Ofset (piksel)", Range(0, 12)) = 3
        _CutShadowWarnMul ("Uyarıda ek ofset (çarpan)", Range(0, 3)) = 1
        _CutShadowDepth ("Derinlik itmesi (dünya birimi)", Range(0, 0.5)) = 0.02

        _Shape ("Shape", Range(0.6, 3)) = 1.35
        _Roundness ("Roundness", Range(0.3, 1.5)) = 0.62
        _Soft ("Reach Softness", Range(0.05, 0.6)) = 0.25

        [HideInInspector] _TipSkew ("Tip Skew (script)", Float) = 0
        [HideInInspector] _Length ("Length (script)", Float) = 1
        [HideInInspector] _Width ("Width (script)", Float) = 0.55
        [HideInInspector] _Pitch ("Pitch rad (script)", Float) = 0.25
        [HideInInspector] _Reach ("Reach (script)", Float) = 1
        [HideInInspector] _Turn ("Turn rad per length (script)", Float) = 7
        [HideInInspector] _BaseCurl ("Base Curl rad (script)", Float) = 0.6
        [HideInInspector] _Roll ("Roll rad (script)", Float) = 0.6
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
            #include "FlowerBend.cginc"

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

            fixed4 _CutShadowCol;
            float _CutShadowPx, _CutShadowWarnMul, _CutShadowDepth, _Warn;

            v2f vert(appdata_t IN)
            {
                float3 pos, nObj;
                FlowerBend(IN.texcoord, pos, nObj);

                float4 posV = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(pos, 1.0)));

                // Bir piksel kaç dünya birimi? Ortografikte sabit, perspektifte derinliğe göre. Ofset görünüm uzayında
                // (sağ, aşağı) verilir; projeksiyon çevirmesi (render texture) bunu etkilemez.
                float worldPerPx = unity_OrthoParams.w > 0.5
                    ? 2.0 * unity_OrthoParams.y / _ScreenParams.y
                    : 2.0 * abs(posV.z) / (UNITY_MATRIX_P._m11 * _ScreenParams.y);
                float px = _CutShadowPx * (1.0 + _Warn * _CutShadowWarnMul);
                float2 off = float2(1.0, -1.0) * px * worldPerPx;
                posV.xy += off;

                // Kameraya doğru dikilen (eğimli) yaprakta ofsetli kopya, kendi yaprağının önüne geçerdi: yüzey ofset kadar
                // yan kayınca derinliği de eğim kadar değişir. Düzlem yaklaşımıyla bu fark kadar (eğim * ofset) geriye itilir;
                // düz yapraklarda itme ~0'dır, böylece gölge altındaki yaprağın üstüne düşebilir. _CutShadowDepth küçük sabit paydır.
                float3 nV = mul((float3x3)UNITY_MATRIX_V, UnityObjectToWorldNormal(nObj));
                float tilt = abs(dot(nV.xy, off)) / max(abs(nV.z), 0.1);
                posV.z -= _CutShadowDepth + tilt;   // kameradan uzağa

                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_P, posV);
                OUT.uv = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                clip(0.5 - FlowerEdgeDpx(IN.uv));
                return fixed4(_CutShadowCol.rgb, 1.0);
            }
            ENDCG
        }
    }
}
