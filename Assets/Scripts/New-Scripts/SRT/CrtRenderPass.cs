using New_Scripts.SRT;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Unity 6 Render Graph API kullanarak gelişmiş CRT filtresini güvenli bir şekilde işleyen sınıftır.
/// </summary>
public class CrtRenderPass : ScriptableRenderPass
{
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int CurvatureId = Shader.PropertyToID("_Curvature");
    private static readonly int VignetteId = Shader.PropertyToID("_Vignette");
    private static readonly int RgbSplitId = Shader.PropertyToID("_RgbSplit");
    private static readonly int PixelResolutionId = Shader.PropertyToID("_PixelResolution"); // EKLENDİ

    private Material filterMaterial;

    public CrtRenderPass(Material material)
    {
        filterMaterial = material;
    }

    private class PassData
    {
        public Material material;
        public float intensity;
        public float curvature;
        public float vignette;
        public float rgbSplit;
        public Vector2 pixelResolution; // EKLENDİ
        public TextureHandle source;
    }

    private class CopyPassData
    {
        public TextureHandle tempSource;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        
        var stack = VolumeManager.instance.stack;
        var filterVolume = stack.GetComponent<CrtFilterVolume>();

        if (filterVolume == null || !filterVolume.IsActive() || filterMaterial == null) return;

        TextureHandle activeColor = resourceData.activeColorTexture;

        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0; 
        TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "CRTTempTexture", false);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("CRT Filter Pass", out var passData))
        {
            passData.material = filterMaterial;
            passData.intensity = filterVolume.intensity.value;
            passData.curvature = filterVolume.curvature.value;
            passData.vignette = filterVolume.vignette.value;
            passData.rgbSplit = filterVolume.rgbSplit.value;
            passData.pixelResolution = filterVolume.pixelResolution.value; // EKLENDİ
            passData.source = activeColor;

            builder.UseTexture(passData.source, AccessFlags.Read);
            builder.SetRenderAttachment(tempTexture, 0);

            builder.SetRenderFunc<PassData>((data, context) => 
            {
                data.material.SetFloat(IntensityId, data.intensity);
                data.material.SetFloat(CurvatureId, data.curvature);
                data.material.SetFloat(VignetteId, data.vignette);
                data.material.SetFloat(RgbSplitId, data.rgbSplit);
                data.material.SetVector(PixelResolutionId, data.pixelResolution); // GPU'YA GÖNDERİLDİ
                
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("CRT Copy Back Pass", out var copyData))
        {
            copyData.tempSource = tempTexture;

            builder.UseTexture(copyData.tempSource, AccessFlags.Read);
            builder.SetRenderAttachment(activeColor, 0);

            builder.SetRenderFunc<CopyPassData>((data, context) => 
            {
                Blitter.BlitTexture(context.cmd, data.tempSource, new Vector4(1, 1, 0, 0), 0.0f, false);
            });
        }
    }
}