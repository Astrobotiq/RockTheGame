// Kodla üretilen beton yüzey (uGUI). Kurallar docs/ui-research/mashup-ui.html'deki mass() ile aynı:
//  - benek: denge > 0.12'de, her sanat pikselinde olasılık = denge/40 (boyada yarısı); %15'i 2 piksel geniş; yarısı açık, yarısı koyu
//  - kalıp çizgisi (yalnız beton): denge > 0.4, her 30 sanat pikselinde 1 koyu + altına 1 açık çizgi
//  - tie-hole (yalnız beton): denge > 0.6, çizgilerin 15 piksel üstünde, x = 22 + 50k, 3x3 koyu + içinde 2x2 açık
// Açık ton = taban + 0.075, koyu ton = taban - 0.082 (mockup'taki #9A9791 / #ADAAA4 / #85827D farkı).
// UV gerektirmez (UiArc'ın UV'si yok): desen yerel konumdan, _RectOrigin (sol-üst) ve _ArtPixel (bir sanat pikselinin yerel birimi) ile çıkar.
Shader "RockTheGame/UI/Concrete"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Balance ("Denge (0 Bauhaus, 1 brutalist)", Range(0,1)) = 0.55
        [Toggle] _Concrete ("Beton (1) / Boya (0)", Float) = 1
        _ArtPixel ("Bir sanat pikselinin yerel birimi", Float) = 4
        _RectOrigin ("Sol-ust (yerel xy)", Vector) = (0,0,0,0)
        _Seed ("Benek tohumu", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 localPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Balance;
            float _Concrete;
            float _ArtPixel;
            float4 _RectOrigin;
            float _Seed;

            static const float LIGHT = 0.075;
            static const float DARK = -0.082;

            float hash(float2 p)
            {
                return frac(sin(dot(p + _Seed, float2(127.1, 311.7))) * 43758.5453);
            }

            // Bir sanat piksel hücresinin beneği: 0 = yok, +1 = açık, -1 = koyu. Geniş benek sağdaki hücreye de taşar.
            float speckAt(float2 cell, float p, out float wide)
            {
                float h = hash(cell);
                wide = 0;
                if (h >= p) return 0;
                wide = step(hash(cell + 17.3), 0.15);
                return hash(cell + 91.7) < 0.5 ? 1 : -1;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                OUT.localPos = v.vertex.xy;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                float v = saturate(_Balance);
                float2 px = float2(IN.localPos.x - _RectOrigin.x, _RectOrigin.y - IN.localPos.y) / max(_ArtPixel, 0.0001);
                float2 cell = floor(px);
                float shade = 0;

                // Benek
                if (v > 0.12)
                {
                    float p = v / 40.0 * lerp(0.5, 1.0, step(0.5, _Concrete));
                    float wide;
                    float s = speckAt(cell, p, wide);
                    float wideLeft;
                    float sl = speckAt(cell - float2(1, 0), p, wideLeft);
                    if (s != 0) shade = s;
                    else if (sl != 0 && wideLeft > 0.5) shade = sl;
                    shade = shade > 0 ? LIGHT : (shade < 0 ? DARK : 0);
                }

                if (_Concrete > 0.5)
                {
                    // Kalıp çizgileri: her 30 sanat pikselinde koyu, altında açık
                    if (v > 0.4 && cell.y >= 30)
                    {
                        float m = fmod(cell.y, 30);
                        if (m < 0.5) shade = DARK;
                        else if (m < 1.5) shade = LIGHT;
                    }

                    // Tie-hole: çizginin 15 piksel üstünde 3x3 koyu, içinde 2x2 açık
                    if (v > 0.6 && cell.y >= 15 && cell.x >= 22)
                    {
                        float hy = fmod(cell.y + 15, 30);
                        float hx = fmod(cell.x - 22, 50);
                        if (hy < 2.5 && hx < 2.5)
                            shade = (hy > 0.5 && hx > 0.5) ? LIGHT : DARK;
                    }
                }

                color.rgb = saturate(color.rgb + shade);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
