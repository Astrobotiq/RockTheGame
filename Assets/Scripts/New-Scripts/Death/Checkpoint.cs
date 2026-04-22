using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Oyuncunun temasını dinler ve yeniden doğma noktasını bağımsız bir olay kanalı üzerinden yayınlar.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private TransformEventChannelSO _checkpointActivatedChannel;
    
        private bool _isActivated;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isActivated) return;

            if (other.TryGetComponent(out PlayerHealth _))
            {
                _isActivated = true;
            
                if (_checkpointActivatedChannel != null)
                {
                    _checkpointActivatedChannel.RaiseEvent(transform);
                }
            }
        }
    }
}