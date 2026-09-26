Shader "Sprites/Custom/ProceduralPixelAcid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Grid and World Settings)]
        [MaterialToggle] _UseSpriteMask ("Use Sprite Mask (Alpha)", Float) = 0
        _PixelScale ("Pixels Per World Unit (XY)", Vector) = (16, 16, 0, 0)
        _FPS ("Animation FPS", Float) = 8.0

        [Header(Acid Color Bands)]
        _TopColor ("Top Layer (#C4F02C)", Color) = (0.7686, 0.9412, 0.1725, 1.0)
        _MidColor ("Middle Layer (#8FD400)", Color) = (0.5608, 0.8314, 0.0, 1.0)
        _DeepColor ("Deep Layer (#4E9A00)", Color) = (0.3059, 0.6039, 0.0, 1.0)
        _TopBandThickness ("Top Layer Thickness (Px)", Float) = 4.0
        _MidBandThickness ("Middle Layer Thickness (Px)", Float) = 8.0

        [Header(Wave Settings)]
        _AcidLevel ("Acid Level (World Unit Offset)", Float) = 0.5
        _WaveFreq1 ("Wave 1 Frequency", Float) = 0.6
        _WaveSpeed1 ("Wave 1 Speed", Float) = 3.0
        _WaveAmp1 ("Wave 1 Amplitude (Px)", Float) = 2.0
        _WaveFreq2 ("Wave 2 Frequency", Float) = 1.2
        _WaveSpeed2 ("Wave 2 Speed", Float) = 4.5
        _WaveAmp2 ("Wave 2 Amplitude (Px)", Float) = 0.8

        [Header(Outline Settings)]
        _OutlineColor ("Outline Color (#0D1F00)", Color) = (0.0510, 0.1216, 0.0, 1.0)
        _OutlineThickness ("Outline Thickness (Px)", Float) = 1.0

        [Header(Bubble Settings)]
        _BubbleColor ("Bubble Highlight (#F5FFB8)", Color) = (0.9608, 1.0, 0.7216, 1.0)
        _BubbleGridSize ("Bubble Cell Size (Px)", Float) = 10.0
        _BubbleThreshold ("Bubble Density Threshold (0-1)", Range(0, 1)) = 0.8
        _BubbleSpeed ("Bubble Lifecycle Speed", Float) = 2.5
        _BubbleMaxRadius ("Max Bubble Radius (Px)", Float) = 2.5

        [Header(Steam Settings)]
        _SteamColor ("Steam Color (#E8FF6B)", Color) = (0.9098, 1.0, 0.4196, 0.45)
        _SteamHeight ("Steam Max Height (Px)", Float) = 12.0
        _SteamSpeed ("Steam Scroll Speed", Float) = 2.0
        _SteamPulseSpeed ("Steam Pulse Speed", Float) = 1.5
        _SteamThreshold ("Steam Density Threshold", Range(0, 1)) = 0.35
        _SteamFreqX1 ("Steam X Freq 1", Float) = 0.8
        _SteamFreqY1 ("Steam Y Freq 1", Float) = 0.6
        _SteamFreqX2 ("Steam X Freq 2", Float) = 1.2
        _SteamFreqY2 ("Steam Y Freq 2", Float) = 0.9
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
            #pragma target 3.0
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
                float4 worldPos : TEXCOORD1; // Dünya koordinatlarını fragmana taşımak için
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            
            fixed4 _Color;
            float _UseSpriteMask;
            
            // Çözünürlük (PPU) ve Kare Hızı
            float4 _PixelScale;
            float _FPS;

            // Renk Katmanları
            fixed4 _TopColor;
            fixed4 _MidColor;
            fixed4 _DeepColor;
            float _TopBandThickness;
            float _MidBandThickness;

            // Dalga Ayarları
            float _AcidLevel;
            float _WaveFreq1;
            float _WaveSpeed1;
            float _WaveAmp1;
            float _WaveFreq2;
            float _WaveSpeed2;
            float _WaveAmp2;

            // Kontur (Outline) Ayarları
            fixed4 _OutlineColor;
            float _OutlineThickness;

            // Kabarcık Ayarları
            fixed4 _BubbleColor;
            float _BubbleGridSize;
            float _BubbleThreshold;
            float _BubbleSpeed;
            float _BubbleMaxRadius;

            // Buhar (Steam) Ayarları
            fixed4 _SteamColor;
            float _SteamHeight;
            float _SteamSpeed;
            float _SteamPulseSpeed;
            float _SteamThreshold;
            float _SteamFreqX1;
            float _SteamFreqY1;
            float _SteamFreqX2;
            float _SteamFreqY2;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                // Lokal verteksi dünya koordinatına çeviriyoruz
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            // Basit psödörastgele gürültü fonksiyonu
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
                // 1. Dünya Koordinatlarında Piksel Snap
                // PixelScale.xy burada Pixels Per Unit (PPU) olarak işlev görür.
                float2 pixelScale = max(_PixelScale.xy, float2(1.0, 1.0));
                
                // Dünya pozisyonunu piksel boyutuna göre yuvarlıyoruz
                float2 snappedWorldPos = floor(IN.worldPos.xy * pixelScale) / pixelScale;
                float2 pixelCoords = snappedWorldPos * pixelScale;

                // 2. Zaman Yuvarlama (Retro FPS animasyon hızı)
                float snappedTime = floor(_Time.y * _FPS) / _FPS;

                // 3. Objeye ve Dünyaya Duyarlı Yüzey Seviyesi
                // Objelerin pivotu değiştikçe asit seviyesi düzgün kalsın diye objenin dünya orijinini alıyoruz
                float3 objectOrigin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Asit temel yüksekliği dünya birimi cinsinden
                float baseAcidWorldY = objectOrigin.y + _AcidLevel;
                // Dünyadaki piksel gridine snap edilmiş temel yükseklik
                float snappedBaseWorldY = floor(baseAcidWorldY * pixelScale.y) / pixelScale.y;

                // Dünya piksel koordinatı X kullanılarak kesintisiz dalga hesabı
                float wave = sin(pixelCoords.x * _WaveFreq1 + snappedTime * _WaveSpeed1) * _WaveAmp1;
                wave += cos(pixelCoords.x * _WaveFreq2 - snappedTime * _WaveSpeed2) * _WaveAmp2;
                
                // Dalganın dünya koordinatındaki karşılığı (piksel yüksekliğini dünya birimine çeviriyoruz)
                float surfaceHeightWorldY = snappedBaseWorldY + (floor(wave) / pixelScale.y);

                // Kontur/outline üst sınırı (dünya birimi olarak)
                float outlineLimitWorldY = surfaceHeightWorldY + (floor(_OutlineThickness) / pixelScale.y);

                // Varsayılan boş piksel (transparan)
                fixed4 finalColor = fixed4(0, 0, 0, 0);

                // Sprite maskelemesi desteği (İstenirse kapatılabilir)
                fixed4 spriteCol = SampleSpriteTexture(IN.texcoord);
                if (_UseSpriteMask > 0.5 && spriteCol.a < 0.01)
                {
                    discard;
                }

                // 4. Dünya Koordinatına Göre Katman Çizimi
                if (snappedWorldPos.y <= surfaceHeightWorldY)
                {
                    // ------------------ ASİT GÖVDESİ (ASİT HAVUZU İÇİ) ------------------

                    // A. Dünya Koordinatlı Posterize Dikey Renk Gradyanı (Banding)
                    float distFromSurface = (surfaceHeightWorldY - snappedWorldPos.y) * pixelScale.y;
                    if (distFromSurface < _TopBandThickness)
                    {
                        finalColor = _TopColor;
                    }
                    else if (distFromSurface < (_TopBandThickness + _MidBandThickness))
                    {
                        finalColor = _MidColor;
                    }
                    else
                    {
                        finalColor = _DeepColor;
                    }

                    // B. Dünya Koordinatlı Hücresel Kabarcık Daireleri (Dikişsiz kabarcıklar)
                    float2 bubbleCell = floor(pixelCoords / _BubbleGridSize);
                    float2 cellUV = frac(pixelCoords / _BubbleGridSize);
                    float cellHash = hash(bubbleCell);

                    if (cellHash > _BubbleThreshold)
                    {
                        // Hücre içi rastgele merkez kaydırma
                        float randX = hash(bubbleCell + float2(1.0, 0.0));
                        float randY = hash(bubbleCell + float2(0.0, 1.0));
                        float2 cellOffset = float2(randX, randY) * 0.4 - 0.2;
                        float2 bubbleCenter = float2(0.5, 0.5) + cellOffset;

                        // Zaman ve hücre faz kaymasıyla beliren/kaybolan döngüsel faz
                        float bubblePhase = snappedTime * _BubbleSpeed + cellHash * 6.2831;
                        float lifecycle = frac(bubblePhase / 6.2831);

                        // Döngünün ilk %80'inde kabarcık görünür
                        if (lifecycle < 0.8)
                        {
                            float tNorm = lifecycle / 0.8;
                            float currentRadius = _BubbleMaxRadius * sin(tNorm * 3.14159);
                            float cellRadius = currentRadius / _BubbleGridSize;
                            
                            float dist = length(cellUV - bubbleCenter);

                            if (dist < cellRadius)
                            {
                                // İçi boş kabarcık halkası çizimi (1 piksel kalınlığında)
                                float pixelInCell = 1.0 / _BubbleGridSize;
                                if (dist > cellRadius - pixelInCell)
                                {
                                    finalColor = _BubbleColor;
                                }
                                
                                // Sol-üst specular highlight noktası
                                float2 specOffset = float2(-0.15, -0.15) * cellRadius;
                                if (length(cellUV - bubbleCenter - specOffset) < pixelInCell * 0.8)
                                {
                                    finalColor = _BubbleColor;
                                }
                            }
                        }
                    }
                }
                else if (snappedWorldPos.y <= outlineLimitWorldY)
                {
                    // ------------------ DİKİŞSİZ ÜST KONTUR ÇİZGİSİ (OUTLINE) ------------------
                    finalColor = _OutlineColor;
                }
                else
                {
                    // ------------------ DİKİŞSİZ BUHAR / PARILTI EFEKTİ (STEAM) ------------------
                    float distAboveSurface = (snappedWorldPos.y - outlineLimitWorldY) * pixelScale.y;
                    
                    if (distAboveSurface < _SteamHeight)
                    {
                        // Buharın dikey boyutu zamanla pulslanır (nefes alma hareketi)
                        float pulse = sin(snappedTime * _SteamPulseSpeed) * 0.15 + 0.85;
                        float heightFade = 1.0 - (distAboveSurface / max(_SteamHeight * pulse, 1.0));
                        
                        // Dünya koordinatlı gürültü dalgaları sayesinde kesintisiz buhar
                        float steamTime = snappedTime * _SteamSpeed;
                        float n1 = sin(pixelCoords.x * _SteamFreqX1 + steamTime) * cos(pixelCoords.y * _SteamFreqY1 - steamTime * 1.5);
                        float n2 = cos(pixelCoords.x * _SteamFreqX2 - steamTime * 0.8) * sin(pixelCoords.y * _SteamFreqY2 - steamTime * 1.2);
                        
                        float combinedNoise = (n1 + n2 * 0.5 + 0.5) / 1.5;
                        float steamIntensity = heightFade * combinedNoise;
                        
                        // Buharın retro geçişi için posterize edilme işlemi
                        float steamSteps = floor(steamIntensity * 3.0) / 3.0;
                        
                        if (steamSteps > 0.0)
                        {
                            float finalAlpha = _SteamColor.a * steamSteps;
                            finalColor = float4(_SteamColor.rgb * finalAlpha, finalAlpha);
                        }
                        else
                        {
                            discard;
                        }
                    }
                    else
                    {
                        discard;
                    }
                }

                // Sprite rengi (tint) ile çarp
                finalColor.rgb *= IN.color.rgb;

                // Unity Sprite Renderer Alpha Blending Maskeleme Mantığı
                float maskAlpha = _UseSpriteMask > 0.5 ? spriteCol.a : 1.0;

                if (snappedWorldPos.y <= outlineLimitWorldY)
                {
                    finalColor.rgb *= IN.color.a * maskAlpha;
                    finalColor.a = IN.color.a * maskAlpha;
                }
                else
                {
                    finalColor.rgb *= IN.color.a * maskAlpha;
                    finalColor.a *= IN.color.a * maskAlpha;
                }

                return finalColor;
            }
        ENDCG
        }
    }
}
