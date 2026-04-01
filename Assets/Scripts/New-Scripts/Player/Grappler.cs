using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Kanca ucunda bulunan, hedeflenen katmanlarla teması algılayıp ana sisteme rapor veren sensör bileşeni.
    /// </summary>

    [RequireComponent(typeof(Collider2D))]
    public class Grappler : MonoBehaviour
    {
        [SerializeField] private LayerMask grappleTargetLayer;

        public bool HasAttached { get; private set; }
        public Vector2 AttachPoint { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & grappleTargetLayer) != 0)
            {
                HasAttached = true;
                AttachPoint = transform.position;
            }
        }

        public void Detach()
        {
            HasAttached = false;
            AttachPoint = Vector2.zero;
        }
    }
}