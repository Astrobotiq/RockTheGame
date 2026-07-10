Shader "Sprites/Custom/PixelArtAcid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,0)
        [PerRendererData] _AlphaSplitEnabled ("Alpha Split Enabled", Float) = 0

        [Header(Acid Colors)]
        _AcidColor ("Acid Color", Color) = (0.12, 0.73, 0.16, 1)
        _BubbleColor ("Bubble Color", Color) = (0.35, 0.88, 0.28, 1)
        _FoamColor ("Foam Color", Color) = (0.78, 1.0, 0.44, 1)
        
        [Header(Wave Settings)]
        _AcidLevel ("Acid Level (0-1)", Range(0, 1)) = 0.85
        _FoamThickness ("Foam Thickness (Pixels)", Float) = 1.0
        _PixelScale ("Pixel Scale (XY Grid)", Vector) = (32, 32, 0, 0)
        _WaveSpeed ("Wave Speed (Time Multiplier)", Float) = 3.0
        _WaveFrequency ("Wave Wavelength (Pixels)", Float) = 32.0
        _WaveAmplitude ("Wave Amplitude (Pixels)", Float) = 2.0
        _FPS ("Animation FPS", Float) = 6.0
        
        [Header(Bubble Settings)]
        _BubbleDensity ("Bubble Spacing (Pixels)", Float) = 8.0
        _BubbleSpeed ("Bubble Speed (Pixels/Sec)", Float) = 15.0
        _BubbleThreshold ("Bubble Density Control (0-1)", Range(0, 1)) = 0.85
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
            fixed4 _BubbleColor;
            fixed4 _FoamColor;
            
            float _AcidLevel;
            float _FoamThickness;
            float4 _PixelScale;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;
            float _FPS;
            
            float _BubbleDensity;
            float _BubbleSpeed;
            float _BubbleThreshold;

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

            // Simple pseudo-random hash function for bubble generation
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
                // 1. Get the dynamic pixel scale (minimum 1 to prevent division by zero)
                float2 pixelScale = max(_PixelScale.xy, float2(1.0, 1.0));

                // 2. Snap the UV coordinates to the pixel grid
                float2 snappedUV = floor(IN.texcoord * pixelScale) / pixelScale;

                // Convert snapped UV to raw pixel coordinates (0 to width/height in pixels)
                float2 pixelCoords = snappedUV * pixelScale;

                // 3. Pixel Art Time Snapping (FPS)
                float snappedTime = floor(_Time.y * _FPS) / _FPS;

                // 4. Calculate wavy surface height mathematically in pixel coordinates
                // We divide by _WaveFrequency (wavelength in pixels) and scale by 2pi
                float waveFrequencyRad = (2.0 * 3.14159265) / max(_WaveFrequency, 1.0);
                float wave = sin(pixelCoords.x * waveFrequencyRad + snappedTime * _WaveSpeed) * _WaveAmplitude;
                
                // Base height of the acid level in pixel units
                float baseAcidHeight = _AcidLevel * pixelScale.y;
                float surfaceHeight = baseAcidHeight + wave;

                // 5. Sample the sprite texture
                fixed4 spriteCol = SampleSpriteTexture(snappedUV);

                // Discard pixels that are transparent in the original sprite OR above the wave height.
                if (spriteCol.a < 0.05 || pixelCoords.y > surfaceHeight)
                {
                    discard;
                }

                // Default color is the acid body
                fixed4 finalColor = _AcidColor;

                // 6. Foam (Surface line in pixel coordinates)
                float foamLimit = surfaceHeight - _FoamThickness;
                
                if (pixelCoords.y > foamLimit)
                {
                    finalColor = _FoamColor;
                }
                else
                {
                    // 7. Bubbles (calculated in pixel coordinates)
                    // Offset Y coordinate over time by bubble speed in pixels per second
                    float2 bubbleCoords = pixelCoords;
                    bubbleCoords.y -= snappedTime * _BubbleSpeed;
                    
                    // Divide by _BubbleDensity (which defines bubble grid cell size in pixels)
                    float2 bubbleGridUV = bubbleCoords / max(_BubbleDensity, 1.0);
                    
                    float2 bubbleCell = floor(bubbleGridUV);
                    float h = hash(bubbleCell);
                    
                    if (h > _BubbleThreshold)
                    {
                        // Draw a pixelated circle inside the bubble grid cell
                        float2 cellFrac = frac(bubbleGridUV);
                        float distToCenter = length(cellFrac - float2(0.5, 0.5));
                        
                        if (distToCenter < 0.3)
                        {
                            finalColor = _BubbleColor;
                        }
                    }
                }

                // Combine with Sprite Renderer's Vertex Tint / Color
                finalColor.rgb *= IN.color.rgb;
                // Premultiply alpha (Standard for Unity Sprites)
                finalColor.rgb *= spriteCol.a;
                finalColor.a = spriteCol.a * IN.color.a;

                return finalColor;
            }
        ENDCG
        }
    }
}
