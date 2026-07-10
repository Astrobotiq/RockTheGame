Shader "Sprites/Custom/FlowingPixelAcid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Grid and Sync Settings)]
        _PixelScale ("Pixel Scale (XY Grid)", Vector) = (32, 32, 0, 0)
        _FPS ("Animation FPS", Float) = 8.0

        [Header(Acid Colors)]
        _AcidColor ("Acid Color (Deep)", Color) = (0.12, 0.73, 0.16, 1)
        _MidColor ("Acid Color (Mid)", Color) = (0.35, 0.88, 0.28, 1)
        _FoamColor ("Foam/Highlight Color", Color) = (0.78, 1.0, 0.44, 1)

        [Header(Flow Settings)]
        _FlowDirection ("Flow Direction (XY Vector)", Vector) = (0, -1, 0, 0)
        _FlowSpeed ("Flow Speed (Pixels/Sec)", Float) = 15.0

        [Header(Streak Settings)]
        _StreakScale ("Streak Grid Scale (Pixels)", Float) = 8.0
        _StreakThreshold1 ("Mid Streak Threshold", Range(0, 1)) = 0.45
        _StreakThreshold2 ("Foam Streak Threshold", Range(0, 1)) = 0.75
        _Distortion ("Flow Wave Distortion", Float) = 3.0
        _DistortionFreq ("Flow Wave Distortion Freq", Float) = 0.15

        [Header(Bubble Settings)]
        _BubbleDensity ("Bubble Spacing (Pixels)", Float) = 12.0
        _BubbleSpeed ("Bubble Flow Speed (Pixels/Sec)", Float) = 22.0
        _BubbleThreshold ("Bubble Density Control (0-1)", Range(0, 1)) = 0.8
        _BubbleColor ("Bubble Color", Color) = (0.78, 1.0, 0.44, 1)

        [Header(Outline Settings)]
        [MaterialToggle] _UseOutline ("Enable Edge Outline", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0.05, 0.12, 0.0, 1)
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
            fixed4 _AcidColor;
            fixed4 _MidColor;
            fixed4 _FoamColor;
            
            float4 _PixelScale;
            float _FPS;
            
            float4 _FlowDirection;
            float _FlowSpeed;
            
            float _StreakScale;
            float _StreakThreshold1;
            float _StreakThreshold2;
            float _Distortion;
            float _DistortionFreq;
            
            float _BubbleDensity;
            float _BubbleSpeed;
            float _BubbleThreshold;
            fixed4 _BubbleColor;
            
            float _UseOutline;
            fixed4 _OutlineColor;

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

            // Basit psödorastgele gürültü fonksiyonu
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
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
                // 1. Dinamik piksel ölçeğini al (0'a bölme hatasını önlemek için min 1.0)
                float2 pixelScale = max(_PixelScale.xy, float2(1.0, 1.0));

                // 2. UV koordinatlarını piksel ızgarasına (grid) eşitle
                float2 snappedUV = floor(IN.texcoord * pixelScale) / pixelScale;
                float2 pixelCoords = snappedUV * pixelScale;

                // 3. Zaman yuvarlama (Animasyon FPS hızı)
                float snappedTime = floor(_Time.y * _FPS) / _FPS;

                // 4. Orijinal sprite dokusunu örnekle, şeffafsa çizme
                fixed4 spriteCol = SampleSpriteTexture(snappedUV);
                if (spriteCol.a < 0.05)
                {
                    discard;
                }

                // 5. Akış yönü dönüşümü (Flow Direction Vector)
                float2 dir = normalize(_FlowDirection.xy);
                if (length(dir) < 0.001)
                {
                    dir = float2(0, -1);
                }

                // Akış doğrultusundaki uzaklığı ve dik doğrultudaki uzaklığı hesapla
                float flowDist = dot(pixelCoords, dir);
                float perpDist = dot(pixelCoords, float2(-dir.y, dir.x));

                // 6. Prosedürel akış çizgilerini (streaks) hesapla
                float scaleRad = (2.0 * 3.14159265) / max(_StreakScale, 1.0);
                float animatedFlow = flowDist - snappedTime * _FlowSpeed;

                // Çizgilerin dalgalanarak akması için yanlara doğru salınım gürültüsü
                float waveOffset = sin(animatedFlow * _DistortionFreq) * _Distortion;
                
                // Çizgilerin el yapımı retro durması için iç içe sinüs dalgaları
                float w1 = sin((perpDist + waveOffset) * scaleRad);
                float w2 = cos((perpDist * 1.5 - animatedFlow * 0.25) * scaleRad);
                float streakVal = (w1 + w2) * 0.5 + 0.5;

                // Çizgi değerlerine göre katman renklerini belirle
                fixed4 finalColor = _AcidColor;
                if (streakVal > _StreakThreshold2)
                {
                    finalColor = _FoamColor;
                }
                else if (streakVal > _StreakThreshold1)
                {
                    finalColor = _MidColor;
                }

                // 7. Kabarcıklar (akış yönünde akan baloncuklar)
                float bubbleFlow = flowDist - snappedTime * _BubbleSpeed;
                float2 bubbleCoords = float2(perpDist, bubbleFlow);
                
                float2 bubbleGridUV = bubbleCoords / max(_BubbleDensity, 1.0);
                float2 bubbleCell = floor(bubbleGridUV);
                float h = hash(bubbleCell);
                
                if (h > _BubbleThreshold)
                {
                    float2 cellFrac = frac(bubbleGridUV);
                    float distToCenter = length(cellFrac - float2(0.5, 0.5));
                    
                    if (distToCenter < 0.3)
                    {
                        finalColor = _BubbleColor;
                    }
                }

                // 8. Kenar Konturu (Outline) - Eğer aktifse
                if (_UseOutline > 0.5)
                {
                    float2 pixelSize = 1.0 / pixelScale;
                    
                    float aUp    = SampleSpriteTexture(snappedUV + float2(0, pixelSize.y)).a;
                    float aDown  = SampleSpriteTexture(snappedUV - float2(0, pixelSize.y)).a;
                    float aLeft  = SampleSpriteTexture(snappedUV - float2(pixelSize.x, 0)).a;
                    float aRight = SampleSpriteTexture(snappedUV + float2(pixelSize.x, 0)).a;
                    
                    if (aUp < 0.05 || aDown < 0.05 || aLeft < 0.05 || aRight < 0.05)
                    {
                        finalColor = _OutlineColor;
                    }
                }

                // Sprite Renderer rengi ile çarp ve Alpha değerini uyarla
                finalColor.rgb *= IN.color.rgb;
                finalColor.rgb *= spriteCol.a;
                finalColor.a = spriteCol.a * IN.color.a;

                return finalColor;
            }
        ENDCG
        }
    }
}
