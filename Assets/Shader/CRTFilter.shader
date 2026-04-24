// Hidden/Custom/CRTFilter
// Tüm CRT parametrelerini destekleyen URP Render Graph uyumlu shader.
Shader "Hidden/Custom/CRTFilter"
{
    Properties
    {
        // Screen Geometry
        _ScreenBend         ("Screen Bend",       Float) = 0.0
        _ScreenOverscan     ("Screen Overscan",   Float) = 0.0
        _PixelResolution    ("Pixel Resolution",  Vector) = (320, 240, 0, 0)

        // Vignette
        _VignetteSize       ("Vignette Size",     Float) = 0.5
        _VignetteSmooth     ("Vignette Smooth",   Float) = 0.4
        _VignetteRound      ("Vignette Round",    Float) = 1.0

        // Blur / Bleed
        _Blur               ("Blur",              Float) = 0.0
        _Bleed              ("Bleed",             Float) = 0.0
        _Smidge             ("Smidge",            Float) = 0.0

        // Scanlines & Noise
        _ScanlinesStrength  ("Scanlines Strength",Float) = 0.04
        _ApertureStrength   ("Aperture Strength", Float) = 0.0
        _Shadowlines        ("Shadowlines",       Float) = 0.0
        _ShadowlinesSpeed   ("Shadowlines Speed", Float) = 1.0
        _ShadowlinesAlpha   ("Shadowlines Alpha", Float) = 0.3
        _NoiseSize          ("Noise Size",        Float) = 2.0
        _NoiseSpeed         ("Noise Speed",       Float) = 10.0
        _NoiseAlpha         ("Noise Alpha",       Float) = 0.0

        // Image Adjustments
        _Brightness         ("Brightness",        Float) = 1.0
        _Contrast           ("Contrast",          Float) = 1.0
        _Gamma              ("Gamma",             Float) = 1.0
        _Red                ("Red",               Float) = 1.0
        _Green              ("Green",             Float) = 1.0
        _Blue               ("Blue",              Float) = 1.0

        // Chromatic Aberration
        _RedOffset          ("Red Offset",        Float) = 0.0
        _GreenOffset        ("Green Offset",      Float) = 0.0
        _BlueOffset         ("Blue Offset",       Float) = 0.0

        // Global
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

            // ── Uniforms ──────────────────────────────────────────────────────
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

            // ── Helpers ───────────────────────────────────────────────────────

            // Hash noise (no texture needed)
            float hash(float2 p)
            {
                p = frac(p * float2(443.8975, 397.2973));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            // Separable Gaussian blur — dir is already in UV space (texel size * spread)
            half3 BlurSample(float2 uv, float2 step)
            {
                half3 col   = 0;
                float total = 0;
                // 7-tap kernel, sigma ≈ 1.5
                const float weights[7] = { 0.0625, 0.125, 0.1875, 0.25, 0.1875, 0.125, 0.0625 };
                UNITY_UNROLL
                for (int i = 0; i < 7; ++i)
                {
                    float w = weights[i];
                    col   += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                 uv + step * (i - 3)).rgb * w;
                    total += w;
                }
                return col / total;
            }

            // Bleed: rightward luminance smear simulating phosphor persistence.
            // Samples are taken only to the RIGHT (past-pixel direction on a
            // left-to-right scan) and accumulate with exponential falloff.
            half3 BleedSample(float2 uv, float amount)
            {
                // Step in screen-space pixels; bleed affects the pixel grid
                float pixW = 1.0 / _PixelResolution.x;
                half3  acc = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                float  w   = 1.0;
                float  wSum = 1.0;
                UNITY_UNROLL
                for (int i = 1; i <= 6; ++i)
                {
                    w   *= (1.0 - amount * 0.35);          // exponential decay
                    half3 tap = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                    uv + float2(pixW * i, 0)).rgb;
                    acc  += tap * w;
                    wSum += w;
                }
                return acc / wSum;
            }

            // ── Fragment ──────────────────────────────────────────────────────
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // ── 1. Screen Overscan ────────────────────────────────────────
                float2 uv = input.texcoord;
                uv = (uv - 0.5) * (1.0 + _ScreenOverscan) + 0.5;

                // ── 2. Screen Bend (curvature) ────────────────────────────────
                float2 centeredUV = uv * 2.0 - 1.0;
                if (_ScreenBend > 0.001)
                {
                    float2 bendOffset = centeredUV.yx / _ScreenBend;
                    uv = uv + centeredUV * bendOffset * bendOffset;
                    centeredUV = uv * 2.0 - 1.0;
                }

                // ── 3. Pixel Snap (pixelation) ────────────────────────────────
                float2 snappedUV = uv;
                if (_PixelResolution.x > 0 && _PixelResolution.y > 0)
                {
                    snappedUV = floor(uv * _PixelResolution) / _PixelResolution;
                    snappedUV += 0.5 / _PixelResolution;
                }

                // ── 4. Out-of-bounds mask ─────────────────────────────────────
                float bounds = step(0.0, uv.x) * step(uv.x, 1.0)
                             * step(0.0, uv.y) * step(uv.y, 1.0);

                // ── 5. Smidge (sub-pixel jitter) ──────────────────────────────
                float2 smidgeUV = snappedUV;
                if (_Smidge > 0.0)
                {
                    float smidgeH = hash(float2(floor(snappedUV.y * _PixelResolution.y),
                                                _CrtTime * 3.7)) - 0.5;
                    smidgeUV.x += smidgeH * _Smidge / _PixelResolution.x;
                }

                // ── 6. Blur ───────────────────────────────────────────────────
                // _Blur is in screen pixels (0 = off, 4 = heavy).
                // We convert to UV space using the actual render target size
                // (approximated via PixelResolution for consistent feel).
                half3 color;
                if (_Blur > 0.001)
                {
                    // spread: _Blur pixels worth of UV distance
                    float2 texelH = float2(_Blur / _PixelResolution.x, 0);
                    float2 texelV = float2(0, _Blur / _PixelResolution.y);
                    half3 hBlur = BlurSample(smidgeUV, texelH);
                    half3 vBlur = BlurSample(smidgeUV, texelV);
                    color = (hBlur + vBlur) * 0.5;
                }
                else
                {
                    color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, smidgeUV).rgb;
                }

                // ── 7. Bleed ──────────────────────────────────────────────────
                // Additive rightward smear; amount 0-1 controls decay rate.
                if (_Bleed > 0.001)
                    color = lerp(color, BleedSample(smidgeUV, _Bleed), saturate(_Bleed));

                // ── 8. Chromatic Aberration ───────────────────────────────────
                // Each channel is pushed radially away from screen centre.
                // The offset magnitude grows with distance from centre (barrel-like),
                // so the effect is zero at the middle and strongest at corners.
                // _RedOffset / _GreenOffset / _BlueOffset are scale factors (try 0.005–0.02).
                {
                    // Direction from centre for this pixel (normalised)
                    float2 radDir = centeredUV; // already in [-1,1]

                    float cR = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                   smidgeUV + radDir * _RedOffset).r;
                    float cG = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                   smidgeUV + radDir * _GreenOffset).g;
                    float cB = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                   smidgeUV + radDir * _BlueOffset).b;
                    color = half3(cR, cG, cB);
                }

                // ── 9. Scanlines ──────────────────────────────────────────────
                if (_ScanlinesStrength > 0.0)
                {
                    float scanline = sin(uv.y * _PixelResolution.y * PI) * _ScanlinesStrength;
                    color -= scanline;
                }

                // ── 10. Aperture Grille ───────────────────────────────────────
                if (_ApertureStrength > 0.0)
                {
                    float ap = sin(uv.x * _PixelResolution.x * PI * 0.5);
                    ap = pow(saturate(ap), 2.0) * _ApertureStrength;
                    color *= 1.0 - ap;
                }

                // ── 11. Shadowlines (rolling bar) ─────────────────────────────
                if (_Shadowlines > 0.0 && _ShadowlinesAlpha > 0.0)
                {
                    float bar = frac(uv.y - _CrtTime * _ShadowlinesSpeed * 0.1);
                    bar = smoothstep(0.0, 0.05, bar) * smoothstep(0.15, 0.05, bar);
                    color *= 1.0 - bar * _ShadowlinesAlpha * _Shadowlines;
                }

                // ── 12. Noise ─────────────────────────────────────────────────
                if (_NoiseAlpha > 0.0)
                {
                    float2 noiseUV = floor(uv * (_PixelResolution / _NoiseSize))
                                   + floor(_CrtTime * _NoiseSpeed);
                    float n = hash(noiseUV) * 2.0 - 1.0;
                    color += n * _NoiseAlpha;
                }

                // ── 13. Vignette ──────────────────────────────────────────────
                {
                    float2 vUV = abs(centeredUV);
                    // VignetteRound interpolates between square (0) and round (1)
                    float  vLen = lerp(max(vUV.x, vUV.y),
                                       length(vUV), _VignetteRound);
                    float  vig  = smoothstep(_VignetteSize,
                                             _VignetteSize - _VignetteSmooth, vLen);
                    color *= vig;
                }

                // ── 14. Bounds clip ───────────────────────────────────────────
                color *= bounds;

                // ── 15. Brightness / Contrast / Gamma ────────────────────────
                color *= _Brightness;
                color  = (color - 0.5) * _Contrast + 0.5;
                color  = pow(max(color, 0.0), 1.0 / _Gamma);

                // ── 16. RGB channel multipliers ───────────────────────────────
                color *= half3(_Red, _Green, _Blue);

                color = saturate(color);

                // ── 17. Blend with original ───────────────────────────────────
                half4 original = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                                                  input.texcoord);
                return lerp(original, half4(color, 1.0), _Intensity);
            }
            ENDHLSL
        }
    }
}
