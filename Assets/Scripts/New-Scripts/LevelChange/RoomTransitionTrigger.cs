using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private Room targetRoom;
        [SerializeField] private RoomTransitionCoordinator coordinator;
        [SerializeField] private Checkpoint associatedCheckpoint;

        [Header("Transition")] [SerializeField]
        private TransitionDirection direction = TransitionDirection.Right;

        private BoxCollider2D triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider2D>();
            triggerCollider.enabled = false;
        }

        public void Enable() => triggerCollider.enabled = true;
        public void Disable() => triggerCollider.enabled = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IPlayerTransitionable player))
            {
                coordinator.ExecuteTransition(
                    player,
                    targetRoom,
                    targetRoom.RoomBounds,
                    transform.position,
                    direction,
                    targetRoom.OverrideCameraSize,
                    targetRoom.OverrideDynamicZoom
                );

                if (associatedCheckpoint != null)
                    associatedCheckpoint.ActivateCheckpoint();
                else
                    Debug.LogWarning(
                        "Warning: No checkpoint associated with this transition trigger. Consider assigning one to ensure proper respawn behavior.");
            }
        }
    }
}