Shader "Flower/Petal"
{
    // Kâğıt yaprak: ızgara bir plane (FlowerPetalMesh) vertex shader'da bükülür, şekli ve boyası fragment'ta çizilir.
    //
    // Pivot yaprağın tabanıdır; yaprak +X'e uzanır, bükülme XY düzleminde (dikey düzlem), en yönü Z'dedir.
    // Boyuna eğrilik: k(t) = _Turn * smoothstep(_Reach, _Reach + _Soft, t) + _BaseCurl   (t = 0 taban, 1 uç)
    //   _Reach'in gerisi düz kalır, ilerisi _Turn ile RULO halinde sarılır; _Reach 1'e giderken rulo çözülür.
    // Enine rulo: _Roll radyan, yaprağın eni boyunca sarılır (kağıdı boyuna rulo yapmak).
    // Parametreleri FlowerBloom her yaprak için MaterialPropertyBlock ile yazar.
    //
    // Boya (el işi kâğıt): düz renk alanlar. Bükülmüş normalden 2 bant (ışık / gölge), orta çizgiden katlanmış iki yüz
    // (bir yüz bir ton gölgede), ince katlama izi ve ışık yakalayan ince kâğıt kenarı. Kontür, damar, parlama çizgisi,
    // gradyan yok; yaprağın okunması kesim gölgesinden gelecek (ikinci pass). Bükme/rulo vertex'tedir ve boyadan bağımsızdır.
    // _Open kapalıyken yaprak düz _ClosedCol tonuna döner (kâğıt "soldu"); _Warn kapanış uyarısında titreşen sıcak tint verir.
    Properties
    {
        _HiCol ("Kâğıt kenarı (ışık)", Color) = (1.0, 1.0, 1.0, 1)
        _BaseCol ("Base (ışık yüzü)", Color) = (0.957, 0.945, 0.918, 1)
        _ShadowCol ("Shadow (gölge yüzü)", Color) = (0.804, 0.824, 0.894, 1)
        _DeepCol ("Katlama izi", Color) = (0.353, 0.376, 0.533, 1)
        _ClosedCol ("Kapalı hal tonu", Color) = (0.431, 0.435, 0.471, 1)
        _WarnCol ("Warn Tint", Color) = (1.0, 0.788, 0.302, 1)

        _Shape ("Shape (en geniş nokta; büyük = uca doğru)", Range(0.6, 3)) = 1.35
        _Roundness ("Roundness (uç yuvarlaklığı; küçük = yuvarlak)", Range(0.3, 1.5)) = 0.62
        _ShadowThr ("Shadow Threshold", Range(0, 1)) = 0.72
        _FoldShade ("Fold Shade (katlanan yüzün gölge payı)", Range(0, 1)) = 0.45
        _CreasePx ("Crease (katlama izi, piksel)", Range(0, 4)) = 1.2
        _EdgePx ("Paper Edge (kenar kalınlığı, piksel)", Range(0, 4)) = 1.5
        _LightDir ("Light Direction (world)", Vector) = (-0.35, 0.55, -0.75, 0)
        _Soft ("Reach Softness", Range(0.05, 0.6)) = 0.25

        [HideInInspector] _Length ("Length (script)", Float) = 1
        [HideInInspector] _Width ("Width (script)", Float) = 0.55
        [HideInInspector] _Pitch ("Pitch rad (script)", Float) = 0.25
        [HideInInspector] _Reach ("Reach (script)", Float) = 1
        [HideInInspector] _Turn ("Turn rad per length (script)", Float) = 7
        [HideInInspector] _BaseCurl ("Base Curl rad (script)", Float) = 0.6
        [HideInInspector] _Roll ("Roll rad (script)", Float) = 0.6
        [HideInInspector] _Open ("Open (script)", Range(0, 1)) = 1
        [HideInInspector] _Warn ("Warn (script)", Range(0, 1)) = 0
        [HideInInspector] _Seed ("Seed (script)", Range(0, 1)) = 0.5
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

        // Yapraklar birbirini ve ortayı örter: derinlik yazılır, kenar clip ile kesilir (sıralama gerekmez).
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

            #define PI 3.14159265359
            #define STEPS 14

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float3 nW     : TEXCOORD1;
                float3 posW   : TEXCOORD2;
            };

            fixed4 _HiCol, _BaseCol, _ShadowCol, _DeepCol, _ClosedCol, _WarnCol;
            float4 _LightDir;
            float _Shape, _Roundness, _ShadowThr, _FoldShade, _CreasePx, _EdgePx, _Soft;
            float _Length, _Width, _Pitch, _Reach, _Turn, _BaseCurl, _Roll, _Open, _Warn, _Seed;

            v2f vert(appdata_t IN)
            {
                float u = IN.texcoord.x;
                float v = IN.texcoord.y * 2.0 - 1.0;

                // Merkez çizgisini u'ya kadar sayısal entegre et (kıvrım profili parçalı, kapalı form yok).
                float a = _Pitch;
                float2 P = float2(0, 0);
                float h = u / STEPS;
                [unroll]
                for (int i = 0; i < STEPS; i++)
                {
                    float t = (i + 0.5) * h;
                    float k = _Turn * smoothstep(_Reach, _Reach + _Soft, t) + _BaseCurl;
                    float am = a + 0.5 * k * h;
                    P += _Length * h * float2(cos(am), sin(am));
                    a += k * h;
                }
                float2 N = float2(-sin(a), cos(a));   // bükülme düzleminde iç (içbükey) yüz yönü

                // Enine rulo: en boyunca psi radyan sarılır, kenarlar içbükey yüze doğru kalkar.
                float psi = max(_Roll, 1e-3);
                float R = _Width / psi;
                float phi = psi * v * 0.5;
                float across = R * sin(phi);
                float lift = R * (1.0 - cos(phi));

                float3 pos = float3(P + N * lift, across);
                float3 nObj = float3(N * cos(phi), -sin(phi));

                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(float4(pos, 1.0));
                OUT.uv = IN.texcoord;
                OUT.nW = UnityObjectToWorldNormal(nObj);
                OUT.posW = mul(unity_ObjectToWorld, float4(pos, 1.0)).xyz;
                return OUT;
            }

            // fwidth'li eşik: kenarlar net ama titreşmez. soft > 0 bilinçli yumuşatma ekler.
            float AAStep(float edge, float x, float soft)
            {
                float w = fwidth(x) + soft + 1e-5;
                return smoothstep(edge - w, edge + w, x);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float u = saturate(IN.uv.x);
                float v = IN.uv.y * 2.0 - 1.0;

                // Damla profili: tabanda dar, ~%60'ta en geniş, uçta yuvarlak bir sivrilik.
                // d < 0 içeride, d = 0 kenarda; dpx = pikselde kenara uzaklık (negatif = içeride).
                float w = pow(saturate(sin(PI * pow(u, _Shape))), _Roundness);
                float d = abs(v) - w;
                float dpx = d / max(fwidth(d), 1e-5);
                clip(0.5 - dpx);

                // Hangi yüz kameraya bakıyor? Ortografik kamerada bakış yönü sabit.
                float3 viewDir = lerp(normalize(_WorldSpaceCameraPos - IN.posW), UNITY_MATRIX_V[2].xyz, unity_OrthoParams.w);
                float3 n = normalize(IN.nW);
                bool inside = dot(n, viewDir) > 0.0;
                float3 nv = inside ? n : -n;
                float3 L = normalize(_LightDir.xyz);

                // Sabit ışık + bakış yönü; küçük bir uç-tabana bağlı terim, düz yatan yaprak da okunsun.
                float ndl = dot(nv, L) * 0.5 + 0.5;
                float ndv = saturate(dot(nv, viewDir));
                float shade = ndl * 0.7 + ndv * 0.3 + (u - 0.5) * 0.15 - (1.0 - _Open) * 0.15;
                float bShadow = AAStep(_ShadowThr, shade, 0.01);

                // Orta çizgiden katlanmış iki yüz: v < 0 yüzü bir ton gölgede.
                float faceB = 1.0 - AAStep(0.0, v, 0.0);
                float foldDim = faceB * _FoldShade;

                // Düz iki ton (ışık / gölge); katlanan yüz gölge tonuna doğru kayar.
                float3 col = lerp(_ShadowCol.rgb, _BaseCol.rgb, bShadow);
                col = lerp(col, _ShadowCol.rgb, foldDim);

                // Katlama izi: orta çizgide ince koyu çizgi, tabanda ve uçta sönen.
                float cw = max(_CreasePx, 0.0) * fwidth(v);
                float crease = (1.0 - smoothstep(cw * 0.5, cw * 0.5 + fwidth(v) + 1e-5, abs(v)))
                             * AAStep(0.08, u, 0.0) * (1.0 - AAStep(0.9, u, 0.0));
                col = lerp(col, _DeepCol.rgb, crease * 0.4);

                // Kâğıt kalınlığı: ışık gören yüzde kenarda ince açık çizgi.
                float edgeBand = smoothstep(-_EdgePx - 0.5, -_EdgePx + 0.5, dpx);
                col = lerp(col, _HiCol.rgb, edgeBand * bShadow * 0.85);

                // Kapalı hal: yaprak düz kapalı tona döner (aynı iki bant + katlanan yüz, daha koyu).
                float closed = 1.0 - smoothstep(0.0, 0.45, _Open);
                float3 closedCol = lerp(_ClosedCol.rgb * 0.8, _ClosedCol.rgb, bShadow);
                closedCol = lerp(closedCol, closedCol * 0.85, foldDim);
                col = lerp(col, closedCol, closed);

                // Uyarı = sıcak tint + titreşen parlaklık (asıl uyarı titreme/kayma; renk ikincil).
                float pulse = 0.5 + 0.5 * sin(_Time.y * (8.0 + 12.0 * _Warn) * 6.2831853);
                col = lerp(col, col * _WarnCol.rgb * 1.15, _Warn * 0.55);
                col *= 1.0 + _Warn * pulse * 0.14;

                // Yaprak başına çok küçük ton sapması
                col *= 1.0 + (_Seed - 0.5) * 0.06;

                return fixed4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
}
