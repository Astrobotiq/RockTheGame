using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçeğin karaktere ve kollarına tepkisi için saf hesap (docs/flower-hareket-plani.md T9, F-etkilesim.md F3/F4).
    /// Açılar radyan. Yaprak azimutu <b>çiçek-yerel</b>dir (<c>FlowerBloom.PetalAzimuth</c>: yaprak +X'ten başlayıp
    /// <c>Pivot.localPosition = (cos az, 0, sin az)</c> yönündedir); dünya yönünü bu azimuta çeviren tek yer
    /// <see cref="WorldToLocalAzimuth"/>'tır, elle işaret türetilmez.
    ///
    /// İşaret kuralı: pozitif yaw yaprağı çiçek-yerel azimutta <b>artan</b> yöne (dünyada saat yönünün tersine, çiçek
    /// yukarıdan bakılırken) çevirir. Kolun saat yönünün tersindeki yaprak (d &gt; 0) pozitif yaw alır, yani koldan uzaklaşır.
    /// </summary>
    public static class FlowerReactMath
    {
        // Başlangıç yay katsayıları (his geliştiricinin kararı, gamepad ile ayarlanır; F-etkilesim.md §3 tablosu).
        // Ayar SO'su (T10) varsayılan olarak bunları kullanır.
        public const float PetalStiffness = 300f, PetalDamping = 17f;          // ζ≈0,49, ~%17 aşım
        public const float GripStiffness = 700f, GripDamping = 36f;            // ζ≈0,68, ~%5 aşım
        public const float GripReleaseDamping = 22f;                           // bırakışta çırpınma, ζ≈0,42
        public const float DirectionStiffness = 120f, DirectionDamping = 15f;  // ζ≈0,68, kol açısı gecikmeli izlenir

        /// <summary>Açıyı (-π, π] aralığına sarar.</summary>
        public static float WrapPi(float angle)
        {
            angle = Mathf.Repeat(angle + Mathf.PI, 2f * Mathf.PI) - Mathf.PI;
            // Repeat [0, 2π) verir, bu da [-π, π). π'yi (-π) yerine +π göstermek için kenarı düzelt.
            return angle <= -Mathf.PI ? Mathf.PI : angle;
        }

        /// <summary>
        /// <paramref name="target"/> açısını <paramref name="previous"/>'a en yakın eşdeğerine kaydırır (fark ≤ π): yön yayı
        /// ±π geçişinde ters yöne tam tur dönmesin diye hedef bununla beslenir.
        /// </summary>
        public static float UnwrapAngle(float previous, float target) => previous + WrapPi(target - previous);

        /// <summary>
        /// Mesafe falloff'u: <paramref name="d"/> = 0 → 1, <paramref name="radius"/> ve ötesi → 0, arası yumuşak (smoothstep).
        /// </summary>
        public static float Falloff(float d, float radius)
        {
            if (radius <= 0f) return 0f;
            float t = Mathf.Clamp01(1f - Mathf.Max(0f, d) / radius);
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Kolun yaprağı yana itmesi (radyan, işaretli). Tek fonksiyon: kolun iki yanındaki yapraklar zıt yöne gider,
        /// tam kol altındaki yaprak yaw almaz (onu <see cref="PartingDip"/> bastırır). Tepe, kolun <c>halfWidth/3</c> yanında
        /// tam <paramref name="maxYaw"/>; <paramref name="halfWidth"/> ve ötesinde 0.
        /// </summary>
        public static float PartingYaw(float petalAz, float armAz, float halfWidth, float maxYaw)
        {
            if (halfWidth <= 0f) return 0f;
            float x = WrapPi(petalAz - armAz) / halfWidth;
            float ax = Mathf.Abs(x);
            if (ax >= 1f) return 0f;
            float inv = 1f - ax;
            return maxYaw * 6.75f * x * inv * inv;   // 6.75 = 27/4: x(1-|x|)² tepesi 4/27'dir
        }

        /// <summary>Kolun yaprağı aşağı bastırması (radyan, ≥ 0). Çift fonksiyon: kolun iki yanında aynı.</summary>
        public static float PartingDip(float petalAz, float armAz, float halfWidth, float maxDip) =>
            maxDip * Falloff(Mathf.Abs(WrapPi(petalAz - armAz)), halfWidth);

        /// <summary>Hızla büyüyen kuvvet çarpanı: <c>|v| / vRef</c>, [min, 1] aralığına sıkıştırılır (dash'te yaprak savrulmasın).</summary>
        public static float SpeedScale(float speed, float referenceSpeed, float min = 0.3f)
        {
            if (referenceSpeed <= 0f) return 1f;
            return Mathf.Clamp(Mathf.Abs(speed) / referenceSpeed, min, 1f);
        }

        /// <summary>
        /// Gövdenin yaprak ucunu yana itmesi (radyan, işaretli; pozitif = saat yönünün tersine). Tüm vektörler dünya (2B).
        /// <paramref name="tip"/> yaprak ucu, <paramref name="body"/> oyuncu gövdesi, <paramref name="petalDir"/> çiçek merkezinden
        /// yaprağa birim yön (dünya). İtme, uç ile gövdeyi birleştiren doğrultunun yaprağın teğetine izdüşümüdür; gövde yaprağın
        /// saat yönündeki yanındaysa pozitif, tersindeyse negatif.
        /// </summary>
        public static float BodyYaw(Vector2 tip, Vector2 body, Vector2 petalDir, float bodyRadius, float maxYaw, float speedScale = 1f)
        {
            Vector2 away = tip - body;
            float d = away.magnitude;
            if (d < 1e-5f) return 0f;
            Vector2 tangent = new Vector2(-petalDir.y, petalDir.x);   // saat yönünün tersi teğet
            return maxYaw * Falloff(d, bodyRadius) * Vector2.Dot(away / d, tangent) * speedScale;
        }

        /// <summary>Gövdenin yaprak ucunu aşağı bastırması (radyan, ≥ 0): yakınsa daha çok.</summary>
        public static float BodyDip(Vector2 tip, Vector2 body, float bodyRadius, float maxDip, float speedScale = 1f) =>
            maxDip * Falloff((tip - body).magnitude, bodyRadius) * speedScale;

        /// <summary>İki etkiyi birleştirir: toplamak yerine büyük olan kazanır (Riftbreaker: karıştırma, toplama değil).</summary>
        public static float Combine(float a, float b) => Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;

        /// <summary>
        /// Tepki kapısı 0..1: çiçek yalnızca tutulabilirken ve yeterince açıkken tepki verir (kapalı tomurcuğun oyuncuya
        /// "bakması" tutulabilir sanılmasın). <c>open</c> 0,5'ten 0,9'a yumuşakça açılır; uyarıda genlik yarıya kadar iner
        /// (uyarının okunurluğunu titreme ve katlanma taşır).
        /// </summary>
        public static float Gate(bool grabbable, float open, float warn01)
        {
            if (!grabbable) return 0f;
            float t = Mathf.Clamp01((open - 0.5f) / 0.4f);
            float opened = t * t * (3f - 2f * t);
            return opened * (1f - 0.5f * Mathf.Clamp01(warn01));
        }

        /// <summary>
        /// Dünyadaki (2B, çiçek yukarıdan bakılırken) bir yönü çiçek-yerel yaprak azimutuna çevirir. <paramref name="modelRotation"/>
        /// yaprakların bağlı olduğu model Transform'unun dünya dönüşüdür (<c>viewTilt</c>, <c>orientationDeg</c>, açılma
        /// dönüşü ve kök dönüşü hepsi bunun içindedir). Dönüş (-π, π].
        /// </summary>
        public static float WorldToLocalAzimuth(Quaternion modelRotation, Vector2 worldDirection)
        {
            Vector3 local = Quaternion.Inverse(modelRotation) * new Vector3(worldDirection.x, worldDirection.y, 0f);
            return Mathf.Atan2(local.z, local.x);
        }

        /// <summary>
        /// Yaprak pivot'unun yerel Y dönüşü (derece): <c>FlowerBloom.Rebuild</c>'in <c>Euler(0, -az, 0)</c> kuralı, yaw eklenmiş.
        /// <paramref name="yaw"/> = 0 iken yaprağın kurulu duruşudur.
        /// </summary>
        public static float PivotYawDegrees(float azimuth, float yaw) => -(azimuth + yaw) * Mathf.Rad2Deg;
    }
}
