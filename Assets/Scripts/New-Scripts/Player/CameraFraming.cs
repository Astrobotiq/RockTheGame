using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// <see cref="CameraFocusType.FrameArea"/> için saf hesap: verilen dünya dikdörtgenini
    /// ekrana sığdıran kamera merkezi ve ortho boyu. Sahneye ya da kameraya bağımlı değil.
    /// </summary>
    public static class CameraFraming
    {
        private const float DefaultAspect = 16f / 9f;

        /// <summary>
        /// Çerçevenin merkezini ve onu (kenar payıyla) kadraja sığdıran ortho boyunu (yarı yükseklik) döndürür.
        /// size = max(h/2, w/2/aspect) + padding. Geçersiz en-boy oranı 16:9'a, negatif padding 0'a düşer;
        /// negatif/sıfır boyutlu çerçeve yalnızca padding kadar boy verir.
        /// </summary>
        public static (Vector2 center, float size) Fit(Rect worldFrame, float aspect, float padding)
        {
            if (aspect <= 0f || float.IsNaN(aspect)) aspect = DefaultAspect;
            if (padding < 0f || float.IsNaN(padding)) padding = 0f;

            float halfWidth = Mathf.Abs(worldFrame.width) * 0.5f;
            float halfHeight = Mathf.Abs(worldFrame.height) * 0.5f;

            float size = Mathf.Max(halfHeight, halfWidth / aspect) + padding;
            return (worldFrame.center, size);
        }

        /// <summary>
        /// Bölge transform'una göre yerel çerçeveyi dünya dikdörtgenine çevirir (iki köşe dönüştürülür;
        /// 90°'nin katı olmayan dönüşte eksen hizalı sınır kutusu kalır).
        /// </summary>
        public static Rect ToWorldRect(Transform space, Rect localFrame)
        {
            if (space == null) return localFrame;

            Vector2 a = space.TransformPoint(new Vector3(localFrame.xMin, localFrame.yMin, 0f));
            Vector2 b = space.TransformPoint(new Vector3(localFrame.xMax, localFrame.yMax, 0f));
            Vector2 min = Vector2.Min(a, b);
            Vector2 max = Vector2.Max(a, b);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
