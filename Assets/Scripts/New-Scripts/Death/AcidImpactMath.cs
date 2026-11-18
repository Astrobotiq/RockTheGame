using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asit temas tepkisinin saf hesapları (AcidReaction kullanır, EditMode'da test edilir).
    /// Piksel koordinatı = asit shader'ının ızgarası: (0,0) sol alt, (_PixelScale.x, _PixelScale.y) sağ üst.
    /// </summary>
    public static class AcidImpactMath
    {
        /// <summary>
        /// Trigger fizik adımından sonra gelir; oyuncu merkezi çoğu zaman asidin içindedir.
        /// Bir adım geri sararak girişten önceki noktayı verir.
        /// </summary>
        public static Vector2 BacktrackPoint(Vector2 position, Vector2 velocity, float dt)
        {
            return position - velocity * dt;
        }

        /// <summary>
        /// Asidin yerel uzayındaki bir noktayı shader'ın piksel ızgarasına çevirir.
        /// Sonuç [0.5, pixelScale - 0.5] aralığına sıkıştırılır (piksel merkezleri).
        /// </summary>
        public static Vector2 LocalToPixel(Vector2 local, Rect spriteLocalRect, Vector2 pixelScale)
        {
            float w = Mathf.Max(spriteLocalRect.width, 1e-5f);
            float h = Mathf.Max(spriteLocalRect.height, 1e-5f);
            float px = (local.x - spriteLocalRect.xMin) / w * pixelScale.x;
            float py = (local.y - spriteLocalRect.yMin) / h * pixelScale.y;
            return new Vector2(ClampToPixels(px, pixelScale.x), ClampToPixels(py, pixelScale.y));
        }

        /// <summary>Yüzeyli asitte temas, yüzey satırına kilitlenir; yüzeysizde (akan asit) dokunulmaz.</summary>
        public static Vector2 SurfaceContact(Vector2 pixel, float acidLevel, float pixelScaleY, bool hasSurface)
        {
            if (!hasSurface) return pixel;
            return new Vector2(pixel.x, Mathf.Floor(Mathf.Clamp01(acidLevel) * pixelScaleY));
        }

        /// <summary>Giriş hızına göre tepki gücü: yavaşta floor, maxSpeed ve üstünde 1.</summary>
        public static float Strength(float speed, float minSpeed, float maxSpeed, float floor)
        {
            return Mathf.Lerp(floor, 1f, Mathf.InverseLerp(minSpeed, maxSpeed, speed));
        }

        /// <summary>Ortam dalga tepesinin üstünde, quad'ın tepesine kadar kalan piksel payı.</summary>
        public static float CrestHeadroom(float pixelScaleY, float acidLevel, float waveAmplitude)
        {
            return Mathf.Max(0f, pixelScaleY * (1f - acidLevel) - waveAmplitude - 1f);
        }

        /// <summary>
        /// Bir damlanın hızı: normal etrafında ±coneDeg içinde (random01 0..1 açıyı seçer),
        /// oyuncunun yatay hızının inheritX katı yana, düşey hız büyüklüğünün inheritY katı normale eklenir.
        /// Büyüklük maxSpeed ile sınırlanır (damlalar iris dairesinden kaçmasın).
        /// </summary>
        public static Vector2 SplashVelocity(Vector2 normal, float coneDeg, float random01, float speed,
            float strength, Vector2 playerVelocity, float inheritX, float inheritY, float maxSpeed)
        {
            Vector2 n = normal.sqrMagnitude > 1e-6f ? normal.normalized : Vector2.up;
            float angle = Mathf.Lerp(-coneDeg, coneDeg, Mathf.Clamp01(random01)) * Mathf.Deg2Rad;
            float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
            Vector2 dir = new Vector2(n.x * c - n.y * s, n.x * s + n.y * c);

            Vector2 v = dir * (speed * strength)
                        + new Vector2(playerVelocity.x * inheritX, 0f)
                        + n * (Mathf.Abs(playerVelocity.y) * inheritY);
            return Vector2.ClampMagnitude(v, maxSpeed);
        }

        /// <summary>Darbe yuvası canlı mı? Shader da aynı koşulu kullanır; değilse etki tam 0.</summary>
        public static bool ImpactLive(float age, float duration, float strength)
        {
            return age >= 0f && age < duration && strength > 0f;
        }

        private static float ClampToPixels(float value, float size)
        {
            if (size <= 1f) return size * 0.5f;
            return Mathf.Clamp(value, 0.5f, size - 0.5f);
        }
    }
}
