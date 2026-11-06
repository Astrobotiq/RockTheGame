using New_Scripts.LevelChange;
using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>Bayrak işaretinin varsayılan yeri: geçiş trigger'ının oyuncunun geldiği tarafında, Margin kadar geride.</summary>
    public static class RaceFlagPlacement
    {
        public const float Margin = 1.5f;
        /// <summary>Yatay çıkışta işaret en fazla bu kadar aşağıdaki zemine indirilir; daha uzaksa trigger yüksekliğinde kalır.</summary>
        public const float MaxGroundDrop = 6f;

        public static Vector2 InFrontOf(Bounds trigger, TransitionDirection direction)
        {
            switch (direction)
            {
                case TransitionDirection.Right: return new Vector2(trigger.min.x - Margin, trigger.center.y);
                case TransitionDirection.Left: return new Vector2(trigger.max.x + Margin, trigger.center.y);
                case TransitionDirection.Up: return new Vector2(trigger.center.x, trigger.min.y - Margin);
                default: return new Vector2(trigger.center.x, trigger.max.y + Margin);
            }
        }

        /// <summary>
        /// Collider'ın dünya sınırları. BoxCollider2D için transform'dan hesaplanır: fizik senkronu beklemez,
        /// editörde yeni oluşturulmuş ya da kapalı nesnede de doğru.
        /// </summary>
        public static Bounds WorldBounds(Collider2D collider)
        {
            if (collider is BoxCollider2D box)
            {
                var t = box.transform;
                Vector3 center = t.TransformPoint(box.offset);
                Vector3 size = Vector3.Scale(new Vector3(box.size.x, box.size.y, 0f), t.lossyScale);
                return new Bounds(center, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 0f));
            }
            return collider.bounds;
        }
    }
}
