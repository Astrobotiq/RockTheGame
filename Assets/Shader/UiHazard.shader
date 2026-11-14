// Tehlike şeridi (uGUI): sarı zemin üstünde çapraz ink şeritler. Kural docs/ui-research/mashup-ui.html'deki hazardPath() ile aynı:
//  - 45° ("/" yönü), her 16 sanat pikselinde 8 piksel ink şerit; desen elemanın sol-alt köşesine göre başlar.
//  - Şerit kenarları sanat pikseli ızgarasına oturur (basamaklı, piksel sanatı gibi); doku yok, her uzunlukta dikişsiz.
//  - Denge < 0,3 iken şerit çizilmez, düz sarı kalır (_Stripes = 0; UiHazardStripes karar verir).
// UV gerektirmez: konum mesh köşesinden (kök canvas uzayı), _RectOrigin sol-alt köşe, _ArtPixel bir sanat pikselinin yerel birimi.
Shader "RockTheGame/UI/Hazard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Zemin (sari)", Color) = (0.99,0.68,0.02,1)
        _Ink ("Serit (ink)", Color) = (0.08,0.05,0.11,1)

        [Toggle] _Stripes ("Serit var", Float) = 1
        _ArtPixel ("Bir sanat pikselinin yerel birimi", Float) = 4
        _Period ("Serit periyodu (sanat pikseli)", Float) = 16
        _StripeWidth ("Serit genisligi (sanat pikseli)", Float) = 8
        _RectOrigin ("Sol-alt (yerel xy)", Vector) = (0,0,0,0)
        _GapAlpha ("Serit disi alfa (1 = dolu zemin)", Range(0,1)) = 1
        _Scroll ("Serit kaymasi (sanat pikseli, tam sayiya iner)", Float) = 0

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
            #pragma target 2.0

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
            fixed4 _Ink;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Stripes;
            float _ArtPixel;
            float _Period;
            float _StripeWidth;
            float4 _RectOrigin;
            float _GapAlpha;
            float _Scroll;

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

                if (_Stripes > 0.5)
                {
                    // Sol-alt köşeden sanat pikseli hücresi (y yukarı). "/" şeritlerde x - y sabittir.
                    float2 cell = floor((IN.localPos - _RectOrigin.xy) / max(_ArtPixel, 0.0001));
                    float period = max(_Period, 1.0);
                    float m = cell.x - cell.y + floor(_Scroll);
                    m -= period * floor(m / period);
                    bool onStripe = m < _StripeWidth;
                    color.rgb = onStripe ? _Ink.rgb : color.rgb;
                    color.a *= onStripe ? 1.0 : _GapAlpha;
                }

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
