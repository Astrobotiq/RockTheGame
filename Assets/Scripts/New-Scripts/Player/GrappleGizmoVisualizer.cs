using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Sahnedeki kanca dugum noktalarini layer bazli tarayarak swing alanlarini gorsellestirir.
    /// </summary>
    public class GrappleGizmoVisualizer : MonoBehaviour
    {
        [SerializeField] private LayerMask grappleNodeLayer;
        [SerializeField] private Color gizmoColor = Color.cyan;
        [SerializeField] private float swingRadius = 5f;

        private void OnDrawGizmos()
        {
            Collider2D[] nodes = Physics2D.OverlapCircleAll(Vector2.zero, float.MaxValue, grappleNodeLayer);

            Gizmos.color = gizmoColor;

            foreach (Collider2D node in nodes)
            {
                Gizmos.DrawWireSphere(node.transform.position, swingRadius);
                Gizmos.DrawIcon(node.transform.position, "sv_label_0", true);
            }
        }
    }
}