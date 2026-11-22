using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>Polen parçacığının sınıfı: boyut, hız, renk ve şekil sınıfa göre değişir.</summary>
    public enum PollenKind
    {
        /// <summary>Küçük düz yuvarlak nokta (delgeç konfetisi), en çok.</summary>
        Dot,
        /// <summary>Hafif dönük elips (tohum).</summary>
        Ellipse,
        /// <summary>Küçük kâğıt parçası (krem yüz + indigo hilal), nadir "hero" parçacık.</summary>
        Piece
    }

    /// <summary>
    /// Çiçek poleni için saf hesap (docs/flower-hareket-plani.md T14, G-polen.md "Delgeç Konfetisi"). Parçacık sayısı/sınıfı,
    /// organ kaynak noktaları, yön ve hız, kamera ölçeğine göre alt boyut. Rastgelelik dışarıdan 0..1 / -1..1 verilir
    /// (<c>UnityEngine.Random</c> bu sınıfta yok), böylece her şey deterministik ve test edilebilir.
    /// </summary>
    public static class FlowerPollenMath
    {
        /// <summary>Çiçeğin organ (anter) sayısı: <c>Flower/Center</c> <c>_OrganCount</c> ile aynı olmalı.</summary>
        public const int DefaultOrganCount = 5;

        /// <summary>CRT alt sınırı: ekran 480x360 olduğundan 360 / 60 = 6 CRT px; bu bölenle ortho / 60 en az ~3 px verir.</summary>
        public const float OrthoDivisor = 60f;

        /// <summary>Tutunma hızına göre güç: yavaşta <paramref name="floor"/>, <paramref name="maxSpeed"/> ve üstünde 1 (asit temasıyla aynı eğri).</summary>
        public static float Strength(float speed, float minSpeed, float maxSpeed, float floor) =>
            AcidImpactMath.Strength(speed, minSpeed, maxSpeed, floor);

        /// <summary>
        /// Güce göre parçacık sayısı: taban güçte (<paramref name="floor"/>) <paramref name="minCount"/>, tam güçte
        /// <paramref name="maxCount"/>, arası doğrusal ve yuvarlanmış.
        /// </summary>
        public static int Count(float strength, float floor, int minCount, int maxCount)
        {
            if (maxCount < minCount) maxCount = minCount;
            float t = Mathf.InverseLerp(Mathf.Clamp01(floor), 1f, Mathf.Clamp01(strength));
            return Mathf.RoundToInt(Mathf.Lerp(minCount, maxCount, t));
        }

        /// <summary>
        /// <paramref name="index"/>. parçacığın sınıfı: sınıflar patlama içinde karışık dağılır (altın oran sırası), oranlar
        /// <paramref name="dotShare"/> ve <paramref name="ellipseShare"/> (kalanı kâğıt parçası).
        /// </summary>
        public static PollenKind Kind(int index, int count, float dotShare, float ellipseShare)
        {
            if (count <= 0) return PollenKind.Dot;
            // Sıralı değil karışık: 0..1 aralığında eşit dağılan bir dizi (altın oran), böylece parçalar sona yığılmaz.
            float rank = FlowerBloomMath.PetalRank(index + 1);
            if (rank < dotShare) return PollenKind.Dot;
            if (rank < dotShare + ellipseShare) return PollenKind.Ellipse;
            return PollenKind.Piece;
        }

        /// <summary>Parçacık çapı için alt sınır: kamera ölçeğine göre (ortho / 60), geniş kamerada nokta kaybolmasın/titremesin.</summary>
        public static float MinDiameter(float orthoSize) => Mathf.Max(0.0f, orthoSize) / OrthoDivisor;

        public static float ClampSize(float size, float orthoSize) => Mathf.Max(size, MinDiameter(orthoSize));

        /// <summary>
        /// Organ ucunun dünya konumu. Organlar diskin dışında <paramref name="ringRadius"/> yarıçaplı halkada eşit aralıklı,
        /// tepeden yarım adım (180/count derece) kayık: çiçek yön dönüşünü kule iptal ettiği için tepe hep dünyada yukarıdır
        /// (<c>FlowerCenter.shader</c>: organ açıları 90° + yarım adım + k · adım, saat yönünün tersine).
        /// </summary>
        public static Vector2 OrganPoint(int index, int count, Vector2 center, float ringRadius)
        {
            count = Mathf.Max(1, count);
            float a = (90f + 180f / count + index * 360f / count) * Mathf.Deg2Rad;
            return center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ringRadius;
        }

        /// <summary>
        /// <paramref name="toward"/> yönüne en yakın <paramref name="take"/> organın indeksini <paramref name="result"/> dizisine yazar
        /// (en yakın önce). Dönüş: yazılan sayı. Allocation yok.
        /// </summary>
        public static int NearestOrgans(Vector2 center, Vector2 toward, int count, int take, int[] result)
        {
            count = Mathf.Max(1, count);
            take = Mathf.Clamp(take, 0, Mathf.Min(count, result.Length));
            if (take == 0) return 0;
            float target = Mathf.Atan2(toward.y, toward.x);
            int written = 0;
            for (int pick = 0; pick < take; pick++)
            {
                int best = -1;
                float bestD = float.MaxValue;
                for (int k = 0; k < count; k++)
                {
                    bool used = false;
                    for (int u = 0; u < written; u++) if (result[u] == k) { used = true; break; }
                    if (used) continue;
                    Vector2 d = OrganPoint(k, count, Vector2.zero, 1f);
                    float diff = Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, target * Mathf.Rad2Deg));
                    if (diff < bestD) { bestD = diff; best = k; }
                }
                result[written++] = best;
            }
            return written;
        }

        /// <summary>
        /// Parçacığın çıkacağı organ: olasılıkla <paramref name="nearShare"/> oyuncuya dönük organlardan
        /// (<paramref name="near"/>, ilk <paramref name="nearCount"/> eleman), kalanı herhangi bir organdan.
        /// <paramref name="r1"/> grubu, <paramref name="r2"/> grup içinden seçimi belirler (ikisi de 0..1).
        /// </summary>
        public static int PickOrgan(float r1, float r2, int[] near, int nearCount, int totalOrgans, float nearShare)
        {
            totalOrgans = Mathf.Max(1, totalOrgans);
            if (nearCount > 0 && r1 < nearShare)
                return near[Mathf.Min(nearCount - 1, Mathf.FloorToInt(Mathf.Clamp01(r2) * nearCount))];
            return Mathf.Min(totalOrgans - 1, Mathf.FloorToInt(Mathf.Clamp01(r2) * totalOrgans));
        }

        /// <summary>
        /// Çıkış yönü (birim): organdan dışa (radyal) yön, oyuncuya doğru <paramref name="towardBias"/> kadar karıştırılır, sonra
        /// ±<paramref name="coneDeg"/> içinde döndürülür (<paramref name="rand11"/> -1..1).
        /// </summary>
        public static Vector2 EmitDirection(Vector2 radial, Vector2 toPlayer, float towardBias, float coneDeg, float rand11)
        {
            Vector2 r = radial.sqrMagnitude > 1e-8f ? radial.normalized : Vector2.up;
            Vector2 t = toPlayer.sqrMagnitude > 1e-8f ? toPlayer.normalized : r;
            Vector2 baseDir = Vector2.Lerp(r, t, Mathf.Clamp01(towardBias));
            if (baseDir.sqrMagnitude < 1e-8f) baseDir = r;   // radyal ile oyuncu yönü tam zıt: radyal kalır
            baseDir.Normalize();
            float ang = Mathf.Clamp(rand11, -1f, 1f) * coneDeg * Mathf.Deg2Rad;
            float c = Mathf.Cos(ang), s = Mathf.Sin(ang);
            return new Vector2(baseDir.x * c - baseDir.y * s, baseDir.x * s + baseDir.y * c);
        }

        /// <summary>
        /// Parçacık hızı: yön x sürat + oyuncunun hızından küçük bir pay (<paramref name="inherit"/>), payın büyüklüğü
        /// <paramref name="maxInherit"/> ile sınırlı (parçacıklar iris/ekran dışına kaçmasın).
        /// </summary>
        public static Vector2 Velocity(Vector2 dir, float speed, Vector2 playerVelocity, float inherit, float maxInherit) =>
            dir * speed + Vector2.ClampMagnitude(playerVelocity * inherit, Mathf.Max(0f, maxInherit));

        /// <summary>Ağırlıklı seçim: <paramref name="r"/> (0..1) ağırlıklara göre bir indeks seçer. Ağırlıklar toplamı 0 ise 0.</summary>
        public static int PickWeighted(float r, float[] weights)
        {
            float sum = 0f;
            for (int i = 0; i < weights.Length; i++) sum += Mathf.Max(0f, weights[i]);
            if (sum <= 0f) return 0;
            float x = Mathf.Clamp01(r) * sum;
            for (int i = 0; i < weights.Length; i++)
            {
                x -= Mathf.Max(0f, weights[i]);
                if (x < 0f) return i;
            }
            return weights.Length - 1;
        }
    }
}
