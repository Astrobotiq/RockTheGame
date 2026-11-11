using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// Canvas'ı tam sayı sanat piksel ölçeğine sabitler: 480x270 sanat ızgarasının bir pikseli her çözünürlükte tam sayıda ekran
    /// pikseli olur (1080p'de 4, 1440p'de 5, 4K'da 8, 720p'de 2), kesirli ölçekteki dalgalı piksel genişlikleri çıkmaz.
    /// Canvas birimi sabit kalır (1 sanat pikseli = 4 canvas birimi, <c>UiArc</c>/shader'lar bunu varsayar): ölçek çarpanı
    /// <c>artScale / 4</c>. Canvas boyutu ekrana göre değişir; elemanları köşelere/kenarlara sabitle, ekranı doldurmaya çalışma.
    /// </summary>
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RockTheGame/UI/Pixel Scale")]
    public sealed class UiPixelScale : MonoBehaviour
    {
        public const float ArtWidth = 480f;
        public const float ArtHeight = 270f;
        /// <summary>1 sanat pikseli kaç canvas birimi.</summary>
        public const float UnitsPerArtPixel = 4f;

        private CanvasScaler _scaler;
        private int _lastWidth, _lastHeight;

        /// <summary>Ekrana sığan en büyük tam sayı sanat ölçeği (en az 1).</summary>
        public static int ArtScaleFor(int screenWidth, int screenHeight)
        {
            int byHeight = Mathf.FloorToInt(screenHeight / ArtHeight);
            int byWidth = Mathf.FloorToInt(screenWidth / ArtWidth);
            return Mathf.Max(1, Mathf.Min(byHeight, byWidth));
        }

        /// <summary><c>CanvasScaler.scaleFactor</c> değeri: 1 sanat pikseli = <paramref name="artScale"/> ekran pikseli, 4 canvas biriminde.</summary>
        public static float ScaleFactorFor(int artScale) => artScale / UnitsPerArtPixel;

        private void OnEnable()
        {
            _scaler = GetComponent<CanvasScaler>();
            Apply();
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight) Apply();
        }

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _scaler.scaleFactor = ScaleFactorFor(ArtScaleFor(_lastWidth, _lastHeight));
        }
    }
}
