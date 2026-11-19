using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Ölüm efektinin (karakterin kendi pikselleri) ve asitte kalan kafaların saf hesapları
    /// (DeathBurst, AcidHeadPool kullanır; EditMode'da test edilir).
    /// </summary>
    public static class DeathBurstMath
    {
        /// <summary>Bir hücre: dolu piksellerin ortalama konumu (sprite dikdörtgeninde piksel cinsinden) ve rengi.</summary>
        public struct PixelCell
        {
            public Vector2 Center;
            public Color32 Color;
        }

        /// <summary>Efektin 0..1 ilerlemesi: hitstop boyunca 0, sonra duration'a kadar doğrusal.</summary>
        public static float Progress(float t, float hitstop, float duration)
        {
            return Mathf.Clamp01((t - hitstop) / Mathf.Max(duration - hitstop, 1e-4f));
        }

        /// <summary>
        /// Parçacığın konumu: üstel yavaşlayan hız (toplam yol = v0 / drag) + gecikmeli yerçekimi.
        /// movingTime hitstop düşüldükten sonraki süredir; &lt;= 0 ise parçacık başlangıçta durur.
        /// </summary>
        public static Vector2 Position(Vector2 origin, Vector2 v0, float drag, float gravity, float gravityDelay, float movingTime)
        {
            if (movingTime <= 0f) return origin;
            float f = drag > 1e-4f ? (1f - Mathf.Exp(-drag * movingTime)) / drag : movingTime;
            float gt = Mathf.Max(0f, movingTime - gravityDelay);
            return origin + v0 * f + new Vector2(0f, -0.5f * gravity * gt * gt);
        }

        /// <summary>Başlangıç hızının büyüklüğü: merkezden uzaktakiler hızlı. dist01 0 merkez, 1 uç.</summary>
        public static float ParticleSpeed(float spread, float drag, float dist01, float jitter)
        {
            return spread * drag * (0.5f + 0.5f * Mathf.Clamp01(dist01)) * jitter;
        }

        /// <summary>Piksel ızgarasına yuvarlar (1 / ppu birim).</summary>
        public static float Snap(float value, float ppu)
        {
            return ppu > 0f ? Mathf.Round(value * ppu) / ppu : value;
        }

        public static Vector2 Snap(Vector2 value, float ppu)
        {
            return new Vector2(Snap(value.x, ppu), Snap(value.y, ppu));
        }

        /// <summary>
        /// Parçacık görünür mü? Ömrünün sonunda yanıp söner, sonra kaybolur. vanishAt parçacığın kaybolma payı (0.5..1).
        /// </summary>
        public static bool ParticleVisible(float progress, float vanishAt)
        {
            if (progress >= 1f) return false;
            float start = vanishAt * 0.85f;
            float vis = Mathf.Clamp01((progress - start) / Mathf.Max(1f - start, 1e-4f));
            if (vis >= 1f) return false;
            if (vis > 0f && Mathf.FloorToInt(vis * 10f) % 2 == 0) return false;
            return true;
        }

        /// <summary>
        /// Sprite'ın dolu piksellerini (alfa &gt;= alphaMin) cellPx × cellPx hücrelere böler; her hücrenin ortalama
        /// konumu ve rengi döner. Hücreler dolu alanın sol-alt köşesinden başlar. pixels satır satır, aşağıdan yukarı.
        /// </summary>
        public static List<PixelCell> BuildPixelCells(Color32[] pixels, int width, int height, int cellPx, byte alphaMin)
        {
            var result = new List<PixelCell>();
            if (pixels == null || width <= 0 || height <= 0 || pixels.Length < width * height) return result;
            cellPx = Mathf.Max(1, cellPx);

            int minX = int.MaxValue, minY = int.MaxValue;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[y * width + x].a < alphaMin) continue;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                }
            }
            if (minX == int.MaxValue) return result;

            var sums = new Dictionary<long, double[]>(); // xs, ys, n, r, g, b
            for (int y = minY; y < height; y++)
            {
                for (int x = minX; x < width; x++)
                {
                    Color32 c = pixels[y * width + x];
                    if (c.a < alphaMin) continue;
                    long key = (long)((x - minX) / cellPx) * 100000L + (y - minY) / cellPx;
                    if (!sums.TryGetValue(key, out double[] s)) { s = new double[6]; sums[key] = s; }
                    s[0] += x + 0.5; s[1] += y + 0.5; s[2] += 1; s[3] += c.r; s[4] += c.g; s[5] += c.b;
                }
            }

            foreach (var s in sums.Values)
            {
                result.Add(new PixelCell
                {
                    Center = new Vector2((float)(s[0] / s[2]), (float)(s[1] / s[2])),
                    Color = new Color32((byte)(s[3] / s[2]), (byte)(s[4] / s[2]), (byte)(s[5] / s[2]), 255)
                });
            }
            return result;
        }

        /// <summary>
        /// Kafa/blok için solma adımı: 1 tam boyut, 0.5 yarım boyut, 0 gizli. blink true ise 0.55'ten sonra yanıp söner.
        /// progress 0..1, vanishAt kaybolma payı (0.78..1).
        /// </summary>
        public static float BlockScale(float progress, bool blink, float vanishAt)
        {
            if (progress >= 1f) return 0f;
            if (blink && progress > 0.55f && Mathf.FloorToInt(progress * 28f) % 2 == 0) return 0f;
            float start = vanishAt * 0.85f;
            if (progress >= start)
            {
                float s = (progress - start) / Mathf.Max(1f - start, 1e-4f);
                if (s >= 1f) return 0f;
                if (s > 0.5f) return 0.5f;
            }
            return 1f;
        }

        /// <summary>Çözülen (en eski) kafa için: yanıp söner, ikinci yarıda yarım boyut, sonunda 0.</summary>
        public static float DissolveScale(float progress)
        {
            if (progress >= 1f) return 0f;
            if (Mathf.FloorToInt(progress * 12f) % 2 == 1) return 0f;
            return progress > 0.5f ? 0.5f : 1f;
        }

        // --- Asit dışı ölümlerde kalan kafalar (fizik) ---

        /// <summary>Kafanın fırlatma hızı: angleRad yönünde spread × drag × factor + oyuncudan geçen hız.</summary>
        public static Vector2 HeadLaunch(float spread, float drag, float factor, float angleRad, Vector2 inherit)
        {
            return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * (spread * drag * factor) + inherit;
        }

        /// <summary>
        /// Oyuncu kafaya değince kafaya verilen itme: oyuncudan kafaya doğru (hafif yukarı eğimli) pushSpeed,
        /// artı oyuncunun hızından bir pay. Büyüklük maxSpeed ile sınırlı.
        /// </summary>
        public static Vector2 HeadPush(Vector2 fromPlayerToHead, float pushSpeed, Vector2 playerVelocity, float inherit, float maxSpeed)
        {
            Vector2 dir = fromPlayerToHead.sqrMagnitude > 1e-6f ? fromPlayerToHead : Vector2.up;
            dir = (dir.normalized + Vector2.up * 0.35f).normalized;
            return Vector2.ClampMagnitude(dir * pushSpeed + playerVelocity * inherit, maxSpeed);
        }

        /// <summary>
        /// Dash vuruşu: kafa ağırlıklı olarak dash yönünde (%75) ve oyuncudan yatay olarak uzağa (%25) gider, yatay bileşenle orantılı
        /// <paramref name="liftDeg"/> derece yukarı eğimlidir (dikey dash'te eğim eklenmez). Hız speed, üst sınır maxSpeed.
        /// </summary>
        public static Vector2 DashPush(Vector2 playerVelocity, Vector2 fromPlayerToHead, float speed, float liftDeg, float maxSpeed)
        {
            // Oyuncunun merkezi kafanın merkezinden yüksekte olduğu için "oyuncudan kafaya" vektörünün düşeyi hep aşağı bakar
            // ve yukarı eğimi eritir; bu yüzden yalnızca yatay yönü kullanılır.
            Vector2 away = Mathf.Abs(fromPlayerToHead.x) > 1e-3f ? new Vector2(Mathf.Sign(fromPlayerToHead.x), 0f) : Vector2.zero;
            Vector2 dash = playerVelocity.sqrMagnitude > 1e-6f ? playerVelocity.normalized : (away != Vector2.zero ? away : Vector2.right);
            Vector2 dir = (dash * 0.75f + away * 0.25f);
            dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : dash;
            dir.y += Mathf.Tan(liftDeg * Mathf.Deg2Rad) * Mathf.Abs(dir.x);
            return Vector2.ClampMagnitude(dir.normalized * speed, maxSpeed);
        }

        /// <summary>Hızı ekler ama mevcut hızdan büyük bir sınırı (max) aşmasına izin vermez (düşen kafa kırpılmaz).</summary>
        public static Vector2 AddCapped(Vector2 velocity, Vector2 add, float maxSpeed)
        {
            return Vector2.ClampMagnitude(velocity + add, Mathf.Max(maxSpeed, velocity.magnitude));
        }

        // --- Kafa squash and stretch ---

        /// <summary>
        /// Hıza göre uzama: hızlı eksen uzar, diğer eksen hafif daralır (hacim yaklaşık korunur).
        /// perSpeed: birim hız başına uzama, max: bir eksendeki en büyük uzama.
        /// </summary>
        public static Vector2 StretchScale(Vector2 velocity, float perSpeed, float max)
        {
            float ex = Mathf.Min(Mathf.Abs(velocity.x) * perSpeed, max);
            float ey = Mathf.Min(Mathf.Abs(velocity.y) * perSpeed, max);
            return new Vector2(1f + ex - 0.5f * ey, 1f + ey - 0.5f * ex);
        }

        /// <summary>Çarpma yassılmasının zamana göre genliği: yaylanarak söner (genlik * e^(-decay t) * cos(freq t)).</summary>
        public static float SquashAmountAt(float amount, float t, float freq, float decay)
        {
            if (t < 0f) return 0f;
            return amount * Mathf.Exp(-decay * t) * Mathf.Cos(freq * t);
        }

        /// <summary>
        /// Çarpma ölçeği: a &gt; 0 çarpma ekseninde yassılır, dik eksende genişler; a &lt; 0 (yaylanma) tersi.
        /// vertical true ise çarpma düşey eksendedir (zemin), değilse yatay (duvar, itme).
        /// </summary>
        public static Vector2 SquashScale(float a, bool vertical)
        {
            return vertical ? new Vector2(1f + a * 0.6f, 1f - a) : new Vector2(1f - a, 1f + a * 0.6f);
        }

        /// <summary>İki ölçeği çarpar ve aşırıya kaçmasın diye [min, max] aralığına sıkıştırır.</summary>
        public static Vector2 CombineScale(Vector2 a, Vector2 b, float min, float max)
        {
            return new Vector2(Mathf.Clamp(a.x * b.x, min, max), Mathf.Clamp(a.y * b.y, min, max));
        }

        /// <summary>Ölçeği sprite pikseline oturtur: genişlik/yükseklik tam piksel olur (pixel art bozulmaz).</summary>
        public static Vector2 QuantizeScale(Vector2 scale, int spritePixels)
        {
            if (spritePixels <= 0) return scale;
            return new Vector2(Mathf.Round(scale.x * spritePixels) / spritePixels, Mathf.Round(scale.y * spritePixels) / spritePixels);
        }

        // --- Asitte kalan kafalar ---

        /// <summary>Havuzun genişliğine sığan kafa yuvası sayısı (en az 1). Yuvalar soldan başlar, kafa kenara taşmaz.</summary>
        public static int SlotCount(float width, float headSize, float spacing)
        {
            return Mathf.Max(1, Mathf.FloorToInt((width - headSize) / Mathf.Max(spacing, 1e-3f)) + 1);
        }

        /// <summary>Verilen x'e en yakın yuva indeksi (0..count-1).</summary>
        public static int SlotNear(float x, float left, float headSize, float spacing, int count)
        {
            int i = Mathf.RoundToInt((x - (left + headSize * 0.5f)) / Mathf.Max(spacing, 1e-3f));
            return Mathf.Clamp(i, 0, Mathf.Max(0, count - 1));
        }

        /// <summary>Yuvanın dünya x'i.</summary>
        public static float SlotX(int index, float left, float headSize, float spacing)
        {
            return left + headSize * 0.5f + index * spacing;
        }

        /// <summary>Tercih edilen yuvadan dışa (p, p+1, p-1, p+2 ...) ilk boş yuva; hepsi doluysa -1.</summary>
        public static int PickSlotNear(IReadOnlyList<bool> used, int preferred)
        {
            int n = used.Count;
            if (n == 0) return -1;
            preferred = Mathf.Clamp(preferred, 0, n - 1);
            for (int o = 0; o < n; o++)
            {
                int a = preferred + o;
                if (a < n && !used[a]) return a;
                int b = preferred - o;
                if (o > 0 && b >= 0 && !used[b]) return b;
            }
            return -1;
        }

        /// <summary>Kafanın duracağı yükseklik: yüzeyin depth * fraction altı.</summary>
        public static float RestY(float surfaceY, float depth, float fraction)
        {
            return surfaceY - depth * fraction;
        }

        /// <summary>Asit tonu payı: derindeki kafa daha yeşil; batmaya başlayınca yavaşça oturur.</summary>
        public static float HeadTint(float fraction, float shallow, float deep, float t, float settleSeconds)
        {
            return Mathf.Lerp(shallow, deep, Mathf.Clamp01(fraction)) * Mathf.Clamp01(t / Mathf.Max(settleSeconds, 1e-3f));
        }

        /// <summary>Yumuşak duruş: x = target + (start - target) * exp(-rate * t).</summary>
        public static float EaseToward(float start, float target, float rate, float t)
        {
            if (t <= 0f) return start;
            return target + (start - target) * Mathf.Exp(-rate * t);
        }
    }
}
