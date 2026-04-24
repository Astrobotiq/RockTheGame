/// <summary>
/// Çözünürlük çökmesine karşı korumalı, Merkez-Örneklemeli (Center-Sampled) pikselleştirme içeren URP CRT Shader'ı.
/// </summary>
Shader "Hidden/Custom/CRTFilter"
{
    Properties
    {
        _Intensity ("Intensity", Float) = 1.0
        _Curvature ("Curvature", Float) = 3.0
        _Vignette ("Vignette", Float) = 1.0
        _RgbSplit ("RGB Split", Float) = 0.002
        _PixelResolution ("Pixel Resolution", Vector) = (320.0, 240.0, 0.0, 0.0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "CRTFilterPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _Curvature;
            float _Vignette;
            float _RgbSplit;
            float4 _PixelResolution;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 uv = input.texcoord;
                float2 centeredUV = uv * 2.0 - 1.0;

                // Eğrilik Hesaplaması (Pürüzsüz UV üzerinde)
                float2 curveOffset = centeredUV.yx * centeredUV.yx;
                uv += centeredUV * curveOffset * (_Curvature * 0.05);

                // Ekran Sınırları
                float bounds = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);

                // Pikselleştirme (Merkezden örnekleyerek precision hatalarını önler)
                float2 safeRes = max(_PixelResolution.xy, float2(2.0, 2.0));
                float2 pUV = (floor(uv * safeRes) + 0.5) / safeRes; 
                
                // Güvenlik: Çözünürlük 2'den küçükse efekti bypass et
                float pixelationActive = step(2.0, _PixelResolution.x) * step(2.0, _PixelResolution.y);
                float2 pixelatedUV = lerp(uv, pUV, pixelationActive);

                // Tarama Çizgileri (Pürüzsüz cam üzerinde hesaplanır)
                float scanline = sin(uv.y * 800.0) * 0.04;

                // Doku Okuma (Renk ayrışması ve pikselleşmiş UV ile)
                float colorR = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pixelatedUV + float2(_RgbSplit, 0.0)).r;
                float colorG = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pixelatedUV).g;
                float colorB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pixelatedUV - float2(_RgbSplit, 0.0)).b;

                half3 color = half3(colorR, colorG, colorB);
                
                color -= scanline;

                // Kararma (Vignette)
                float dist = length(uv * 2.0 - 1.0);
                float vignette = smoothstep(1.5, 1.5 - max(_Vignette, 0.01), dist);
                color *= vignette;

                color *= bounds;

                half4 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                return lerp(originalColor, half4(color, originalColor.a), _Intensity);
            }
            ENDHLSL
        }
    }
}