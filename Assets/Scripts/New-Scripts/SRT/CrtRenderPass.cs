using New_Scripts.SRT;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Unity 6 Render Graph API kullanarak gelişmiş CRT filtresini güvenli bir şekilde (2-Pass) işleyen sınıftır.
/// </summary>
public class CrtRenderPass : ScriptableRenderPass
{
    // ─── Screen Geometry ─────────────────────────────────────────────────────
    private static readonly int ScreenBendId      = Shader.PropertyToID("_ScreenBend");
    private static readonly int ScreenOverscanId  = Shader.PropertyToID("_ScreenOverscan");
    private static readonly int PixelResolutionId = Shader.PropertyToID("_PixelResolution");

    // ─── Vignette ─────────────────────────────────────────────────────────────
    private static readonly int VignetteSizeId    = Shader.PropertyToID("_VignetteSize");
    private static readonly int VignetteSmoothId  = Shader.PropertyToID("_VignetteSmooth");
    private static readonly int VignetteRoundId   = Shader.PropertyToID("_VignetteRound");

    // ─── Blur / Bleed ─────────────────────────────────────────────────────────
    private static readonly int BlurId            = Shader.PropertyToID("_Blur");
    private static readonly int BleedId           = Shader.PropertyToID("_Bleed");
    private static readonly int SmidgeId          = Shader.PropertyToID("_Smidge");

    // ─── Scanlines & Noise ────────────────────────────────────────────────────
    private static readonly int ScanlinesStrengthId = Shader.PropertyToID("_ScanlinesStrength");
    private static readonly int ApertureStrengthId  = Shader.PropertyToID("_ApertureStrength");
    private static readonly int ShadowlinesId       = Shader.PropertyToID("_Shadowlines");
    private static readonly int ShadowlinesSpeedId  = Shader.PropertyToID("_ShadowlinesSpeed");
    private static readonly int ShadowlinesAlphaId  = Shader.PropertyToID("_ShadowlinesAlpha");
    private static readonly int NoiseSizeId         = Shader.PropertyToID("_NoiseSize");
    private static readonly int NoiseSpeedId        = Shader.PropertyToID("_NoiseSpeed");
    private static readonly int NoiseAlphaId        = Shader.PropertyToID("_NoiseAlpha");

    // ─── Image Adjustments ────────────────────────────────────────────────────
    private static readonly int BrightnessId  = Shader.PropertyToID("_Brightness");
    private static readonly int ContrastId    = Shader.PropertyToID("_Contrast");
    private static readonly int GammaId       = Shader.PropertyToID("_Gamma");
    private static readonly int RedId         = Shader.PropertyToID("_Red");
    private static readonly int GreenId       = Shader.PropertyToID("_Green");
    private static readonly int BlueId        = Shader.PropertyToID("_Blue");

    // ─── Chromatic Aberration ─────────────────────────────────────────────────
    private static readonly int RedOffsetId   = Shader.PropertyToID("_RedOffset");
    private static readonly int GreenOffsetId = Shader.PropertyToID("_GreenOffset");
    private static readonly int BlueOffsetId  = Shader.PropertyToID("_BlueOffset");

    // ─── Global ───────────────────────────────────────────────────────────────
    private static readonly int IntensityId   = Shader.PropertyToID("_Intensity");
    private static readonly int TimeId        = Shader.PropertyToID("_CrtTime");

    private Material filterMaterial;

    public CrtRenderPass(Material material)
    {
        filterMaterial = material;
    }

    private class PassData
    {
        public Material material;

        // Screen Geometry
        public float   screenBend;
        public float   screenOverscan;
        public Vector2 pixelResolution;

        // Vignette
        public float vignetteSize;
        public float vignetteSmooth;
        public float vignetteRound;

        // Blur / Bleed
        public float blur;
        public float bleed;
        public float smidge;

        // Scanlines & Noise
        public float scanlinesStrength;
        public float apertureStrength;
        public float shadowlines;
        public float shadowlinesSpeed;
        public float shadowlinesAlpha;
        public float noiseSize;
        public float noiseSpeed;
        public float noiseAlpha;

        // Image Adjustments
        public float brightness;
        public float contrast;
        public float gamma;
        public float red;
        public float green;
        public float blue;

        // Chromatic Aberration
        public float redOffset;
        public float greenOffset;
        public float blueOffset;

        // Global
        public float intensity;
        public float time;

        public TextureHandle source;
    }

    private class CopyPassData
    {
        public TextureHandle tempSource;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData   cameraData   = frameData.Get<UniversalCameraData>();

        var stack        = VolumeManager.instance.stack;
        var filterVolume = stack.GetComponent<CrtFilterVolume>();

