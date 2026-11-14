using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Tirmanirken/kayarken duvara yapisma hesabi (docs/systems/wall-movement.md "Duvara yapisma").
    /// </summary>
    public static class WallClingMath
    {
        /// <summary>
        /// Duvar ile collider arasindaki bosluk (gap) tolerans icindeyse ve guvenlik payindan buyukse,
        /// bosluğu bu fizik adiminda kapatacak X hiz buyuklugunu verir; degilse 0.
        /// KinematicPhysicsHandler carpismayi kendisi cozdugu icin hiz duvari asmaz.
        /// </summary>
        public static float ClosingSpeed(float gap, float safetyOffset, float tolerance, float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f) return 0f;
            if (gap > tolerance) return 0f;
            float distance = gap - safetyOffset;
            return distance > 0f ? distance / fixedDeltaTime : 0f;
        }
    }
}
