using UnityEngine;

namespace New_Scripts.LevelChange
{
    /// <summary>
    /// Odalar arası geçişi algılayan ve koordinatöre geçiş verilerini ileten tetikleyici sınıftır.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private Collider2D targetRoomBounds;
        [SerializeField] private Transform targetSpawnPoint;
        
        [Header("Camera Settings")]
        [SerializeField] private bool overrideDynamicZoom = false; // True ise dinamik zoom kapanır
        [SerializeField] private float overrideCameraSize = 8f; // Sadece override true ise kullanılır

        [SerializeField] private RoomTransitionCoordinator coordinator;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IPlayerTransitionable player))
            {
                // Artık geçişe override bilgisini de yolluyoruz
                coordinator.ExecuteTransition(player, targetRoomBounds, targetSpawnPoint.position, overrideCameraSize, overrideDynamicZoom);
            }
        }
    }
}