/// <summary>
/// Performanslı matematiksel fonksiyonlarla eğrilik, kararma ve renk sapması içeren URP CRT Shader'ı.
/// </summary>
Shader "Hidden/Custom/CRTFilter"
{
    Properties
    {
        _Intensity ("Intensity", Float) = 1.0
        _Curvature ("Curvature", Float) = 3.0
        _Vignette ("Vignette", Float) = 1.0
        _RgbSplit ("RGB Split", Float) = 0.002
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

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                float2 centeredUV = uv * 2.0 - 1.0;
                float2 offset = centeredUV.yx / _Curvature;
                uv = uv + centeredUV * offset * offset;

                float bounds = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);

                float scanline = sin(uv.y * 800.0) * 0.04;

                float colorR = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(_RgbSplit, 0.0)).r;
                float colorG = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).g;
                float colorB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(_RgbSplit, 0.0)).b;

                half3 color = half3(colorR, colorG, colorB);
                
                color -= scanline;

                float vignette = smoothstep(1.5, 1.5 - _Vignette, length(centeredUV));
                color *= vignette;

                color *= bounds;

                half4 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                return lerp(originalColor, half4(color, 1.0), _Intensity);
            }
            ENDHLSL
        }
    }
}