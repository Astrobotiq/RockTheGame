using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace CRTEffect
{
    /// <summary>
    /// Unity 6 Render Graph API kullanarak CRT filtresini iki aşamalı (blit-back) olarak işleyen sınıftır.
    /// </summary>
    public class CrtRenderPass : ScriptableRenderPass
    {
        private static readonly int ScreenBendId = Shader.PropertyToID("_ScreenBend");
        private static readonly int ScreenOverscanId = Shader.PropertyToID("_ScreenOverscan");
        private static readonly int PixelResolutionId = Shader.PropertyToID("_PixelResolution");
        private static readonly int VignetteSizeId = Shader.PropertyToID("_VignetteSize");
        private static readonly int VignetteSmoothId = Shader.PropertyToID("_VignetteSmooth");
        private static readonly int VignetteRoundId = Shader.PropertyToID("_VignetteRound");
        private static readonly int BlurId = Shader.PropertyToID("_Blur");
        private static readonly int BleedId = Shader.PropertyToID("_Bleed");
        private static readonly int SmidgeId = Shader.PropertyToID("_Smidge");
        private static readonly int ScanlinesStrengthId = Shader.PropertyToID("_ScanlinesStrength");
        private static readonly int ApertureStrengthId = Shader.PropertyToID("_ApertureStrength");
        private static readonly int ShadowlinesId = Shader.PropertyToID("_Shadowlines");
        private static readonly int ShadowlinesSpeedId = Shader.PropertyToID("_ShadowlinesSpeed");
        private static readonly int ShadowlinesAlphaId = Shader.PropertyToID("_ShadowlinesAlpha");
        private static readonly int NoiseSizeId = Shader.PropertyToID("_NoiseSize");
        private static readonly int NoiseSpeedId = Shader.PropertyToID("_NoiseSpeed");
        private static readonly int NoiseAlphaId = Shader.PropertyToID("_NoiseAlpha");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int GammaId = Shader.PropertyToID("_Gamma");
        private static readonly int RedId = Shader.PropertyToID("_Red");
        private static readonly int GreenId = Shader.PropertyToID("_Green");
        private static readonly int BlueId = Shader.PropertyToID("_Blue");
        private static readonly int RedOffsetId = Shader.PropertyToID("_RedOffset");
        private static readonly int GreenOffsetId = Shader.PropertyToID("_GreenOffset");
        private static readonly int BlueOffsetId = Shader.PropertyToID("_BlueOffset");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int TimeId = Shader.PropertyToID("_CrtTime");

        private readonly Material _filterMaterial;

        public CrtRenderPass(Material material)
        {
            _filterMaterial = material;
        }

        private class PassData
        {
            public Material material;
            public CrtFilterVolume volume;
            public TextureHandle source;
            public float time;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            var volume = VolumeManager.instance.stack.GetComponent<CrtFilterVolume>();

            if (volume == null || !volume.IsActive() || _filterMaterial == null) return;

            TextureHandle activeColor = resourceData.activeColorTexture;
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
        
            TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "CRTTempTexture", false);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Filter Pass", out var passData))
            {
                passData.material = _filterMaterial;
                passData.volume = volume;
                passData.source = activeColor;
                passData.time = Time.time;

                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0);

                builder.SetRenderFunc<PassData>((data, context) =>
                {
                    var mat = data.material;
                    var vol = data.volume;

                    mat.SetFloat(ScreenBendId, vol.screenBend.value);
                    mat.SetFloat(ScreenOverscanId, vol.screenOverscan.value);
                    mat.SetVector(PixelResolutionId, vol.pixelResolution.value);
                    mat.SetFloat(VignetteSizeId, vol.vignetteSize.value);
                    mat.SetFloat(VignetteSmoothId, vol.vignetteSmooth.value);
                    mat.SetFloat(VignetteRoundId, vol.vignetteRound.value);
                    mat.SetFloat(BlurId, vol.blur.value);
                    mat.SetFloat(BleedId, vol.bleed.value);
                    mat.SetFloat(SmidgeId, vol.smidge.value);
                    mat.SetFloat(ScanlinesStrengthId, vol.scanlinesStrength.value);
                    mat.SetFloat(ApertureStrengthId, vol.apertureStrength.value);
                    mat.SetFloat(ShadowlinesId, vol.shadowlines.value);
                    mat.SetFloat(ShadowlinesSpeedId, vol.shadowlinesSpeed.value);
                    mat.SetFloat(ShadowlinesAlphaId, vol.shadowlinesAlpha.value);
                    mat.SetFloat(NoiseSizeId, vol.noiseSize.value);
                    mat.SetFloat(NoiseSpeedId, vol.noiseSpeed.value);
                    mat.SetFloat(NoiseAlphaId, vol.noiseAlpha.value);
                    mat.SetFloat(BrightnessId, vol.brightness.value);
                    mat.SetFloat(ContrastId, vol.contrast.value);
                    mat.SetFloat(GammaId, vol.gamma.value);
                    mat.SetFloat(RedId, vol.red.value);
                    mat.SetFloat(GreenId, vol.green.value);
                    mat.SetFloat(BlueId, vol.blue.value);
                    mat.SetFloat(RedOffsetId, vol.redOffset.value);
                    mat.SetFloat(GreenOffsetId, vol.greenOffset.value);
                    mat.SetFloat(BlueOffsetId, vol.blueOffset.value);
                    mat.SetFloat(IntensityId, vol.intensity.value);
                    mat.SetFloat(TimeId, data.time);

                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), mat, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Copy Back Pass", out var passData))
            {
                passData.source = tempTexture;
                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0);

                builder.SetRenderFunc<PassData>((data, context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), 0.0f, false);
                });
            }
        }
    }
}