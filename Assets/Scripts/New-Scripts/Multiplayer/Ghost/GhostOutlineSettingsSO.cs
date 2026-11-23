using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Rakibin platform ve tehlike konturunun görünümü ve zamanlaması (docs/multiplayer-ceza-kapi-ve-platform-plani.md → Sorun 2).
    /// Resources/Multiplayer/GhostOutlineSettings'ten yüklenir; yoksa varsayılanlarla çalışır ama materyal olmadığı için çizmez.
    /// Çizgi kalınlığı ve kesik uzunlukları ekran pikseli: kamera yaklaşıp uzaklaşınca dünya boyutu değişir, görünüm sabit kalır.
    /// </summary>
    [CreateAssetMenu(menuName = "Rock The Game/Multiplayer/Ghost Outline Settings", fileName = "GhostOutlineSettings")]
    public sealed class GhostOutlineSettingsSO : ScriptableObject
    {
        public const string ResourcePath = "Multiplayer/GhostOutlineSettings";

        [Header("Görünüm")]
        [Tooltip("Kesik çizgi materyali (Shader: RockTheGame/GhostOutline). Boşsa kontur çizilmez.")]
        public Material dashMaterial;
        [Tooltip("Kâğıt beyazı #FFF6E0.")]
        public Color color = new Color(1f, 0.965f, 0.878f, 1f);
        [Tooltip("Tam görünürkenki en yüksek saydamlık.")]
        [Range(0f, 1f)] public float maxAlpha = 1f;
        [Tooltip("Çizgi kalınlığı, ekran pikseli.")]
        public float widthPixels = 5f;
        [Tooltip("Kesik çizginin dolu kısmı, ekran pikseli (mockup A: 14).")]
        public float dashPixels = 14f;
        [Tooltip("Kesik çizginin boş kısmı, ekran pikseli (mockup A: 9).")]
        public float gapPixels = 9f;

        [Header("Platform ve node")]
        [Tooltip("Rakip yüzeye basınca konturun tam görünmesi için süre (sn).")]
        public float enterSeconds = 0.1f;
        [Tooltip("Rakip ayrılınca kontur tam görünür kalır (sn).")]
        public float lingerSeconds = 0.4f;
        [Tooltip("Bekleme bittikten sonra solma süresi (sn).")]
        public float fadeSeconds = 0.2f;
        [Tooltip("Rakibin dünyasındaki poz yerel nesneyle bundan yakınsa kontur çizilmez (dünya birimi).")]
        public float minLocalDistance = 0.25f;

        [Header("Öldüren tehlike")]
        [Tooltip("Ölüm anındaki pozunda görünme süresi, solma dahil (sn).")]
        public float hazardSeconds = 0.6f;
        [Tooltip("Bu sürenin son kaçı solma (sn).")]
        public float hazardFadeSeconds = 0.2f;

        public GhostOutlineTiming Timing => new GhostOutlineTiming(enterSeconds, lingerSeconds, fadeSeconds, hazardSeconds, hazardFadeSeconds);

        public static GhostOutlineSettingsSO Load()
        {
            var settings = Resources.Load<GhostOutlineSettingsSO>(ResourcePath);
            return settings != null ? settings : CreateInstance<GhostOutlineSettingsSO>();
        }
    }
}
