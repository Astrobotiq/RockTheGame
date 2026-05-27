using UnityEngine;
using UnityEngine.Events;

namespace New_Scripts.Player.Nodes.Rotation
{
    /// <summary>
    /// Bir Node etrafında tam tur dönüldüğünde event fırlatan tetikleyici.
    /// Kapı açmak, sandık düşürmek veya köprü indirmek için kullanılabilir.
    /// </summary>
    public class RotationEventTrigger : MonoBehaviour, IFullRotationEffect
    {
        [Tooltip("360 derece dönme işlemi tamamlandığında çağrılacak olaylar.")]
        public UnityEvent OnRotationCompleted;

        public void OnFullRotationCompleted()
        {
            // Event'i fırlat.
            OnRotationCompleted?.Invoke();
        }
    }
}