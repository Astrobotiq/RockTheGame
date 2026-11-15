using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Platformun hareket stratejisini tanımlayan soyut temel sınıf.
    /// </summary>
    public abstract class MovementStrategy : MonoBehaviour
    {
        public abstract Vector2 GetPositionAtTime(float time);

        /// <summary>
        /// Platformun bir tam tur (periyot) süresini döndürür.
        /// </summary>
        public virtual float Period => 0f;

        /// <summary>
        /// Platformdan zıplarken oyuncuya uygulanacak ek hız çarpanı.
        /// Boost istemeyen stratejiler için 0f döner.
        /// </summary>
        public virtual float JumpBoostMultiplier => 0f;

        /// <summary>
        /// Platformun belirli bir zamandaki kinematic rotasyonunu (z açısı cinsinden) döndürür.
        /// Rotasyonu kontrol etmeyen stratejiler için null döner.
        /// </summary>
        public virtual float? GetRotationAtTime(float time)
        {
            return null;
        }

        /// <summary>
        /// Platform merkezinin gidebileceği dünya dikdörtgeni (editör: kamera çerçevesini tehlikeye sığdırmak için).
        /// Platformun kendi boyutunu içermez. Desteklemeyen strateji false döner.
        /// </summary>
        public virtual bool TryGetPathBounds(out Rect worldBounds)
        {
            worldBounds = default;
            return false;
        }

        protected static Rect RectFromPoints(Vector2 a, Vector2 b)
        {
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }
    }
}