using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Render Graph uyumlu CRT Pass'ini render döngüsüne enjekte eden Renderer Feature sınıfıdır.
/// Inspector'dan Injection Point seçilebilir.
/// </summary>
public class CrtRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material crtMaterial;

    [Tooltip("CRT efektinin render pipeline'a hangi noktada enjekte edileceğini belirler.")]
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

    private CrtRenderPass crtRenderPass;

    public override void Create()
    {
        crtRenderPass = new CrtRenderPass(crtMaterial)
        {
            renderPassEvent = injectionPoint
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (crtMaterial != null)
        {
            // Injection point değiştirilmişse anında uygula (Play Mode'da da çalışır)
            crtRenderPass.renderPassEvent = injectionPoint;
            renderer.EnqueuePass(crtRenderPass);
        }
    }
}