using UnityEngine;
using UnityEngine.Rendering;

namespace CRTEffect
{
    /// <summary>
    /// CRT filtre efektinin tüm görsel parametrelerini tutan Volume nesnesidir.
    /// </summary>
    [System.Serializable, VolumeComponentMenu("Custom/CRT Filter")]
    public class CrtFilterVolume : VolumeComponent, IPostProcessComponent
    {
        [Header("Screen Geometry")]
        public ClampedFloatParameter screenBend = new ClampedFloatParameter(0f, 0f, 10f);
        public ClampedFloatParameter screenOverscan = new ClampedFloatParameter(0f, 0f, 0.2f);
        public Vector2Parameter pixelResolution = new Vector2Parameter(new Vector2(320f, 240f));
        
        [Header("Vignette")]
        public ClampedFloatParameter vignetteSize = new ClampedFloatParameter(0.5f, 0f, 2f);
        public ClampedFloatParameter vignetteSmooth = new ClampedFloatParameter(0.4f, 0f, 1f);
        public ClampedFloatParameter vignetteRound = new ClampedFloatParameter(1f, 0f, 1f);
        
        [Header("Blur Effects")]
        public ClampedFloatParameter blur = new ClampedFloatParameter(0f, 0f, 4f);
        public ClampedFloatParameter bleed = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter smidge = new ClampedFloatParameter(0f, 0f, 1f);
        
        [Header("Scanlines and Noise")]
        public ClampedFloatParameter scanlinesStrength = new ClampedFloatParameter(0.04f, 0f, 1f);
        public ClampedFloatParameter apertureStrength = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter shadowlines = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter shadowlinesSpeed = new ClampedFloatParameter(1f, 0f, 5f);
        public ClampedFloatParameter shadowlinesAlpha = new ClampedFloatParameter(0.3f, 0f, 1f);
        public ClampedFloatParameter noiseSize = new ClampedFloatParameter(2f, 1f, 8f);
        public ClampedFloatParameter noiseSpeed = new ClampedFloatParameter(10f, 0f, 60f);
        public ClampedFloatParameter noiseAlpha = new ClampedFloatParameter(0f, 0f, 1f);
        
        [Header("Image Adjustments")]
        public ClampedFloatParameter brightness = new ClampedFloatParameter(1f, 0f, 3f);
        public ClampedFloatParameter contrast = new ClampedFloatParameter(1f, 0f, 3f);
        public ClampedFloatParameter gamma = new ClampedFloatParameter(1f, 0.1f, 3f);
        public ClampedFloatParameter red = new ClampedFloatParameter(1f, 0f, 2f);
        public ClampedFloatParameter green = new ClampedFloatParameter(1f, 0f, 2f);
        public ClampedFloatParameter blue = new ClampedFloatParameter(1f, 0f, 2f);
        
        [Header("Chromatic Aberration")]
        public ClampedFloatParameter redOffset = new ClampedFloatParameter(0f, -0.05f, 0.05f);
        public ClampedFloatParameter greenOffset = new ClampedFloatParameter(0f, -0.05f, 0.05f);
        public ClampedFloatParameter blueOffset = new ClampedFloatParameter(0f, -0.05f, 0.05f);
        
        [Header("Global")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public bool IsActive() => intensity.value > 0f && active;
        public bool IsTileCompatible() => false;
    }
}