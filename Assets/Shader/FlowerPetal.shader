Shader "Flower/Petal"
{
    // Cel-shaded 3B yaprak: ızgara bir plane (FlowerPetalMesh) vertex shader'da bükülür, şekli ve boyası fragment'ta çizilir.
    //
    // Pivot yaprağın tabanıdır; yaprak +X'e uzanır, bükülme XY düzleminde (dikey düzlem), en yönü Z'dedir.
    // Boyuna eğrilik: k(t) = _Turn * smoothstep(_Reach, _Reach + _Soft, t) + _BaseCurl   (t = 0 taban, 1 uç)
    //   _Reach'in gerisi düz kalır, ilerisi _Turn ile RULO halinde sarılır; _Reach 1'e giderken rulo çözülür.
    // Enine rulo: _Roll radyan, yaprağın eni boyunca sarılır (kağıdı boyuna rulo yapmak).
    // Parametreleri FlowerBloom her yaprak için MaterialPropertyBlock ile yazar.
    //
    // Boya: sabit sahte ışık + bükülmüş normal -> 2 bant (gölge / ana / parlak) + hue-shifted gölge, dış çizgi şeklin
    // kendi mesafesinden (ek geometri yok), tek sert parlama çizgisi, birkaç temiz damar. _Open kapalıyken koyu/soluk,
    // _Warn kapanış uyarısında turuncuya kayıp titreşir (oyuncuya okunurluk).
    Properties
    {
        _HiCol ("Highlight", Color) = (1.0, 0.878, 0.925, 1)
        _TipCol ("Tip", Color) = (1.0, 0.769, 0.855, 1)
        _BaseCol ("Base (ana)", Color) = (1.0, 0.620, 0.769, 1)
        _ShadowCol ("Shadow (magentaya kaymış)", Color) = (0.878, 0.416, 0.651, 1)
        _DeepCol ("Deep / Vein", Color) = (0.761, 0.298, 0.549, 1)
        _OutlineCol ("Outline", Color) = (0.620, 0.184, 0.431, 1)
        _GlowCol ("Base Glow", Color) = (1.0, 0.788, 0.639, 1)
        _WarnCol ("Warn Tint", Color) = (1.0, 0.62, 0.38, 1)

        _Shape ("Shape (en geniş nokta; büyük = uca doğru)", Range(0.6, 3)) = 1.35
        _Roundness ("Roundness (uç yuvarlaklığı; küçük = yuvarlak)", Range(0.3, 1.5)) = 0.62
        _OutlinePx ("Outline (piksel)", Range(0.5, 6)) = 2.2
        _ClosedOutlineMul ("Closed Outline Mul", Range(1, 3)) = 1.5
        _ShadowThr ("Shadow Threshold", Range(0, 1)) = 0.72
        _LightThr ("Light Threshold", Range(0, 1)) = 0.965
        _SpecThr ("Specular Threshold", Range(0.8, 1)) = 0.992
        _VeinCount ("Vein Lines", Float) = 3
        _VeinWidth ("Vein Width", Range(0.005, 0.1)) = 0.028
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

            fixed4 _HiCol, _TipCol, _BaseCol, _ShadowCol, _DeepCol, _OutlineCol, _GlowCol, _WarnCol;
            float4 _LightDir;
            float _Shape, _Roundness, _OutlinePx, _ClosedOutlineMul, _ShadowThr, _LightThr, _SpecThr, _VeinCount, _VeinWidth, _Soft;
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
                float bLight = AAStep(_LightThr, shade, 0.01);

                // Tabandan uca 3 sert basamak
                float s1 = AAStep(0.33, u, 0.0);
                float s2 = AAStep(0.66, u, 0.0);
                float3 grad = lerp(lerp(_BaseCol.rgb, lerp(_BaseCol.rgb, _TipCol.rgb, 0.5), s1), _TipCol.rgb, s2);

                float3 col = lerp(_ShadowCol.rgb, grad, bShadow);
                col = lerp(col, _HiCol.rgb, bLight * 0.6);

                // Taban parıltısı (sahte SSS): sert kenarlı sıcak şeftali
                col = lerp(col, _GlowCol.rgb, (1.0 - AAStep(0.16, u, 0.01)) * 0.9 * bShadow);

                // Temiz damarlar: sabit genişlik, uçta ve tabanda sönen
                float vv = abs(frac(v * _VeinCount * 0.5 + 0.5) - 0.5);
                float vein = (1.0 - AAStep(_VeinWidth, vv, 0.0)) * AAStep(0.14, u, 0.0) * (1.0 - AAStep(0.82, u, 0.0));
                col = lerp(col, _DeepCol.rgb, vein * 0.55);

                // Tek sert parlama çizgisi (sadece bükülmüş yüzeylerde yakalanır) + sert kenar ışığı
                float3 H = normalize(L + viewDir);
                float spec = AAStep(_SpecThr, dot(nv, H), 0.0);
                col = lerp(col, float3(1, 1, 1), spec * (1.0 - vein));
                float rim = AAStep(0.72, 1.0 - ndv, 0.01) * bShadow;
                col = lerp(col, _HiCol.rgb, rim * 0.55);

                // Dış yüz solgun ve düz; iç yüz canlı
                col = inside ? col : lerp(col, _ShadowCol.rgb, 0.5);

                // Dış çizgi: şeklin kendi mesafesinden. Kapalıyken kalınlaşır, üst üste binen yapraklar ayrışsın.
                float ow = _OutlinePx * lerp(_ClosedOutlineMul, 1.0, saturate(_Open));
                float edge = smoothstep(-ow - 0.5, -ow + 0.5, dpx);
                col = lerp(col, _OutlineCol.rgb, edge);

                // Oynanış okunurluğu: kapalı = koyu + soluk, açık = canlı. Uyarı = turuncuya kayış + titreşen parlaklık.
                float lum = dot(col, float3(0.3, 0.6, 0.1));
                float closed = 1.0 - saturate(_Open);
                col = lerp(col, lum.xxx, 0.25 * closed);
                col *= lerp(1.04, 0.85, closed);
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
