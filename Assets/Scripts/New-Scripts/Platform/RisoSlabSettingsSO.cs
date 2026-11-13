using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Riso levha platform görünümünün ayarları (docs/ui-research/halftone/implementation-plan.md).
    /// His geliştiricinin kararıdır; değer değişince eski ve yeni değeri not et. İz ayarları H5'te buraya eklenir.
    /// </summary>
    [CreateAssetMenu(fileName = "RisoSlabSettings", menuName = "Platform/Riso Slab Settings")]
    public class RisoSlabSettingsSO : ScriptableObject
    {
        [Header("Hıza bağlı kayık baskı")]
        [Tooltip("Görsel hızın üstel yumuşatma zaman sabiti (sn). Durunca kaymanın geri yaylanma süresini de belirler.")]
        [Min(0.001f)] public float velocitySmoothTime = 0.1f;

        [Tooltip("Kayma katsayısı k (sn): teal hızın tersine 2k·|v|, pembe hız yönünde k·|v|.")]
        [Min(0f)] public float misregVelocityK = 0.012f;

        [Tooltip("Hızdan gelen toplam kaymanın üst sınırı (dünya birimi).")]
        [Min(0f)] public float misregMax = 0.06f;

        [Header("Oyuncu teması (aşama 2)")]
        [Tooltip("Oyuncu platformun üstündeyken (ya da duvara tutunurken) kalıcı etki miktarı (0-1).")]
        [Range(0f, 1f)] public float contactStandingAmount = 0.15f;

        [Tooltip("İniş anında 1'e fırlayan etkinin hedefe yaylanma zaman sabiti (sn). Temas bitince sönme süresi de bu.")]
        [Min(0.001f)] public float contactDecayTime = 0.25f;

        [Tooltip("Etki yarıçapı (dünya birimi). Shader'daki _InfluenceRadius.")]
        [Min(0.1f)] public float influenceRadius = 2.5f;

        [Header("Mürekkep değişimi dalgası (aşama 2)")]
        [Tooltip("İniş anında ayaklardan genişleyen halkanın hızı (birim/sn).")]
        [Min(0.1f)] public float shiftSpeed = 6f;

        [Tooltip("Halka bandının genişliği: bu bandın içindeki sarı mürekkep pembeye döner (birim).")]
        [Min(0.1f)] public float shiftBandWidth = 1.2f;

        [Tooltip("Halka bu yarıçapı (bant dahil) geçince söner (birim).")]
        [Min(0.5f)] public float shiftMaxRadius = 8f;

        [Header("İz (HalftoneTrailEmitter)")]
        [Tooltip("Nokta aralığı P (dünya birimi). Shader'daki _DotPitch ile aynı olmalı; CRT kararına göre 0.42 ya da 0.5.")]
        [Min(0.05f)] public float dotPitch = 0.42f;

        [Tooltip("Gecikmeli kopya: teal, sarı, pembe (sn).")]
        public Vector3 inkDelays = new Vector3(0f, 0.06f, 0.12f);

        [Tooltip("Mürekkep başına, katedilen birim başına parçacık.")]
        [Min(0f)] public float trailPerUnitPerInk = 1.2f;

        [Tooltip("Yoğunluk genişlikle artar: clamp(genişlik / bu değer, 1, 2).")]
        [Min(0.01f)] public float densityReferenceWidth = 3f;

        [Tooltip("Parçacık ömrü aralığı (sn).")]
        public Vector2 trailLifetime = new Vector2(0.7f, 1.1f);

        [Tooltip("Başlangıç hızı = platform hızı × bu değer (negatif: geriye).")]
        public float trailInheritVelocity = -0.15f;

        [Tooltip("Hareket yönüne dik rastgele hız (± birim/sn).")]
        [Min(0f)] public float trailPerpJitter = 0.2f;

        [Tooltip("Sürtünme (1/sn). Parçacık sistemine LimitVelocityOverLifetime dampen olarak verilir.")]
        [Min(0f)] public float trailDrag = 3f;

        [Tooltip("%15 olasılıkla 'mürekkep damlası': çap 1.2P.")]
        [Range(0f, 1f)] public float bigDropChance = 0.15f;

        [Min(1)] public int maxParticlesPerInk = 18;
        [Min(1)] public int maxParticlesTotal = 54;

        [Tooltip("Bir karede bu mesafeden fazla yer değiştiren platform ışınlanmış sayılır (birim).")]
        [Min(0.1f)] public float teleportThreshold = 1.5f;

        [Header("Mürekkep renkleri")]
        public Color tealInk = new Color(0.157f, 0.604f, 0.643f, 1f);   // #289AA4
        public Color yellowInk = new Color(0.988f, 0.682f, 0.016f, 1f); // #FCAE04
        public Color pinkInk = new Color(1f, 0.318f, 0.871f, 1f);       // #FF51DE
    }
}
