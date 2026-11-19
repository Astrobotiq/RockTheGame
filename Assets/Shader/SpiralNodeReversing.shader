Shader "UI/SpiralNodeReversing"
{
    // Ters dönen spiral görseli: A2 "Vasarely Kanatları" (docs/ui-research/spiral-node/research/A-opart.js). Merkezde buluşan
    // _Blades adet kalın kanat, yarıçap boyunca _Twist radyan burulur; kanat ve boşluk eşit genişlikte.
    // _Curl (script): 1 = tam burulma, 0 = düz kanatlar. Ters dönen spiral yön değiştirmeye yaklaştıkça kanatlar düzleşir,
    // çevrilince ters yöne yeniden burulur (SpiralNode sürer). Normal spiral UI/SpiralNode'u kullanır.
    // Açı/yön/parlama/dış sönme UI/SpiralNode ile aynı sözleşme; kanatlar ortalanmış, _Direction aynasında yerinde kalır.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaperColor ("Paper (zemin)", Color) = (0.9451, 0.9020, 0.7804, 1)
        _LineColor ("Blade (kanat)", Color) = (0.0157, 0.3451, 0.4235, 1)

        _Angle ("Angle (radians, script)", Float) = 0
        _Direction ("Direction (+1 CCW, -1 CW)", Float) = 1
        _Blades ("Blades", Float) = 5
        _Twist ("Twist (radyan, merkezden kenara)", Float) = 1.95
        _BladeFill ("Blade Fill (0-1)", Range(0.1, 0.9)) = 0.5
        _Curl ("Curl (1 = Twist, 0 = düz kanat; script)", Range(0, 1)) = 1
        _Glow ("Glow (-1 dim .. 1 bright)", Range(-1, 1)) = 0
        _OuterFadeStart ("Outer Fade Start (0-1)", Range(0, 1)) = 0.4
        _OuterFadePower ("Outer Fade Power", Range(0.1, 5)) = 0.96

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
            fixed4 _PaperColor;
            fixed4 _LineColor;
            float _Angle, _Direction, _Blades, _Twist, _BladeFill, _Curl, _Glow, _OuterFadeStart, _OuterFadePower;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord - 0.5;
                float dist = length(uv);
                float edge = 1.0 - aastep(0.5, dist);
                if (edge <= 0.0) return fixed4(0, 0, 0, 0);

                float dir = _Direction >= 0.0 ? 1.0 : -1.0;
                // Ayna: CW'de x ters çevrilir; desen kendi çerçevesinde _Angle*dir kadar döner (dönüş yönüne göre).
                float phi = atan2(uv.y, uv.x * dir) - _Angle * dir;

                // Kanat merkezi yarıçap boyunca geri burulur (A2: a = base − twist·r/R); _Curl burulmayı azaltır.
                float s = phi + _Twist * saturate(_Curl) * (dist / 0.5);
                float blades = max(_Blades, 1.0);
                float d = abs(frac(s * blades / TAU + 0.5) - 0.5);   // 0 kanat ortası, 0.5 boşluk ortası
                float ink = 1.0 - aastep(_BladeFill * 0.5, d);

                fixed4 col = fixed4(lerp(_PaperColor.rgb, _LineColor.rgb, ink), 1.0);

                // Parlama / soluklaşma
                col.rgb = _Glow >= 0 ? lerp(col.rgb, 1.0 - (1.0 - col.rgb) * 0.4, _Glow) : col.rgb * (1.0 + _Glow * 0.6);
                col.a *= _Glow >= 0 ? 1.0 : (1.0 + _Glow * 0.5);

                // Normal node (CircularRings) ile aynı dışa doğru sönme: aynı ayak izi.
                float nr = dist / 0.5;
                if (nr > _OuterFadeStart)
                {
                    float t = (nr - _OuterFadeStart) / (1.0 - _OuterFadeStart + 0.0001);
                    col.a *= pow(1.0 - saturate(t), _OuterFadePower);
                }

                col.a *= edge;
                col *= IN.color;
                return col;
            }
            ENDCG
        }
    }
}
