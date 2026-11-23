using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Balon cezasının süreleri, mesafeleri ve salınımı (docs/multiplayer-ceza-balonu-plani.md → Dizi tablosu).
    /// Resources/Multiplayer/PenaltyBubbleSettings'ten yüklenir; yoksa varsayılanlarla çalışır.
    /// Değerler başlangıçtır; hissi insan ayarlar (BB7).
    /// </summary>
    [CreateAssetMenu(menuName = "Rock The Game/Multiplayer/Penalty Bubble Settings", fileName = "PenaltyBubbleSettings")]
    public sealed class PenaltyBubbleSettingsSO : ScriptableObject
    {
        public const string ResourcePath = "Multiplayer/PenaltyBubbleSettings";

        [Header("1. Kalkış")]
        [Tooltip("Oyuncunun havaya yükselme süresi (sn).")]
        public float liftSeconds = 0.35f;
        [Tooltip("Yükselme miktarı (dünya birimi).")]
        public float liftHeight = 1.5f;
        [Tooltip("Uçma pozunda görselin gidiş yönüne yatma açısı (derece).")]
        public float poseAngleDegrees = 70f;

        [Header("2. Balon")]
        [Tooltip("Balonun yarıçapı (dünya birimi); oyuncuyu sarar.")]
        public float bubbleRadius = 1.4f;
        [Tooltip("Balon materyali (Shader: Sprites/PenaltyBubble). Boşsa düz bir daire çizilir. Yerel ve rakip balonu kullanır.")]
        public Material bubbleMaterial;
        [Tooltip("Balonun hız yönünde en çok ne kadar uzadığı (0,15 = %15; dik yönde yarısı kadar kısalır).")]
        public float maxStretch = 0.15f;
        [Tooltip("Esnemenin tavana vardığı hız (birim/sn).")]
        public float stretchReferenceSpeed = 60f;
        [Tooltip("Balonun 0'dan şişme süresi (sn); kalkışla üst üste biner.")]
        public float inflateSeconds = 0.25f;
        [Tooltip("Şişerken ulaşılan en büyük ölçek; sonra 1'e oturur.")]
        public float inflateOvershoot = 1.15f;

        [Header("3. Yolculuk")]
        [Tooltip("Seyir hızı (birim/sn). Süre = yol / hız, aşağıdaki sınırlarla kırpılır.")]
        public float cruiseSpeed = 25f;
        public float minTravelSeconds = 2.5f;
        public float maxTravelSeconds = 5f;
        [Tooltip("Balon iki oda arasındaki kapıya varınca kameranın yeni odaya kayma süresi (sn); balon o sürede kapıda bekler. Normal oda geçişiyle aynı.")]
        public float doorPanSeconds = 0.5f;
        [Tooltip("Yola dik salınımın genliği (dünya birimi).")]
        public float wobbleAmplitude = 0.25f;
        [Tooltip("Salınım frekansı (Hz).")]
        public float wobbleHz = 1.2f;

        [Header("4. Varış")]
        [Tooltip("Balonun checkpoint'in ne kadar üstünde durduğu (dünya birimi).")]
        public float arriveHeight = 1.5f;
        [Tooltip("Varıp patlamadan önce kısa bekleme (sn).")]
        public float arriveHoldSeconds = 0.3f;

        [Header("Ses ve titreşim (BB5)")]
        [Tooltip("SFX-Play kanalı (Audio/SO/Play-StopEvents/SFX-Play).")]
        public AudioCuePlayEventChannelSO sfxPlayChannel;
        [Tooltip("SFX-Stop kanalı; yolculuk döngüsünü durdurur.")]
        public AudioCueStopEventChannelSO sfxStopChannel;
        [Tooltip("Şişme sesi (bloop). 2D.")]
        public AudioCueSO inflateCue;
        [Tooltip("Yolculuk uğultusu; döngü, balon varana kadar sürer. 2D.")]
        public AudioCueSO travelCue;
        [Tooltip("Patlama sesi (pop). 2D.")]
        public AudioCueSO popCue;
        [Tooltip("Şişme ve patlama titreşimi.")]
        public PenaltyVibrationSettingsSO vibration;

        /// <summary>Verilen yol uzunluğu için yolculuk süresi (sn).</summary>
        public float TravelSeconds(float pathLength) =>
            PenaltyBubbleMath.Duration(pathLength, cruiseSpeed, minTravelSeconds, maxTravelSeconds);

        public static PenaltyBubbleSettingsSO Load()
        {
            var settings = Resources.Load<PenaltyBubbleSettingsSO>(ResourcePath);
            return settings != null ? settings : CreateInstance<PenaltyBubbleSettingsSO>();
        }
    }
}
