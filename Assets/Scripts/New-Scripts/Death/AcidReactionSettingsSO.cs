using New_Scripts.Audio;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asit temas tepkisinin sayısal ayarları (docs/asit-tepki-plani.md §4). Görsel ayarlar (renk, çukur,
    /// parlama) asit materyalinde. Değerler başlangıçtır; his kullanıcının kararı.
    /// </summary>
    [CreateAssetMenu(fileName = "AcidReactionSettings", menuName = "RockTheGame/Hazard/Acid Reaction Settings")]
    public class AcidReactionSettingsSO : ScriptableObject
    {
        [Header("Güç (giriş hızına göre)")]
        [Tooltip("Bu hızın altında taban güç.")]
        public float minSpeed = 4f;
        [Tooltip("Bu hızda ve üstünde tam güç.")]
        public float maxSpeed = 40f;
        [Range(0f, 1f)] public float strengthFloor = 0.35f;

        [Header("Damlalar (2 px kareler)")]
        [Tooltip("Tam güçte y, taban güçte x tane.")]
        public Vector2Int dropletCount = new Vector2Int(6, 14);
        public Vector2 dropletSpeed = new Vector2(5f, 9f);
        public int dropletSizePx = 2;

        [Header("Büyük damlalar (3 px)")]
        public Vector2Int bigDropletCount = new Vector2Int(2, 3);
        public Vector2 bigDropletSpeed = new Vector2(2.5f, 4f);
        public int bigDropletSizePx = 3;

        [Header("Yön")]
        [Tooltip("Yüzey normali etrafında ± derece.")]
        public float coneDeg = 55f;
        [Tooltip("Oyuncunun yatay hızından damlaya geçen pay.")]
        public float inheritX = 0.3f;
        [Tooltip("Oyuncunun düşey hız büyüklüğünden yukarı fırlatmaya eklenen pay.")]
        public float inheritY = 0.1f;
        [Tooltip("Damlalar iris dairesinden kaçmasın diye hız tavanı.")]
        public float maxDropletSpeed = 9f;
        public float pixelsPerUnit = 16f;

        [Header("Geri bildirim (boş bırakılabilir)")]
        [Tooltip("Killed'de çalar ve genel Death titreşimini ezer.")]
        public RumbleProfile acidDeathRumble;
        public AudioCueSO splashCue;
        public AudioCueSO sizzleCue;

        private void OnValidate()
        {
            minSpeed = Mathf.Max(0f, minSpeed);
            maxSpeed = Mathf.Max(minSpeed + 0.01f, maxSpeed);
            dropletCount = new Vector2Int(Mathf.Max(0, Mathf.Min(dropletCount.x, dropletCount.y)), Mathf.Max(dropletCount.x, dropletCount.y));
            bigDropletCount = new Vector2Int(Mathf.Max(0, Mathf.Min(bigDropletCount.x, bigDropletCount.y)), Mathf.Max(bigDropletCount.x, bigDropletCount.y));
            dropletSpeed = new Vector2(Mathf.Min(dropletSpeed.x, dropletSpeed.y), Mathf.Max(dropletSpeed.x, dropletSpeed.y));
            bigDropletSpeed = new Vector2(Mathf.Min(bigDropletSpeed.x, bigDropletSpeed.y), Mathf.Max(bigDropletSpeed.x, bigDropletSpeed.y));
            dropletSizePx = Mathf.Max(1, dropletSizePx);
            bigDropletSizePx = Mathf.Max(1, bigDropletSizePx);
            pixelsPerUnit = Mathf.Max(1f, pixelsPerUnit);
            maxDropletSpeed = Mathf.Max(0.1f, maxDropletSpeed);
        }
    }
}
