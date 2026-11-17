Shader "UI/SpiralNode"
{
    // Spiral node görseli (docs/spiral-node-tasarimi.md §9). Dönüş açısı script'ten gelir (_Angle):
    // hit-stop ve pause sırasında script ilerletmez, görsel de durur. Pürüzsüz baskı / halftone, piksel art değil.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _RampTex ("Ramp Texture (1D)", 2D) = "white" {}
        _RampU ("Disk Color (ramp U)", Range(0, 1)) = 0.15
        _InkColor ("Ink Color", Color) = (0.1, 0.08, 0.12, 1)
        _AccentColor ("Accent Color (chevrons)", Color) = (1, 0.85, 0.3, 1)

        _Angle ("Angle (radians, script)", Float) = 0
        _Direction ("Direction (+1 CCW, -1 CW)", Float) = 1
        _ArmCount ("Arm Count", Float) = 3
        _Twist ("Twist", Float) = 2.2
        _ArmWidth ("Arm Width (0-1)", Range(0.05, 0.95)) = 0.45

        _DotDensity ("Halftone Dot Density", Float) = 18
        _InnerRadius ("Inner Radius (0-0.5)", Range(0, 0.5)) = 0.08
        _RingInner ("Chevron Ring Inner (0-0.5)", Range(0, 0.5)) = 0.4
        _RingOuter ("Chevron Ring Outer (0-0.5)", Range(0, 0.5)) = 0.49
        _ChevronCount ("Chevron Count", Float) = 10
        _ChevronSlant ("Chevron Slant", Float) = 6

        _Progress ("Reverse Progress (0 = hidden)", Range(0, 1)) = 0
        _Reversing ("Reversing (0/1)", Float) = 0
        _Glow ("Glow (-1 dim .. 1 bright)", Range(-1, 1)) = 0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

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
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _RampTex;
            fixed4 _InkColor;
            fixed4 _AccentColor;
            float _RampU;
            float _Angle, _Direction, _ArmCount, _Twist, _ArmWidth;
            float _DotDensity, _InnerRadius, _RingInner, _RingOuter, _ChevronCount, _ChevronSlant;
            float _Progress, _Reversing, _Glow;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            // Anti-aliased eşik: kenar genişliği ekran türevinden, baskı gibi pürüzsüz.
            float aastep(float threshold, float value)
            {
                float w = max(fwidth(value), 1e-4) * 0.75;
                return smoothstep(threshold - w, threshold + w, value);
            }

            // 45° döndürülmüş halftone ızgara: tone 0..1 → nokta yarıçapı.
            float halftone(float2 uv, float tone)
            {
                float2 r = float2(uv.x + uv.y, uv.y - uv.x) * 0.70710678 * _DotDensity;
                float2 cell = frac(r) - 0.5;
                float d = length(cell);
                float radius = sqrt(saturate(tone)) * 0.62;
                return 1.0 - aastep(radius, d);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord - 0.5;
                float dist = length(uv);
                float edge = 1.0 - aastep(0.5, dist);
                if (edge <= 0.0) return fixed4(0, 0, 0, 0);

                float polar = atan2(uv.y, uv.x);              // CCW pozitif
                float a = (polar - _Angle) / TAU;              // desen _Angle kadar döner
                float rn = saturate(dist / max(_RingInner, 1e-3));

                // --- Spiral kollar (iç disk) ---
                // Kollar dönüş yönünün tersine kıvrılır. Her kolun ön kenarı dolu mürekkep, arkası halftone
                // noktalarla söner: dönerken kuyruğu arkada bırakır, durağan karede de yön okunur.
                float arm = frac(a * _ArmCount - _Direction * _Twist * rn);
                float lead = _Direction > 0 ? arm : 1.0 - arm;   // ön kenarda 1, arkada 0
                float tone = lead * lead;
                float solid = aastep(1.0 - _ArmWidth * 0.25, lead);
                float dots = max(halftone(IN.texcoord, tone * (1.0 - rn * 0.3)), solid);

                fixed3 paper = tex2D(_RampTex, float2(_RampU, 0.5)).rgb;
                fixed4 disk = fixed4(lerp(paper, _InkColor.rgb, dots), 1.0);
                float diskMask = (1.0 - aastep(_RingInner, dist)) * aastep(_InnerRadius, dist);
                // merkez göbek: düz mürekkep
                float hub = 1.0 - aastep(_InnerRadius, dist);

                // --- Dış halka: yön dişleri (chevron) ---
                float ringMask = aastep(_RingInner + 0.012, dist) * (1.0 - aastep(_RingOuter, dist));
                float ringMid = (_RingInner + _RingOuter) * 0.5;
                float ringHalf = max((_RingOuter - _RingInner) * 0.5, 1e-3);
                float across = abs(dist - ringMid) / ringHalf;  // 0 ortada, 1 kenarda
                // Ucu dönüş yönünü gösteren ">" şekli: kenarlar ortaya göre geride kalır.
                float chevU = a * _ChevronCount + _Direction * across * _ChevronSlant * 0.1;
                float chevIndex = floor(chevU);
                float chev = frac(chevU);
                float tooth = aastep(0.45, chev) * (1.0 - aastep(0.9, chev));

                // Ters dönen spiral: dişler ilerlemeyle dolar (sabit konumda, desenle dönmez).
                float baseIndex = frac(-(polar / TAU) * _Direction);
                float filled = step(baseIndex, _Progress) * step(0.001, _Progress);

                fixed3 ringCol = lerp(_InkColor.rgb, _AccentColor.rgb, tooth);
                float ringAlpha = lerp(0.55, 1.0, tooth);
                if (_Reversing > 0.5)
                {
                    // Çift yönlü işaret: ters yöndeki ince ikinci diş sırası.
                    float backU = a * _ChevronCount - _Direction * across * _ChevronSlant * 0.1 + 0.5;
                    float back = frac(backU);
                    float backTooth = aastep(0.6, back) * (1.0 - aastep(0.75, back)) * (1.0 - aastep(0.55, across));
                    ringCol = lerp(ringCol, _AccentColor.rgb * 0.6 + 0.2, backTooth * 0.8);
                    ringAlpha = max(ringAlpha, backTooth);
                    ringCol = lerp(ringCol * 0.55, ringCol, max(filled, 1.0 - step(0.001, _Progress)));
                    ringCol = lerp(ringCol, fixed3(1, 1, 1), filled * tooth * 0.35);
                }

                fixed4 col = fixed4(0, 0, 0, 0);
                col = lerp(col, disk, diskMask);
                col = lerp(col, fixed4(_InkColor.rgb, 1), hub);
                col = lerp(col, fixed4(ringCol, ringAlpha), ringMask);

                // Parlama / soluklaşma
                col.rgb = _Glow >= 0 ? lerp(col.rgb, 1.0 - (1.0 - col.rgb) * 0.4, _Glow) : col.rgb * (1.0 + _Glow * 0.6);
                col.a *= _Glow >= 0 ? 1.0 : (1.0 + _Glow * 0.5);

                col.a *= edge;
                col *= IN.color;
                return col;
            }
            ENDCG
        }
    }
}
