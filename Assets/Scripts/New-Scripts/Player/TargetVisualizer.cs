using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// FSM mantığından izole edilmiş, oyuncu girdisini ve NodeDetector sensörünü dinleyerek seçili hedeflerin üzerine görsel imleç (Reticle) yerleştiren sınıf.
    /// </summary>
    public class TargetVisualizer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerController playerContext;
        [SerializeField] private NodeDetector nodeDetector;

        [Header("Reticles (UI / Visuals)")]
        [SerializeField] private Transform leftReticle;
        [SerializeField] private Transform rightReticle;

        private void LateUpdate()
        {
            UpdateReticle(leftReticle, playerContext.Input.LeftStick, playerContext.LeftAnchor);
            UpdateReticle(rightReticle, playerContext.Input.RightStick, playerContext.RightAnchor);
        }

        private void UpdateReticle(Transform reticle, Vector2 aimDirection, Vector2? activeAnchor)
        {
            if (reticle == null) return;

            if (activeAnchor.HasValue)
            {
                if (reticle.gameObject.activeSelf) reticle.gameObject.SetActive(false);
                return;
            }

            if (aimDirection.sqrMagnitude > 0.01f && 
                nodeDetector.TryFindBestNode(aimDirection, playerContext.PlayerRigidbody.position, out Vector2 bestNodePos))
            {
                if (!reticle.gameObject.activeSelf) reticle.gameObject.SetActive(true);
                
                reticle.position = bestNodePos;
            }
            else
            {
                if (reticle.gameObject.activeSelf) reticle.gameObject.SetActive(false);
            }
        }
    }
}