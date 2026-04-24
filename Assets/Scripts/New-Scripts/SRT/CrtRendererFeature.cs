/// <summary>
/// Render Graph uyumlu CRT Pass'ini render döngüsüne enjekte eden Renderer Feature sınıfıdır.
/// </summary>
public class CrtRendererFeature : UnityEngine.Rendering.Universal.ScriptableRendererFeature
{
    [UnityEngine.SerializeField] private UnityEngine.Material crtMaterial;
    private CrtRenderPass crtRenderPass;

    public override void Create()
    {
        crtRenderPass = new CrtRenderPass(crtMaterial);
        crtRenderPass.renderPassEvent = UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
    {
        if (crtMaterial != null)
        {
            renderer.EnqueuePass(crtRenderPass);
        }
    }
}