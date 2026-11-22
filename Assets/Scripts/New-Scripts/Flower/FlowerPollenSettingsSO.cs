using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçek poleninin ayarları (docs/flower-hareket-plani.md T14, G-polen.md "Delgeç Konfetisi"). Tutunma anında organ uçlarından
    /// birkaç düz renkli kâğıt konfeti ve nokta dökülür. Değerler başlangıç tahminidir; his geliştiricinin kararı.
    /// Hesap <see cref="FlowerPollenMath"/>'ta, sürücü <see cref="FlowerPollen"/>'de. Sınıf başına diziler sırası
    /// <see cref="PollenKind"/> ile aynıdır: Dot, Ellipse, Piece.
    /// </summary>
    [CreateAssetMenu(fileName = "FlowerPollenSettings", menuName = "RockTheGame/Flower/Pollen Settings")]
    public class FlowerPollenSettingsSO : ScriptableObject
    {
        [Header("Genel")]
        public bool enabled = true;
        [Tooltip("Tutunmadan bu kadar sonra patlar: yaprak sarsıntısından 2-4 kare sonra (follow-through).")]
        [Min(0f)] public float delay = 0.05f;
        [Tooltip("Çiçek başına en çok canlı parçacık. Dolunca doğum kırpılır.")]
        [Min(1)] public int maxParticles = 48;
        [Tooltip("Çiçekler arası ortak bekleme (sn): aynı karede çok çiçek patlarsa ses/titreşim birikmesin.")]
        [Min(0f)] public float globalCooldown = 0.1f;

        [Header("Güç (tutunma hızına göre)")]
        public float minSpeed = 4f;
        public float maxSpeed = 40f;
        [Range(0f, 1f)] public float strengthFloor = 0.35f;

        [Header("Sayı ve sınıf dağılımı")]
        [Tooltip("x: taban güçte, y: tam güçte parçacık sayısı.")]
        public Vector2Int count = new Vector2Int(10, 18);
        [Tooltip("Nokta ve elips payı (kalanı kâğıt parçası): 0,5 + 0,33 + 0,17.")]
        [Range(0f, 1f)] public float dotShare = 0.5f;
        [Range(0f, 1f)] public float ellipseShare = 0.33f;

        [Header("Boyut (dünya birimi çapı), hız (b/sn) ve ömür (sn): sınıf başına [Dot, Ellipse, Piece]")]
        public Vector2[] size =
        {
            new Vector2(0.14f, 0.20f), new Vector2(0.22f, 0.30f), new Vector2(0.32f, 0.45f)
        };
        public Vector2[] speed =
        {
            new Vector2(3.5f, 6f), new Vector2(2.5f, 4.5f), new Vector2(2f, 3.5f)
        };
        [Tooltip("Ömür tüm sınıflar için aynı aralıkta; kâğıt parçaları üst yarıyı alır.")]
        public Vector2 lifetime = new Vector2(1.2f, 1.9f);
        [Tooltip("Sınıf başına yerçekimi çarpanı (Physics2D yerçekimine göre).")]
        public float[] gravity = { 0.09f, 0.09f, 0.15f };

        [Header("Kaynak ve yön")]
        [Tooltip("Oyuncuya dönük organ sayısı ve parçacıkların bu organlardan çıkma oranı.")]
        [Range(1, 5)] public int nearOrgans = 3;
        [Range(0f, 1f)] public float nearShare = 0.7f;
        [Tooltip("Çıkış yönünün oyuncuya doğru eğilimi (0 = tam dışa, 1 = tam oyuncuya).")]
        [Range(0f, 1f)] public float towardBias = 0.5f;
        [Range(0f, 180f)] public float coneDeg = 70f;
        [Tooltip("Oyuncunun tutunma hızından parçacığa geçen pay ve payın üst sınırı (b/sn).")]
        [Range(0f, 0.6f)] public float inherit = 0.2f;
        public float maxInherit = 3.5f;

        [Header("Renk (sınıf başına ağırlık: Krem, Beyaz, Lavanta, İndigo, Kobalt)")]
        public Color[] palette =
        {
            new Color(1f, 0.953f, 0.769f, 1f),      // krem   #FFF3C4
            new Color(1f, 1f, 1f, 1f),              // beyaz
            new Color(0.898f, 0.914f, 1f, 1f),      // lavanta #E5E9FF
            new Color(0.188f, 0.216f, 0.471f, 1f),  // indigo #303778
            new Color(0.082f, 0.212f, 0.910f, 1f)   // kobalt #1536E8
        };
        [Tooltip("Gökte krem/beyaz tek başına okunmaz (1.9:1): küçük taneler koyu ağırlıklı, parçalar krem (indigo hilal sprite'ta).")]
        public float[] dotColorWeights = { 0.30f, 0.05f, 0f, 0.40f, 0.25f };
        public float[] ellipseColorWeights = { 0.50f, 0.20f, 0f, 0.30f, 0f };
        public float[] pieceColorWeights = { 0.60f, 0.25f, 0.15f, 0f, 0f };

        [Header("Ses (isteğe bağlı)")]
        [Tooltip("Patlamayla aynı anda (SFX-Play, tutunma noktasında). Klip yoksa sessiz.")]
        public AudioCueSO pollenCue;

        public float[] ColorWeights(PollenKind kind) => kind switch
        {
            PollenKind.Dot => dotColorWeights,
            PollenKind.Ellipse => ellipseColorWeights,
            _ => pieceColorWeights
        };

        public Vector2 SizeRange(PollenKind kind) => Pick(size, kind, new Vector2(0.2f, 0.3f));
        public Vector2 SpeedRange(PollenKind kind) => Pick(speed, kind, new Vector2(2f, 5f));
        public float Gravity(PollenKind kind) => (int)kind < gravity.Length ? gravity[(int)kind] : 0.1f;

        private static Vector2 Pick(Vector2[] arr, PollenKind kind, Vector2 fallback) =>
            arr != null && (int)kind < arr.Length ? arr[(int)kind] : fallback;

        private void OnValidate()
        {
            if (count.y < count.x) count.y = count.x;
            if (maxSpeed < minSpeed) maxSpeed = minSpeed;
            if (lifetime.y < lifetime.x) lifetime.y = lifetime.x;
            if (ellipseShare > 1f - dotShare) ellipseShare = 1f - dotShare;
            maxParticles = Mathf.Max(1, maxParticles);
            for (int i = 0; size != null && i < size.Length; i++)
                if (size[i].y < size[i].x) size[i].y = size[i].x;
            for (int i = 0; speed != null && i < speed.Length; i++)
                if (speed[i].y < speed[i].x) speed[i].y = speed[i].x;
        }
    }
}
