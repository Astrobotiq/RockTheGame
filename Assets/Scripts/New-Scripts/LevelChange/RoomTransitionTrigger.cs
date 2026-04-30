using New_Scripts.LevelChange;
using UnityEngine;

/// <summary>
/// Odalar arası geçişi algılayan ve koordinatöre geçiş verilerini ileten tetikleyici sınıftır.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RoomTransitionTrigger : MonoBehaviour
{
    [SerializeField] private Collider2D targetRoomBounds;
    [SerializeField] private Transform targetSpawnPoint;
    [SerializeField] private RoomTransitionCoordinator coordinator;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IPlayerTransitionable player))
        {
            coordinator.ExecuteTransition(player, targetRoomBounds, targetSpawnPoint.position);
        }
    }
}