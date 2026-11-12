using System;
using TMPro;
using UnityEngine;

namespace New_Scripts.UI.Theme
{
    /// <summary>Denge değeri ekran başına ayrı verilebilen UI ekranları.</summary>
    public enum UiScreen
    {
        MainMenu,
        Pause,
        Hud,
        RoomCard,
        MultiplayerResult,
        MultiplayerLobby
    }

    /// <summary>
    /// UI görünümünün tek veri kaynağı: palet, fontlar, çerçeve kalınlığı, gölge ofseti ve Bauhaus/brutalist dengesi.
    /// Paletin kaynağı ve gerekçesi: <c>docs/ui-research/palette.md</c>. Yüzey kuralları: <see cref="UiBalance"/>.
    /// <c>Resources/UI/UiTheme</c>'ten yüklenir (<see cref="Load"/>), <c>MultiplayerUISettingsSO</c> gibi.
    /// </summary>
    [CreateAssetMenu(menuName = "Rock The Game/UI/Theme", fileName = "UiTheme")]
    public sealed class UiThemeSO : ScriptableObject
    {
        public const string ResourcePath = "UI/UiTheme";

        [Serializable]
        public struct ScreenBalance
        {
            public UiScreen Screen;
            [Range(0f, 1f)] public float Balance;
        }

        [Header("Palet (kaynak PNG'lerden, docs/ui-research/palette.md)")]
        public Color Ink = new Color32(0x14, 0x0C, 0x1C, 255);
        public Color Paper = new Color32(0xF1, 0xE6, 0xC7, 255);
        public Color Red = new Color32(0xFC, 0x1E, 0x00, 255);
        public Color Yellow = new Color32(0xFC, 0xAE, 0x04, 255);
        public Color Blue = new Color32(0x59, 0x7D, 0xCE, 255);
        public Color BlueDark = new Color32(0x30, 0x34, 0x6D, 255);
        public Color Sky = new Color32(0x71, 0xBA, 0xEA, 255);
        [Tooltip("Boyalı iki dairenin kesişim rengi (Candy/Run.png koyu pembe-mor, kaynak pikselinden).")]
        public Color Purple = new Color32(0x97, 0x33, 0x8A, 255);
        [Tooltip("Sönük metin / ikincil gri.")]
        public Color Muted = new Color32(0x4E, 0x4A, 0x4E, 255);

        [Header("Beton tonları (mockup'tan, el ile seçilmiş)")]
        public Color ConcreteWall = new Color32(0xC9, 0xC6, 0xBF, 255);
        public Color ConcreteSlab = new Color32(0xE2, 0xDF, 0xD8, 255);
        public Color ConcreteMid = new Color32(0x9A, 0x97, 0x91, 255);

        [Header("Fontlar (Jost 800 başlık, Space Mono gövde/etiket; ikisi de OFL, bkz. Assets/UI/Fonts/Licenses)")]
        public TMP_FontAsset TitleFont;
        public TMP_FontAsset MonoFont;
        [Tooltip("Mockup'ta düğme ve etiketler mono 700 kullanıyor.")]
        public TMP_FontAsset MonoBoldFont;

        [Header("Beton materyali (RockTheGame/UI/Concrete; build'e girsin diye burada tutulur)")]
        public Material ConcreteMaterial;
        [Tooltip("Pause karartması (RockTheGame/UI/Dither); build'e girsin diye burada tutulur.")]
        public Material DitherMaterial;
        [Tooltip("Tehlike şeridi (RockTheGame/UI/Hazard); build'e girsin diye burada tutulur.")]
        public Material HazardMaterial;

        [Header("Yüzey (sanat pikseli, 480x270 ızgara)")]
        [Min(0f)] public float FrameThickness = 3f;
        [Min(0f)] public float ShadowOffset = 4f;
        [Min(0f)] public float ExtrusionDepth = 8f;

        [Header("Denge: 0 = Bauhaus, 1 = brutalist")]
        [Range(0f, 1f)] public float Balance = 0.55f;
        [Tooltip("Ekran başına değer; listede olmayan ekran genel Balance'ı kullanır.")]
        public ScreenBalance[] ScreenOverrides = Array.Empty<ScreenBalance>();

        public static UiThemeSO Load()
        {
            var theme = Resources.Load<UiThemeSO>(ResourcePath);
            return theme != null ? theme : CreateInstance<UiThemeSO>();
        }

        /// <summary>Ekrana özel denge; yoksa genel <see cref="Balance"/>.</summary>
        public float BalanceFor(UiScreen screen)
        {
            if (ScreenOverrides != null)
            {
                for (int i = 0; i < ScreenOverrides.Length; i++)
                {
                    if (ScreenOverrides[i].Screen == screen) return UiBalance.Clamp(ScreenOverrides[i].Balance);
                }
            }

            return UiBalance.Clamp(Balance);
        }

        public float BorderWidthFor(UiScreen screen) => UiBalance.BorderWidth(FrameThickness, BalanceFor(screen));
        public int ShadowOffsetFor(UiScreen screen) => UiBalance.ShadowOffset(ShadowOffset, BalanceFor(screen));
        public int ExtrusionDepthFor(UiScreen screen) => UiBalance.ExtrusionDepth(ExtrusionDepth, BalanceFor(screen));

        /// <summary>Beton yüzeyin zemin rengi: denge düşükken kâğıt tonu, yükseldikçe beton (levha).</summary>
        public Color ConcreteSurfaceFor(UiScreen screen) =>
            Color.Lerp(Paper, ConcreteSlab, UiBalance.ConcreteBlend(BalanceFor(screen)));
    }
}
