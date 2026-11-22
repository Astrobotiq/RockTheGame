using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçek titreşimi (docs/flower-hareket-plani.md T8). Yalnızca oyuncu çiçeğe tutunurken çalar. Süresi 0 olan profil çalmaz.
    /// <c>HapticManager.Vibrate</c> çalmakta olan tek seferlik titreşimi keser; çiçek titreşimi (kısa) başka bir titreşimin
    /// (ör. dash) üstüne denk gelirse onu böler. Değerler başlangıçtır; his kullanıcının kararı.
    /// </summary>
    [CreateAssetMenu(fileName = "FlowerVibrationSettings", menuName = "RockTheGame/Flower/Vibration Settings")]
    public class FlowerVibrationSettingsSO : ScriptableObject
    {
        [Tooltip("Tutunma anı: kısa, tok bir tık.")]
        public RumbleProfile grabRumble;
        [Tooltip("Uyarıda her sayaç tıkında (tutulurken): çok kısa ve hafif.")]
        public RumbleProfile warnTickRumble;
        [Tooltip("Çiçek tutulurken kapanırsa (snap): fırlatma darbesi.")]
        public RumbleProfile snapRumble;
    }
}
