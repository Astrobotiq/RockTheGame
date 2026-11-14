using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Taşıyan yüzeyden (zemin ya da tutunulan duvar) ayrılırken oyuncuya aktarılacak hızın saf hesabı.
    /// Kural: docs/systems/platforms.md "Ayrılırken hız aktarımı".
    /// </summary>
    public static class SurfaceCarry
    {
        /// <summary>
        /// Yüzeyin bu fizik adımındaki hızı (<c>DeltaPosition / dt</c>). <c>SurfaceVelocity</c> kullanılmaz:
        /// o son 3 adımın en büyüğüdür ve boost için tasarlanmıştır.
        /// </summary>
        public static Vector2 Velocity(IMovingSurface surface, float fixedDeltaTime)
        {
            if (surface == null || fixedDeltaTime <= 0f) return Vector2.zero;
            return surface.DeltaPosition / fixedDeltaTime;
        }

        /// <summary>
        /// Zıplamada eklenecek hız. <c>JumpBoostMultiplier &gt; 0</c> ise eski formül
        /// (<c>SurfaceVelocity × çarpan</c>, <paramref name="boosted"/> true) taban hızın yerine geçer;
        /// değilse <see cref="Velocity"/> ×1 döner.
        /// </summary>
        public static Vector2 JumpAddition(IMovingSurface surface, float fixedDeltaTime, out bool boosted)
        {
            boosted = surface != null && surface.JumpBoostMultiplier > 0f;
            if (boosted) return surface.SurfaceVelocity * surface.JumpBoostMultiplier;
            return Velocity(surface, fixedDeltaTime);
        }
    }
}
