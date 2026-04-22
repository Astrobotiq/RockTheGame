using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Fiziksel tetikleyici alana giren IKillable arayüzüne sahip nesneleri tespit edip ölüm komutunu iletir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IKillable killableEntity))
            {
                killableEntity.Kill();
            }
        }
    }
}