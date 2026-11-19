Shader "UI/SpiralNode"
{
    // Spiral node görseli: "Nokta ve Çizgi" (docs/ui-research/spiral-node, B2). Ortada bir nokta, ondan çıkan tek Arşimet
    // çizgisi, 3 tur, çizgi kalınlığı = boşluk. Başka hiçbir şey yok.
    // Dönüş açısı script'ten gelir (_Angle): hit-stop ve pause sırasında script ilerletmez, görsel de durur.
    // Görsel normal node ile aynı boyda (20 birim) ve aynı dışa doğru sönmeyle çizilir. Çizgi dönüş yönünün arkasında kıvrılır; _Direction = -1 ise desen ayna görüntüsüdür.
    // _Curl: kıvrım (1 = _Turns tur, 0 = merkezden kenara düz kol). Ters dönen spiral yön değiştirmeye yaklaştıkça açılır,
    // çevrilince ters yöne yeniden sarılır (SpiralNode sürer). Çizgi kalınlığı kıvrımdan bağımsız sabittir.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaperColor ("Paper (zemin)", Color) = (0.9333, 0.9176, 0.7804, 1)
        _LineColor ("Line (çizgi + nokta)", Color) = (1, 0.3176, 0.0157, 1)

        _Angle ("Angle (radians, script)", Float) = 0
        _Direction ("Direction (+1 CCW, -1 CW)", Float) = 1
        _Turns ("Turns", Float) = 3
        _Curl ("Curl (1 = Turns, 0 = düz kol; script)", Range(0, 1)) = 1
        _LineWidth ("Line Width (pitch oranı)", Range(0.2, 0.8)) = 0.5
        _DotRadius ("Center Dot Radius (0-0.5)", Range(0, 0.5)) = 0.06
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
            float _Angle, _Direction, _Turns, _Curl, _LineWidth, _DotRadius, _Glow, _OuterFadeStart, _OuterFadePower;

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

                // Arşimet çizgisi: yarıçap tur başına pitch kadar büyür, çizgi dönüşün arkasında kalır.
                // Kıvrım _Curl ile azalır; turns → 0'da çizgi phi = 0 yönünde düz bir kol olur.
                float turns = max(_Turns, 0.5) * saturate(_Curl);
                float f = dist * 2.0 * turns + phi / TAU;      // 2·turns = 1 / pitch
                float d = abs(frac(f + 0.5) - 0.5);            // f biriminde çizgiye uzaklık
                // Dünya biriminde dik uzaklık = d / |∇f|; kalınlık tam kıvrık spiraldeki ile aynı kalır.
                float gradient = sqrt(4.0 * turns * turns + 1.0 / max(TAU * TAU * dist * dist, 1e-6));
                float halfWidth = _LineWidth * 0.5 * (0.5 / max(_Turns, 0.5));
                float stroke = 1.0 - aastep(halfWidth, d / gradient);

                float hub = 1.0 - aastep(_DotRadius, dist);
                float ink = max(stroke, hub);

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
