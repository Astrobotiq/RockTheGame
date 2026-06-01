using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Core
{
    /// <summary>
    /// Nesnelerin (özellikle oyuncunun) temasını tespit eden ve ConditionAction'a bildiren bileşen.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KinematicTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayerMask;
        
        public bool IsTriggered { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsInLayerMask(other.gameObject, playerLayerMask) || other.CompareTag("Player"))
            {
                IsTriggered = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsInLayerMask(other.gameObject, playerLayerMask) || other.CompareTag("Player"))
            {
                IsTriggered = false;
            }
        }

        public void ResetTrigger()
        {
            IsTriggered = false;
        }

        private bool IsInLayerMask(GameObject obj, LayerMask mask)
        {
            return (mask.value & (1 << obj.layer)) > 0;
        }
    }
}
