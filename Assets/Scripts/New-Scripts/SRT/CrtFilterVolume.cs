using UnityEngine;
using UnityEngine.Rendering;

namespace New_Scripts.SRT
{
    /// <summary>
    /// CRT filtre efektinin görsel parametrelerini (eğrilik, kararma, renk sapması) tutan Volume nesnesidir.
    /// </summary>
    [System.Serializable, UnityEngine.Rendering.VolumeComponentMenu("Custom/CRT Filter")]
    public class CrtFilterVolume : UnityEngine.Rendering.VolumeComponent, UnityEngine.Rendering.IPostProcessComponent
    {
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter curvature = new ClampedFloatParameter(0f, 0f, 10f);
        public ClampedFloatParameter vignette = new ClampedFloatParameter(0f, 0f, 2f);
        public ClampedFloatParameter rgbSplit = new ClampedFloatParameter(0f, 0f, 0.05f);
        public Vector2Parameter pixelResolution = new Vector2Parameter(new Vector2(320f, 240f));

        public bool IsActive() => intensity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}