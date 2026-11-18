using System;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Fiziksel tetikleyici alana giren IKillable arayüzüne sahip nesneleri tespit edip ölüm komutunu iletir.
    /// Killing, Kill()'den önce (girenin hızı ve konumu henüz sıfırlanmamışken), Killed ise sonra yayınlanır.
    /// Dinleyici hata atsa bile ölüm gerçekleşir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        public event Action<Collider2D> Killing;
        public event Action<Collider2D> Killed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IKillable killableEntity))
            {
                try { Killing?.Invoke(other); }
                finally { killableEntity.Kill(); }
                Killed?.Invoke(other);
            }
        }
    }
}
