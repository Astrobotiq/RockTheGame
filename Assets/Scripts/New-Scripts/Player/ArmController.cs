using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Girdi yönüne göre görsel açıyı hesaplayan ve uygulayan pasif kol kontrolcüsü.
    /// </summary>

    public class ArmController : MonoBehaviour
    {
        [SerializeField] private Transform rotationPivot;

        public void UpdateArmRotation(Vector2 inputDirection)
        {
            if (inputDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;
                rotationPivot.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            }
        }
    }
}