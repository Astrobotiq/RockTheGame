using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğin karaktere ve kollarına tepkisinin ayarları (docs/flower-hareket-plani.md T10, F-etkilesim.md F1-F4).
    /// Açılar derece (kodda radyana çevrilir). Tüm değerler başlangıç tahminidir; his geliştiricinin kararı, gamepad ile ayarlanır.
    /// Hesap <see cref="FlowerReactMath"/>'ta, sürücü <see cref="FlowerReactor"/>'da.
    /// </summary>
    [CreateAssetMenu(fileName = "FlowerReactionSettings", menuName = "RockTheGame/Flower/Reaction Settings")]
    public class FlowerReactionSettingsSO : ScriptableObject
    {
        [Header("Genel")]
        [Tooltip("Kapalıysa çiçek karaktere hiç tepki vermez (eski davranış).")]
        public bool enabled = true;
        [Tooltip("Oyuncu çiçek menzilinin bu kadar (dünya birimi) ötesindeyse ve yaylar durgunsa reactor hiçbir şey hesaplamaz.")]
        public float farMargin = 3f;

        [Header("Kol süpürmesi (tutunan kol yaprakları saat yelkovanı gibi aralar)")]
        [Range(0f, 25f)] public float maxYawDeg = 12f;
        [Range(0f, 25f)] public float maxDipDeg = 10f;
        [Tooltip("Kolun yaprağı etkilediği yarım açı. 5 yaprakta 50°: kol hangi açıda olursa olsun en az bir yaprak menzilde.")]
        [Range(20f, 90f)] public float halfWidthDeg = 50f;

        [Header("Gövde yakınlığı (oyuncu çiçeğin üstünden geçerken yapraklar yol verir)")]
        [Tooltip("Etki yarıçapı, yaprak boyunun oranı (yaprak boyu = (petalLength + coreRadius) x çiçek ölçeği).")]
        [Range(0.1f, 1.2f)] public float bodyRadiusRel = 0.5f;
        [Range(0f, 20f)] public float bodyMaxYawDeg = 8f;
        [Range(0f, 20f)] public float bodyMaxDipDeg = 6f;
        [Tooltip("Hızla büyüyen kuvvetin tabanı: bu orandan (|v| / referans) aşağısı kıskaçlanır.")]
        [Range(0f, 1f)] public float bodySpeedFloor = 0.3f;
        [Tooltip("Hız referansı (b/sn). 0 ise PlayerStatsSO.MoveSpeed kullanılır.")]
        public float bodySpeedReference;

        [Header("Yaylar (sertlik / sönüm)")]
        [Tooltip("Yaprak yaw ve eğimi: ζ ≈ 0,49, ~%17 aşım. Yaprak başına seed'den küçük sapma eklenir (yapraklar eşzamanlı oynamasın).")]
        public float petalStiffness = FlowerReactMath.PetalStiffness;
        public float petalDamping = FlowerReactMath.PetalDamping;
        [Range(0f, 0.5f)] public float petalSeedVariation = 0.2f;
        [Tooltip("Kavrama (yumruk) yayı: tutarken sert ve az aşımlı; bırakırken daha az sönümlü (çırpınma).")]
        public float gripStiffness = FlowerReactMath.GripStiffness;
        public float gripDamping = FlowerReactMath.GripDamping;
        public float gripReleaseDamping = FlowerReactMath.GripReleaseDamping;
        [Tooltip("Kolun yönü gecikmeli izlenir (kol hızlı dönerken yapraklar sırayla çarpılır).")]
        public float directionStiffness = FlowerReactMath.DirectionStiffness;
        public float directionDamping = FlowerReactMath.DirectionDamping;

        [Header("Kavrama yumruğu (iplik tacı tutan ele sarılır)")]
        [Tooltip("Tek kol tutarken yumruğun gücü (iki kolda 1). Tek kol baskın durum, yarım yumruk zayıf kalır.")]
        [Range(0f, 1f)] public float gripOneArm = 0.75f;
        [Tooltip("Tam kavramada şeritlerin ek kalkışı (derece), eğriliği, tabana büzülme ve kısalma oranı.")]
        [Range(0f, 40f)] public float gripLiftDeg = 20f;
        [Range(0f, 1f)] public float gripCurl = 0.3f;
        [Range(0f, 0.6f)] public float gripPinch = 0.25f;
        [Range(0f, 0.4f)] public float gripShorten = 0.1f;

        [Header("Yakalama darbesi (tutunma anında)")]
        [Tooltip("Ortadaki kulenin kabarması (pop yayına verilen hız). +4 yaklaşık +%15 büyür.")]
        public float pokeCenterKick = 4f;
        [Tooltip("Çiçeğin açılma yayına verilen hız: negatif = çekinir, pozitif = karşılar. -2 yaklaşık -0,08 açılma.")]
        public float pokeOpenKick = -2f;

        /// <summary>Kavrama yumruğunun şerit etkisi (FlowerCoronaMath'a verilir).</summary>
        public FlowerCoronaMath.GripShape GripShape => new FlowerCoronaMath.GripShape
        {
            Lift = gripLiftDeg * Mathf.Deg2Rad,
            Curl = gripCurl,
            Pinch = gripPinch,
            Shorten = gripShorten
        };

        private void OnValidate()
        {
            petalStiffness = Mathf.Max(1f, petalStiffness);
            gripStiffness = Mathf.Max(1f, gripStiffness);
            directionStiffness = Mathf.Max(1f, directionStiffness);
            petalDamping = Mathf.Max(0f, petalDamping);
            gripDamping = Mathf.Max(0f, gripDamping);
            gripReleaseDamping = Mathf.Max(0f, gripReleaseDamping);
            directionDamping = Mathf.Max(0f, directionDamping);
            farMargin = Mathf.Max(0f, farMargin);
        }
    }
}
