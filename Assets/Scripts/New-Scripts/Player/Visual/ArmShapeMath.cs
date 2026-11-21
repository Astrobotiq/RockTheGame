using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Kol şeklinin saf matematiği: eğri noktaları, kavis, boştaki el hedefi, uzanma ilerlemesi.
    /// </summary>
    public static class ArmShapeMath
    {
        /// <summary>Kuadratik Bezier.</summary>
        public static Vector2 Bezier(Vector2 a, Vector2 control, Vector2 b, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * control + t * t * b;
        }

        /// <summary>Kol uzadıkça kavis azalır: idleBow × clamp01(idleReach / length).</summary>
        public static float BowForLength(float length, float idleReach, float idleBow)
        {
            if (length <= 0f) return idleBow;
            return idleBow * Mathf.Clamp01(idleReach / length);
        }

        /// <summary>
        /// Boştaki el hedefi. Nişan varsa o yönde; yoksa aşağı sarkar ve gövde hızının biraz gerisinde kalır.
        /// </summary>
        public static Vector2 IdleHandTarget(Vector2 shoulder, Vector2 aim, Vector2 bodyVelocity, float reach, float droopTrail)
        {
            if (aim.sqrMagnitude > 0.01f)
                return shoulder + aim.normalized * reach;
            Vector2 offset = Vector2.down * reach - bodyVelocity * droopTrail;
            return shoulder + Vector2.ClampMagnitude(offset, reach);
        }

        /// <summary>
        /// Yerdeyken el zeminde sürünür: yana açık, hızın tersine geride kalır. Yükseklik zemine sabit kalır,
        /// yalnızca yatay mesafe kol boyuyla sınırlanır.
        /// </summary>
        public static Vector2 GroundDragTarget(Vector2 shoulder, float side, float groundY, float handHeight,
            float spread, float velocityX, float trail, float reach)
        {
            float y = groundY + handHeight;
            float dy = y - shoulder.y;
            float maxDx = Mathf.Sqrt(Mathf.Max(0f, reach * reach - dy * dy));
            float dx = Mathf.Clamp(side * spread - velocityX * trail, -maxDx, maxDx);
            return new Vector2(shoulder.x + dx, y);
        }

        private const float GripPortion = 0.7f;

        /// <summary>
        /// Duvarda elin gövdeye göre yüksekliği. Faz 0..0.7 tutma (el yukarıdan aşağı iner, dünyada sabit kalır),
        /// 0.7..1 hızlıca yukarı uzanma.
        /// </summary>
        public static float WallHandOffsetY(float phase, float low, float high)
        {
            float t = Mathf.Repeat(phase, 1f);
            if (t < GripPortion) return high - (high - low) * (t / GripPortion);
            return low + (high - low) * ((t - GripPortion) / (1f - GripPortion));
        }

        public static bool IsWallGrip(float phase)
        {
            return Mathf.Repeat(phase, 1f) < GripPortion;
        }

        /// <summary>Gövde dy kadar yükselince fazı ilerletir; tutan el dünyada sabit kalır. Aşağı inerken faz geriler.</summary>
        public static float WallPhaseStep(float phase, float dy, float low, float high)
        {
            float range = high - low;
            if (range <= 1e-4f) return phase;
            return phase + dy / range * GripPortion;
        }

        /// <summary>Ease-out kübik: 1 − (1 − p)³.</summary>
        public static float ExtendProgress(float elapsed, float duration)
        {
            if (duration <= 0f) return 1f;
            float p = Mathf.Clamp01(elapsed / duration);
            float q = 1f - p;
            return 1f - q * q * q;
        }

        /// <summary>
        /// Uzanma yalnızca yeni bir tutunmada başlar: anchor yoktan var olunca, node değişince
        /// ya da node'suz noktanın kendisi değişince. Hareketli node'da anchor her adım değişse de yeniden başlamaz.
        /// </summary>
        public static bool IsNewGrab(bool wasActive, Transform prevNode, Transform node, Vector2? prevAnchor, Vector2 anchor)
        {
            return !wasActive || node != prevNode || (node == null && prevAnchor != anchor);
        }

        /// <summary>Gövde ışınlanınca el ekranı boydan boya katetmesin: mesafe idleReach × 4'ü aşarsa snap.</summary>
        public static bool ShouldSnap(Vector2 hand, Vector2 shoulder, float idleReach)
        {
            return (hand - shoulder).sqrMagnitude > (idleReach * 4f) * (idleReach * 4f);
        }
    }
}
