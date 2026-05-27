using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.Player.Nodes.Rotation
{
    /// <summary>
    /// Örnek efekt: 360 derece tamamlandığında karaktere hız patlaması verir.
    /// 
    /// Kendi efektini yazmak için bu sınıfı kopyala, IFullRotationEffect'i implemente et,
    /// FullRotationNode'un Inspector alanına bağla. Başka hiçbir şeye dokunma.
    /// </summary>
    public class SpeedBoostRotationEffect : MonoBehaviour, IFullRotationEffect
    {
        [SerializeField] private PlayerController playerContext;
        [SerializeField] private float boostMultiplier = 1.8f;
        [SerializeField] private float boostDuration = 1.2f;
        [SerializeField] private float velocityMultiplier = 1.2f;
 
        private CancellationTokenSource _cts;
 
        public void OnFullRotationCompleted()
        {
            Debug.Log("[SpeedBoostRotationEffect] 360 tamamlandı — hız patlaması!");
 
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
 
            ApplyBoostAsync(_cts.Token).Forget();
        }
 
        private async UniTaskVoid ApplyBoostAsync(CancellationToken ct)
        {
            playerContext.Velocity *= velocityMultiplier; 
            
            playerContext.ActiveSpeedMultiplier = boostMultiplier;
 
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(boostDuration),
                cancellationToken: ct
            );

            playerContext.ActiveSpeedMultiplier = 1f;
        }
 
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}