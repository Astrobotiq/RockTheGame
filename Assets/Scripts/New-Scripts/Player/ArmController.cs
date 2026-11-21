using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Omuz konumu ve nişan yönünü tutan pasif kol kontrolcüsü. Çizimi <c>Visual/ArmVisual</c> yapar.
    /// </summary>
    public class ArmController : MonoBehaviour
    {
        /// <summary>Son nişan yönü (normalize). Girdi yoksa <see cref="Vector2.zero"/>.</summary>
        public Vector2 AimDirection { get; private set; }

        public void UpdateArmRotation(Vector2 inputDirection)
        {
            AimDirection = inputDirection.sqrMagnitude > 0.01f ? inputDirection.normalized : Vector2.zero;
        }
    }
}