        if (filterVolume == null || !filterVolume.IsActive() || filterMaterial == null) return;

        TextureHandle activeColor = resourceData.activeColorTexture;

        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "CRTTempTexture", false);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Filter Pass", out var passData))
        {
            // ── Fill PassData ──────────────────────────────────────────────
            passData.material = filterMaterial;

            passData.screenBend      = filterVolume.screenBend.value;
            passData.screenOverscan  = filterVolume.screenOverscan.value;
            passData.pixelResolution = filterVolume.pixelResolution.value;

            passData.vignetteSize    = filterVolume.vignetteSize.value;
            passData.vignetteSmooth  = filterVolume.vignetteSmooth.value;
            passData.vignetteRound   = filterVolume.vignetteRound.value;

            passData.blur   = filterVolume.blur.value;
            passData.bleed  = filterVolume.bleed.value;
            passData.smidge = filterVolume.smidge.value;

            passData.scanlinesStrength = filterVolume.scanlinesStrength.value;
            passData.apertureStrength  = filterVolume.apertureStrength.value;
            passData.shadowlines       = filterVolume.shadowlines.value;
            passData.shadowlinesSpeed  = filterVolume.shadowlinesSpeed.value;
            passData.shadowlinesAlpha  = filterVolume.shadowlinesAlpha.value;
            passData.noiseSize         = filterVolume.noiseSize.value;
            passData.noiseSpeed        = filterVolume.noiseSpeed.value;
            passData.noiseAlpha        = filterVolume.noiseAlpha.value;

            passData.brightness = filterVolume.brightness.value;
            passData.contrast   = filterVolume.contrast.value;
            passData.gamma      = filterVolume.gamma.value;
            passData.red        = filterVolume.red.value;
            passData.green      = filterVolume.green.value;
            passData.blue       = filterVolume.blue.value;

            passData.redOffset   = filterVolume.redOffset.value;
            passData.greenOffset = filterVolume.greenOffset.value;
            passData.blueOffset  = filterVolume.blueOffset.value;

            passData.intensity = filterVolume.intensity.value;
            passData.time      = Time.time;
            passData.source    = activeColor;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(tempTexture, 0);

            builder.SetRenderFunc<PassData>((data, context) =>
            {
                var mat = data.material;

                mat.SetFloat(ScreenBendId,      data.screenBend);
                mat.SetFloat(ScreenOverscanId,  data.screenOverscan);
                mat.SetVector(PixelResolutionId, data.pixelResolution);

                mat.SetFloat(VignetteSizeId,   data.vignetteSize);
                mat.SetFloat(VignetteSmoothId, data.vignetteSmooth);
                mat.SetFloat(VignetteRoundId,  data.vignetteRound);

                mat.SetFloat(BlurId,   data.blur);
                mat.SetFloat(BleedId,  data.bleed);
                mat.SetFloat(SmidgeId, data.smidge);

                mat.SetFloat(ScanlinesStrengthId, data.scanlinesStrength);
                mat.SetFloat(ApertureStrengthId,  data.apertureStrength);
                mat.SetFloat(ShadowlinesId,       data.shadowlines);
                mat.SetFloat(ShadowlinesSpeedId,  data.shadowlinesSpeed);
                mat.SetFloat(ShadowlinesAlphaId,  data.shadowlinesAlpha);
                mat.SetFloat(NoiseSizeId,         data.noiseSize);
                mat.SetFloat(NoiseSpeedId,        data.noiseSpeed);
                mat.SetFloat(NoiseAlphaId,        data.noiseAlpha);

                mat.SetFloat(BrightnessId, data.brightness);
                mat.SetFloat(ContrastId,   data.contrast);
                mat.SetFloat(GammaId,      data.gamma);
                mat.SetFloat(RedId,        data.red);
                mat.SetFloat(GreenId,      data.green);
                mat.SetFloat(BlueId,       data.blue);

                mat.SetFloat(RedOffsetId,   data.redOffset);
                mat.SetFloat(GreenOffsetId, data.greenOffset);
                mat.SetFloat(BlueOffsetId,  data.blueOffset);

                mat.SetFloat(IntensityId, data.intensity);
                mat.SetFloat(TimeId,      data.time);

                Blitter.BlitTexture(context.cmd, data.source,
                    new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("CRT Copy Back Pass", out var copyData))
        {
            copyData.tempSource = tempTexture;

            builder.UseTexture(copyData.tempSource, AccessFlags.Read);
            builder.SetRenderAttachment(activeColor, 0);

            builder.SetRenderFunc<CopyPassData>((data, context) =>
            {
                Blitter.BlitTexture(context.cmd, data.tempSource,
                    new Vector4(1, 1, 0, 0), 0.0f, false);
            });
        }
    }
}