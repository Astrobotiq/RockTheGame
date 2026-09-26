Shader "Sprites/Custom/PaletteSwap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Palette Swap Colors 1)]
        _TargetColor1 ("Target Color 1", Color) = (1, 0.92, 0.016, 1)
        _ReplaceColor1 ("Replacement Color 1", Color) = (1, 0, 0, 1)
        _Tolerance1 ("Tolerance 1", Range(0.0, 1.0)) = 0.05

        [Header(Palette Swap Colors 2)]
        _TargetColor2 ("Target Color 2", Color) = (1, 0.8, 0.0, 1)
        _ReplaceColor2 ("Replacement Color 2", Color) = (0.8, 0, 0, 1)
        _Tolerance2 ("Tolerance 2", Range(0.0, 1.0)) = 0.05

        [Header(Palette Swap Colors 3)]
        _TargetColor3 ("Target Color 3", Color) = (0.9, 0.7, 0.0, 1)
        _ReplaceColor3 ("Replacement Color 3", Color) = (0.6, 0, 0, 1)
        _Tolerance3 ("Tolerance 3", Range(0.0, 1.0)) = 0.05

        [Header(Blend Setting)]
        _Blend ("Swap Blend", Range(0.0, 1.0)) = 0.0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            
            fixed4 _Color;
            
            fixed4 _TargetColor1;
            fixed4 _ReplaceColor1;
            float _Tolerance1;

            fixed4 _TargetColor2;
            fixed4 _ReplaceColor2;
            float _Tolerance2;

            fixed4 _TargetColor3;
            fixed4 _ReplaceColor3;
            float _Tolerance3;

            float _Blend;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 color = tex2D (_MainTex, uv);
#if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D (_AlphaTex, uv);
                color.a = alpha.r;
#endif //ETC1_EXTERNAL_ALPHA
                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 spriteCol = SampleSpriteTexture(IN.texcoord);

                fixed3 swappedRGB = spriteCol.rgb;

                // Color Swap 1
                float dist1 = distance(spriteCol.rgb, _TargetColor1.rgb);
                float shouldSwap1 = step(dist1, _Tolerance1);
                swappedRGB = lerp(swappedRGB, _ReplaceColor1.rgb, shouldSwap1 * _Blend);

                // Color Swap 2
                float dist2 = distance(spriteCol.rgb, _TargetColor2.rgb);
                float shouldSwap2 = step(dist2, _Tolerance2);
                swappedRGB = lerp(swappedRGB, _ReplaceColor2.rgb, shouldSwap2 * _Blend);

                // Color Swap 3
                float dist3 = distance(spriteCol.rgb, _TargetColor3.rgb);
                float shouldSwap3 = step(dist3, _Tolerance3);
                swappedRGB = lerp(swappedRGB, _ReplaceColor3.rgb, shouldSwap3 * _Blend);

                fixed4 finalColor = fixed4(swappedRGB, spriteCol.a);

                // Combine with Sprite Renderer's Vertex Tint / Color
                finalColor.rgb *= IN.color.rgb;
                
                // Premultiply alpha (Standard for Unity Sprites)
                finalColor.rgb *= finalColor.a;
                finalColor.a *= IN.color.a;

                return finalColor;
            }
        ENDCG
        }
    }
}
