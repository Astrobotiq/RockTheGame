using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>Riso levha izinin üç mürekkebi. Sıra shader'daki katman sırasıyla aynı (teal, sarı, pembe).</summary>
    public enum TrailInk
    {
        Teal = 0,
        Yellow = 1,
        Pink = 2
    }

    /// <summary>
    /// HalftoneTrailEmitter'ın saf hesabı (Unity nesnesi gerektirmez, EditMode'da test edilir).
    /// Kafes formülü RisoSlab.shader'daki InkDots ile aynıdır (docs/ui-research/halftone/implementation-plan.md H5).
    /// </summary>
    public static class HalftoneTrailMath
    {
        /// <summary>Shader katman açıları (derece): teal 15, sarı 45, pembe 75.</summary>
        public static float AngleDegrees(TrailInk ink)
        {
            switch (ink)
            {
                case TrailInk.Teal: return 15f;
                case TrailInk.Yellow: return 45f;
                default: return 75f;
            }
        }

        /// <summary>Bir karede eşikten fazla yer değiştiren platform ışınlanmış sayılır.</summary>
        public static bool IsTeleport(Vector2 previous, Vector2 current, float threshold)
        {
            return (current - previous).sqrMagnitude > threshold * threshold;
        }

        /// <summary>Genişliğe göre iz yoğunluğu: perUnit × clamp(width/refWidth, 1, 2).</summary>
        public static float DensityPerUnit(float perUnit, float width, float refWidth)
        {
            return perUnit * Mathf.Clamp(width / Mathf.Max(refWidth, 0.01f), 1f, 2f);
        }

        /// <summary>
        /// Katedilen mesafeyi biriktirir, tam sayıya ulaşan kısmı doğum sayısı olarak döndürür; kesir birikimde kalır.
        /// </summary>
        public static int Accumulate(ref float accumulator, float distance, float perUnit)
        {
            accumulator += Mathf.Max(0f, distance) * perUnit;
            int count = Mathf.FloorToInt(accumulator);
            accumulator -= count;
            return count;
        }

        /// <summary>Nokta çapı: shader'daki tabloyla aynı. v: 0 üst, 1 alt. Minimum v altında nokta yoktur (0 döner).</summary>
        public static float DiameterFor(TrailInk ink, float v, float pitch)
        {
            switch (ink)
            {
                case TrailInk.Yellow: return pitch * (0.50f + 0.35f * v);
                case TrailInk.Pink: return v < 0.20f ? 0f : pitch * (0.26f + 0.40f * v);
                default: return v < 0.35f ? 0f : pitch * (0.24f + 0.34f * v);
            }
        }

        /// <summary>v: dünya yukarısına göre derinlik oranı (0 üst, 1 alt). up birim vektör, yerel uzayda.</summary>
        public static float DepthV(Vector2 localPoint, Vector2 halfSize, Vector2 up)
        {
            float support = halfSize.x * Mathf.Abs(up.x) + halfSize.y * Mathf.Abs(up.y);
            return Mathf.Clamp01((support - Vector2.Dot(localPoint, up)) / Mathf.Max(2f * support, 1e-4f));
        }

        /// <summary>
        /// Gecikmeli platform kopyasının arka %40'ı ve alt yarısındaki (v &gt; 0.4) bir kafes hücresini rastgele seçer.
        /// Dönen yerel uzay merkezi ve v döner. Hareket yönü sıfırsa arka koşulu aranmaz.
        /// </summary>
        public static bool TryPickCell(System.Random rng, TrailInk ink, Vector2 size, Vector2 upLocal, Vector2 dirLocal,
            float pitch, out Vector2 centerLocal, out float v)
        {
            Vector2 half = size * 0.5f;
            float rad = AngleDegrees(ink) * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            bool hasDir = dirLocal.sqrMagnitude > 1e-8f;
            Vector2 dir = hasDir ? dirLocal.normalized : Vector2.zero;
            float supportDir = half.x * Mathf.Abs(dir.x) + half.y * Mathf.Abs(dir.y);

            for (int attempt = 0; attempt < 12; attempt++)
            {
                float x = (float)(rng.NextDouble() * 2.0 - 1.0) * half.x;
                float y = (float)(rng.NextDouble() * 2.0 - 1.0) * half.y;
                // shader'daki kafes: q = R(-a)·p, hücre merkezi, geri döndür
                float qx = c * x + s * y;
                float qy = -s * x + c * y;
                float cqx = (Mathf.Floor(qx / pitch) + 0.5f) * pitch;
                float cqy = (Mathf.Floor(qy / pitch) + 0.5f) * pitch;
                Vector2 center = new Vector2(c * cqx - s * cqy, s * cqx + c * cqy);

                if (Mathf.Abs(center.x) > half.x || Mathf.Abs(center.y) > half.y) continue;
                float depth = DepthV(center, half, upLocal);
                if (depth <= 0.4f) continue;
                if (hasDir && supportDir > 1e-4f)
                {
                    float along = Vector2.Dot(center, dir) / (2f * supportDir); // -0.5 arka, +0.5 ön
                    if (along > -0.1f) continue; // arka %40
                }

                centerLocal = center;
                v = depth;
                return true;
            }

            centerLocal = Vector2.zero;
            v = 0f;
            return false;
        }
    }

    /// <summary>
    /// Platformun son birkaç yüz ms'lik konum ve dönüş geçmişi. Gecikmeli kopya örneklemesi ve teleport sıfırlaması için.
    /// </summary>
    public class TrailHistory
    {
        private struct Sample
        {
            public float Time;
            public Vector2 Position;
            public float Rotation;
        }

        private readonly List<Sample> _samples = new List<Sample>(64);
        private readonly float _keepSeconds;

        public int Count => _samples.Count;

        public TrailHistory(float keepSeconds)
        {
            _keepSeconds = keepSeconds;
        }

        public void Record(float time, Vector2 position, float rotationDegrees)
        {
            _samples.Add(new Sample { Time = time, Position = position, Rotation = rotationDegrees });
            int drop = 0;
            while (drop < _samples.Count - 2 && _samples[drop + 1].Time < time - _keepSeconds) drop++;
            if (drop > 0) _samples.RemoveRange(0, drop);
        }

        public void Clear()
        {
            _samples.Clear();
        }

        /// <summary>time anındaki konum/dönüş; kayıt aralığı dışındaysa en yakın uç. Boşsa false.</summary>
        public bool TrySample(float time, out Vector2 position, out float rotationDegrees)
        {
            position = Vector2.zero;
            rotationDegrees = 0f;
            if (_samples.Count == 0) return false;

            if (time <= _samples[0].Time)
            {
                position = _samples[0].Position;
                rotationDegrees = _samples[0].Rotation;
                return true;
            }

            for (int i = _samples.Count - 1; i > 0; i--)
            {
                Sample b = _samples[i];
                Sample a = _samples[i - 1];
                if (time >= a.Time)
                {
                    float span = b.Time - a.Time;
                    float t = span > 1e-6f ? Mathf.Clamp01((time - a.Time) / span) : 1f;
                    position = Vector2.Lerp(a.Position, b.Position, t);
                    rotationDegrees = Mathf.LerpAngle(a.Rotation, b.Rotation, t);
                    return true;
                }
            }

            Sample last = _samples[_samples.Count - 1];
            position = last.Position;
            rotationDegrees = last.Rotation;
            return true;
        }
    }

    /// <summary>
    /// İz parçacık bütçesi: mürekkep başına ve toplam üst sınır. Parçacıkların ölüm zamanları tutulur,
    /// sınır doluysa doğum atlanır.
    /// </summary>
    public class TrailBudget
    {
        private readonly List<float>[] _expiry =
        {
            new List<float>(24), new List<float>(24), new List<float>(24)
        };

        private readonly int _perInk;
        private readonly int _total;

        public TrailBudget(int perInk, int total)
        {
            _perInk = perInk;
            _total = total;
        }

        public int Alive(TrailInk ink, float now)
        {
            Prune(now);
            return _expiry[(int)ink].Count;
        }

        public int TotalAlive(float now)
        {
            Prune(now);
            return _expiry[0].Count + _expiry[1].Count + _expiry[2].Count;
        }

        /// <summary>Yer varsa kaydeder ve true döner; doluysa false (doğum atlanır).</summary>
        public bool TryReserve(TrailInk ink, float now, float lifetime)
        {
            Prune(now);
            if (_expiry[(int)ink].Count >= _perInk) return false;
            if (_expiry[0].Count + _expiry[1].Count + _expiry[2].Count >= _total) return false;
            _expiry[(int)ink].Add(now + lifetime);
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _expiry.Length; i++) _expiry[i].Clear();
        }

        private void Prune(float now)
        {
            // Lambda'sız (kare başı GC üretmesin)
            for (int i = 0; i < _expiry.Length; i++)
            {
                List<float> list = _expiry[i];
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    if (list[j] <= now) list.RemoveAt(j);
                }
            }
        }
    }
}
