using UnityEngine;

namespace New_Scripts.NPC
{
    /// <summary>Diyalog levhasının durduğu ekran tarafı.</summary>
    public enum SlabSide
    {
        Left,
        Right
    }

    /// <summary>
    /// Diyalog levhasının oyuncuyu kapatmamasını sağlayan saf mantık (EditMode'da test edilir). Koordinatlar viewport (0..1).
    /// Levha varsayılan olarak sol altta durur; oyuncu levhanın yarısına girerse karşı köşeye geçer
    /// (histerezisle, orta çizgide gidip gelirken titremesin), yine de kapanırsa arka planı soluklaşır.
    /// </summary>
    public static class DialogueSlabMath
    {
        /// <summary>
        /// Levhanın duracağı taraf. Mevcut taraf <c>Left</c> iken oyuncu orta çizgiyi <paramref name="hysteresis"/> kadar sola
        /// geçince <c>Right</c>'a; <c>Right</c> iken aynı kadar sağa geçince <c>Left</c>'e döner. Arada mevcut taraf korunur.
        /// </summary>
        public static SlabSide ResolveSide(float playerViewportX, SlabSide current, float hysteresis)
        {
            hysteresis = Mathf.Max(0f, hysteresis);
            if (current == SlabSide.Left && playerViewportX < 0.5f - hysteresis) return SlabSide.Right;
            if (current == SlabSide.Right && playerViewportX > 0.5f + hysteresis) return SlabSide.Left;
            return current;
        }

        /// <summary>Oyuncu noktası, <paramref name="clearance"/> kadar genişletilmiş levha dikdörtgeninin içinde mi.</summary>
        public static bool Overlaps(Rect slabViewport, Vector2 playerViewport, float clearance)
        {
            clearance = Mathf.Max(0f, clearance);
            var expanded = new Rect(
                slabViewport.xMin - clearance, slabViewport.yMin - clearance,
                slabViewport.width + clearance * 2f, slabViewport.height + clearance * 2f);
            return expanded.Contains(playerViewport);
        }
    }
}
