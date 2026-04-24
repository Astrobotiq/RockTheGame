// Hidden/Custom/CRTFilter
// Tüm CRT parametrelerini destekleyen URP Render Graph uyumlu shader.
Shader "Hidden/Custom/CRTFilter"
{
    Properties
    {
        _ScreenBend         ("Screen Bend",       Float) = 0.0
        _ScreenOverscan     ("Screen Overscan",   Float) = 0.0
        _PixelResolution    ("Pixel Resolution",  Vector) = (320, 240, 0, 0)

        _VignetteSize       ("Vignette Size",     Float) = 0.5
        _VignetteSmooth     ("Vignette Smooth",   Float) = 0.4
        _VignetteRound      ("Vignette Round",    Float) = 1.0

        _Blur               ("Blur",              Float) = 0.0
        _Bleed              ("Bleed",             Float) = 0.0
        _Smidge             ("Smidge",            Float) = 0.0

        _ScanlinesStrength  ("Scanlines Strength",Float) = 0.04
        _ApertureStrength   ("Aperture Strength", Float) = 0.0
        _Shadowlines        ("Shadowlines",       Float) = 0.0
        _ShadowlinesSpeed   ("Shadowlines Speed", Float) = 1.0
        _ShadowlinesAlpha   ("Shadowlines Alpha", Float) = 0.3
        _NoiseSize          ("Noise Size",        Float) = 2.0
        _NoiseSpeed         ("Noise Speed",       Float) = 10.0
        _NoiseAlpha         ("Noise Alpha",       Float) = 0.0

        _Brightness         ("Brightness",        Float) = 1.0
        _Contrast           ("Contrast",          Float) = 1.0
        _Gamma              ("Gamma",             Float) = 1.0
        _Red                ("Red",               Float) = 1.0
        _Green              ("Green",             Float) = 1.0
        _Blue               ("Blue",              Float) = 1.0

        _RedOffset          ("Red Offset",        Float) = 0.0
        _GreenOffset        ("Green Offset",      Float) = 0.0
        _BlueOffset         ("Blue Offset",       Float) = 0.0

        _Intensity          ("Intensity",         Float) = 1.0
        _CrtTime            ("CRT Time",          Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
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

            // ── Uniforms ─────────────────────────────────────────────────────
            float  _ScreenBend;
            float  _ScreenOverscan;
            float2 _PixelResolution;

            float  _VignetteSize;
            float  _VignetteSmooth;
            float  _VignetteRound;

            float  _Blur;
            float  _Bleed;
            float  _Smidge;

            float  _ScanlinesStrength;
            float  _ApertureStrength;
            float  _Shadowlines;
            float  _ShadowlinesSpeed;
            float  _ShadowlinesAlpha;
            float  _NoiseSize;
            float  _NoiseSpeed;
            float  _NoiseAlpha;

            float  _Brightness;
            float  _Contrast;
            float  _Gamma;
            float  _Red;
            float  _Green;
            float  _Blue;

            float  _RedOffset;
            float  _GreenOffset;
            float  _BlueOffset;

            float  _Intensity;
            float  _CrtTime;

            // ── Helpers ──────────────────────────────────────────────────────

            float hash(float2 p)
            {
                p = frac(p * float2(443.8975, 397.2973));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            // 7-tap separable Gaussian for a single channel (ch: 0=R 1=G 2=B).
            // stepDir = per-tap UV offset = (texelSize * _Blur).
            float GaussChannel(float2 uv, float2 stepDir, int ch)
            {
                const float w[7] = { 0.0625, 0.125, 0.1875, 0.25, 0.1875, 0.125, 0.0625 };
                float result = 0;
                UNITY_UNROLL
                for (int i = 0; i < 7; ++i)
                {
                    float3 s = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                   uv + stepDir * (i - 3)).rgb;
                    result += (ch == 0 ? s.r : (ch == 1 ? s.g : s.b)) * w[i];
                }
                return result;
            }

            // Rightward phosphor bleed for a single channel.
            // _Bleed 0=off, 1=strong smear. Decay per tap: (1 - bleed*0.35)^i
            float BleedChannel(float2 uv, float amount, int ch)
            {
                float pixW = 1.0 / _PixelResolution.x;
                float3 base = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float acc  = (ch == 0 ? base.r : (ch == 1 ? base.g : base.b));
                float w    = 1.0;
                float wSum = 1.0;
                UNITY_UNROLL
                for (int i = 1; i <= 6; ++i)
                {
                    w    *= (1.0 - amount * 0.35);
                    float3 tap = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                     uv + float2(pixW * i, 0)).rgb;
                    acc  += (ch == 0 ? tap.r : (ch == 1 ? tap.g : tap.b)) * w;
                    wSum += w;
                }
                return acc / wSum;
            }

            // ── Fragment ─────────────────────────────────────────────────────
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 1. Overscan
                float2 uv = input.texcoord;
                uv = (uv - 0.5) * (1.0 + _ScreenOverscan) + 0.5;

                // 2. Screen bend
                float2 centeredUV = uv * 2.0 - 1.0;
                if (_ScreenBend > 0.001)
                {
                    float2 bendOffset = centeredUV.yx / _ScreenBend;
                    uv = uv + centeredUV * bendOffset * bendOffset;
                    centeredUV = uv * 2.0 - 1.0;
                }

                // 3. Pixel snap
                float2 snappedUV = uv;
                if (_PixelResolution.x > 0 && _PixelResolution.y > 0)
                {
                    snappedUV = floor(uv * _PixelResolution) / _PixelResolution;
                    snappedUV += 0.5 / _PixelResolution;
                }

                // 4. Bounds mask
                float bounds = step(0.0, uv.x) * step(uv.x, 1.0)
                             * step(0.0, uv.y) * step(uv.y, 1.0);

                // 5. Smidge (per-scanline horizontal jitter)
                float2 smidgeUV = snappedUV;
                if (_Smidge > 0.0)
                {
                    float j = hash(float2(floor(snappedUV.y * _PixelResolution.y),
                                          _CrtTime * 3.7)) - 0.5;
                    smidgeUV.x += j * _Smidge / _PixelResolution.x;
                }

                // 6. Chromatic Aberration — compute per-channel UV FIRST.
                //    Radial: zero at centre, grows toward corners.
                //    Suggested starting values: R=0.01, G=0.0, B=-0.01
                float2 radDir = centeredUV;
                float2 uvR = smidgeUV + radDir * _RedOffset;
                float2 uvG = smidgeUV + radDir * _GreenOffset;
                float2 uvB = smidgeUV + radDir * _BlueOffset;

                // 7. Blur — uses per-channel CA-shifted UVs so blur is NOT overwritten.
                //    _Blur is in virtual pixels (e.g. _Blur=2 → 2-pixel Gaussian radius).
                half3 color;
                if (_Blur > 0.001)
                {
                    float2 stepH = float2(_Blur / _PixelResolution.x, 0);
                    float2 stepV = float2(0, _Blur / _PixelResolution.y);
                    color.r = (GaussChannel(uvR, stepH, 0) + GaussChannel(uvR, stepV, 0)) * 0.5;
                    color.g = (GaussChannel(uvG, stepH, 1) + GaussChannel(uvG, stepV, 1)) * 0.5;
                    color.b = (GaussChannel(uvB, stepH, 2) + GaussChannel(uvB, stepV, 2)) * 0.5;
                }
                else
                {
                    color.r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvR).r;
                    color.g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvG).g;
                    color.b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvB).b;
                }

                // 8. Bleed — rightward phosphor smear, per-channel, after blur.
                //    _Bleed 0=none, 1=heavy smear. CA offsets are preserved.
                if (_Bleed > 0.001)
                {
                    color.r = BleedChannel(uvR, _Bleed, 0);
                    color.g = BleedChannel(uvG, _Bleed, 1);
                    color.b = BleedChannel(uvB, _Bleed, 2);
                }

                // 9. Scanlines
                if (_ScanlinesStrength > 0.0)
                {
                    float sl = sin(uv.y * _PixelResolution.y * PI) * _ScanlinesStrength;
                    color -= sl;
                }

                // 10. Aperture grille
                if (_ApertureStrength > 0.0)
                {
                    float ap = pow(saturate(sin(uv.x * _PixelResolution.x * PI * 0.5)), 2.0)
                               * _ApertureStrength;
                    color *= 1.0 - ap;
                }

                // 11. Shadowlines (rolling dark bar)
                if (_Shadowlines > 0.0 && _ShadowlinesAlpha > 0.0)
                {
                    float bar = frac(uv.y - _CrtTime * _ShadowlinesSpeed * 0.1);
                    bar = smoothstep(0.0, 0.05, bar) * smoothstep(0.15, 0.05, bar);
                    color *= 1.0 - bar * _ShadowlinesAlpha * _Shadowlines;
                }

                // 12. Noise
                if (_NoiseAlpha > 0.0)
                {
                    float2 nUV = floor(uv * (_PixelResolution / _NoiseSize))
                               + floor(_CrtTime * _NoiseSpeed);
                    color += (hash(nUV) * 2.0 - 1.0) * _NoiseAlpha;
                }

                // 13. Vignette
                {
                    float2 vUV = abs(centeredUV);
                    float  vLen = lerp(max(vUV.x, vUV.y), length(vUV), _VignetteRound);
                    float  vig  = smoothstep(_VignetteSize, _VignetteSize - _VignetteSmooth, vLen);
                    color *= vig;
                }

                // 14. Bounds
                color *= bounds;

                // 15. Brightness / Contrast / Gamma
                color *= _Brightness;
                color  = (color - 0.5) * _Contrast + 0.5;
                color  = pow(max(color, 0.0), 1.0 / _Gamma);

                // 16. RGB channel multipliers
                color *= half3(_Red, _Green, _Blue);

                color = saturate(color);

                // 17. Blend with original
                half4 original = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                return lerp(original, half4(color, 1.0), _Intensity);
            }
            ENDHLSL
        }
    }
}
