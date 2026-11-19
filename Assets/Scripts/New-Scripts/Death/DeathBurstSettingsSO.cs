using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Ölüm efekti (karakterin kendi pikselleri) ve asitte kalan kafaların ayarları. Değerler başlangıçtır;
    /// his kullanıcının kararı. Önizleme: docs/ui-research/death-particle/death-particle-v2.html
    /// </summary>
    [CreateAssetMenu(fileName = "DeathBurstSettings", menuName = "RockTheGame/Death/Death Burst Settings")]
    public class DeathBurstSettingsSO : ScriptableObject
    {
        [Header("Zamanlama")]
        [Tooltip("Ölüm anında kısa donma (HitStopEvents). 0 ise donma yok.")]
        public float hitstopSeconds = 0.09f;
        [Tooltip("Parçacıklar için toplam süre; hitstop dahil.")]
        public float duration = 0.6f;
        [Tooltip("Başta beyaz silüet flaşı (kare sayısı, 60 fps varsayımıyla). 0 ise flaş yok.")]
        [Range(0, 4)] public int flashFrames = 2;

        [Header("Parçacıklar (karakterin kendi pikselleri)")]
        [Tooltip("Karakterin kaç piksellik karesi tek parçacık olsun. 3 piksel = 2 sanat pikseli. " +
                 "CRT filtresi ekranı 480x360'a indirdiği için 1-2 pikselde titreyebilir.")]
        [Range(1, 5)] public int cellTexels = 2;
        [Tooltip("Piksel ızgarası: parçacık konumları 1/ppu birime oturur.")]
        public float pixelsPerUnit = 16f;
        [Tooltip("Yaklaşık yayılma yarıçapı (birim). Toplam yol = hız / drag.")]
        public float spread = 2.5f;
        public float drag = 3f;
        public float gravity = 4.5f;
        public float gravityDelay = 0.12f;
        [Tooltip("Oyuncunun ölüm hızından parçacıklara geçen pay.")]
        [Range(0f, 1f)] public float velocityInherit = 0.3f;
        [Tooltip("Geçen hızın üst sınırı (parçacıkların iris dairesinden kaçmaması için).")]
        public float maxInheritSpeed = 4f;
        [Tooltip("Bu alfanın altındaki pikseller boş sayılır.")]
        [Range(1, 255)] public int alphaMin = 25;

        [Header("Kamera sarsıntısı")]
        [Tooltip("Cinemachine impulse hızı (yön: ölüm hızı). 0 ise sarsıntı yok.")]
        public float shakeForce = 15f;

        [Header("Kafa")]
        [Tooltip("Kafa sprite'ı. Şimdilik pembe daire (placeholder); sonra gerçek pixel art ile değişir.")]
        public Sprite headSprite;
        [Tooltip("Diğer ölümlerde kafa fırlatma hızı (spread x drag çarpanı).")]
        public float headThrowFactor = 0.55f;
        [Tooltip("Kafanın oyuncunun konumuna göre başlangıç yüksekliği (birim).")]
        public float headStartOffsetY = 0.55f;

        [Header("Asit dışı ölümlerde kalan kafalar (fizik)")]
        [Tooltip("Tüm oyunda en fazla bu kadar kafa. Havuz taşarsa en eskisi iris kapalıyken silinir.")]
        [Range(5, 15)] public int maxGroundHeads = 12;
        [Tooltip("Kafanın çarpıştığı katmanlar: Ground ve DeadHead (kafalar birbirini iter). Oyuncu ve dikenler (Death) " +
                 "yok sayılır, yani kafa oyuncuyla iç içe geçer ve dikenlerin üstünü örtmez.")]
        public LayerMask headCollisionMask;
        public float headGravityScale = 2f;
        public float headDamping = 0.3f;
        [Tooltip("Dönme sönümü: yuvarlanan kafalar zeminde sonsuza kadar dönmesin.")]
        public float headAngularDamping = 2f;
        [Range(0f, 1f)] public float headBounciness = 0.25f;
        [Range(0f, 1f)] public float headFriction = 0.6f;
        [Tooltip("Oyuncunun collider'ı kafaya değince verilen itme hızı.")]
        public float headPushSpeed = 3.5f;
        [Tooltip("Oyuncunun hızından itmeye geçen pay.")]
        [Range(0f, 1f)] public float headPushInherit = 0.3f;
        [Tooltip("İtme hızının üst sınırı.")]
        public float headPushMaxSpeed = 7f;
        [Tooltip("Aynı kafaya art arda itme arasındaki en kısa süre.")]
        public float headPushInterval = 0.12f;

        [Header("Dash vuruşu (dash atarken kafaya değince)")]
        [Tooltip("Dash ile değince kafanın aldığı hız (normal itme 3.5). Dash'in kendisi ≈33 birim/sn.")]
        public float headDashPushSpeed = 10f;
        [Tooltip("Dash vuruşunun hız üst sınırı.")]
        public float headDashPushMaxSpeed = 14f;
        [Tooltip("Dash vuruşunun yukarı eğimi (derece, yatay dash'e göre). 0 = düz, 25 = hafif yukarı.")]
        public float headDashLiftDeg = 25f;
        [Tooltip("Aynı kafaya art arda dash vuruşu arasındaki en kısa süre (dash 0.15 sn sürer).")]
        public float headDashPushInterval = 0.2f;
        [Tooltip("Dash vuruşunun yassılması.")]
        public float headDashSquash = 0.3f;

        [Header("Kafa squash and stretch (asit dışı)")]
        [Tooltip("Birim hız başına uzama: hızlı düşen kafa hız yönünde uzar.")]
        public float headStretchPerSpeed = 0.02f;
        [Tooltip("Bir eksendeki en büyük uzama (0.25 = %25).")]
        public float headStretchMax = 0.25f;
        [Tooltip("Çarpma hızı başına yassılma.")]
        public float headSquashPerSpeed = 0.03f;
        [Tooltip("En büyük yassılma.")]
        public float headSquashMax = 0.3f;
        [Tooltip("Bu hızın altındaki çarpmalar yassılmaz.")]
        public float headSquashMinSpeed = 2f;
        [Tooltip("Yaylanma hızı (rad/sn): büyük = hızlı titreşim.")]
        public float headSquashFreq = 18f;
        [Tooltip("Yaylanmanın sönme hızı: büyük = çabuk durur.")]
        public float headSquashDecay = 9f;
        [Tooltip("Oyuncu kafayı itince verilen yassılma.")]
        public float headPushSquash = 0.2f;

        [Header("Asitte kalan kafalar")]
        [Tooltip("Tüm oyunda en fazla bu kadar kafa. Dolunca en eskisi çözülür.")]
        [Range(5, 15)] public int maxHeads = 12;
        [Tooltip("Havuzda kafanın duracağı derinlik: yüzeyden dibe oran (min, max).")]
        public Vector2 headRestFraction = new Vector2(0.35f, 0.65f);
        public float headSlotSpacing = 0.45f;
        public float headSinkRate = 3.2f;
        public float headSlideRate = 6f;
        public float headBobSeconds = 0.6f;
        public float headDissolveSeconds = 0.5f;
        [Tooltip("Hareketli asite çekilme süresi.")]
        public float blobEnterSeconds = 0.18f;
        [Tooltip("Hareketli asitte kafanın durabileceği yarıçap: asidin yarı genişliğinin oranı.")]
        public float blobRadiusFactor = 0.5f;
        [Tooltip("Asit tonu (kafa rengiyle çarpılır).")]
        public Color headTint = new Color(0.6f, 1f, 0.5f, 1f);
        [Range(0f, 1f)] public float headTintShallow = 0.3f;
        [Range(0f, 1f)] public float headTintDeep = 0.8f;
        public float headTintSettleSeconds = 0.6f;
    }
}
