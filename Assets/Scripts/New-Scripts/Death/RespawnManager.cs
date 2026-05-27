using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Dinamik kayıt noktası olaylarını bir ScriptableObject kanalından dinleyerek yeniden doğma dizilimini yönetir.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Transform _initialSpawnPoint;
        [SerializeField] private TransitionManager _transitionManager;
        [SerializeField] private TransformEventChannelSO _checkpointActivatedChannel;
        [SerializeField] private float _cameraCatchUpDelay = 1f;
    
        private Transform _currentRespawnPoint;
        private PlayerController _playerController;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _currentRespawnPoint = _initialSpawnPoint;

            if (_playerHealth != null)
            {
                _playerController = _playerHealth.GetComponent<PlayerController>();
            }
        }

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();

            if (_playerHealth != null)
            {
                _playerHealth.OnDeath += HandlePlayerRespawn;
            }

            if (_checkpointActivatedChannel != null)
            {
                _checkpointActivatedChannel.OnEventRaised += UpdateRespawnPoint;
            }
        }

        private void OnDisable()
        {
            CancelTasks();

            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= HandlePlayerRespawn;
            }

            if (_checkpointActivatedChannel != null)
            {
                _checkpointActivatedChannel.OnEventRaised -= UpdateRespawnPoint;
            }
        }

        private void UpdateRespawnPoint(Transform newPoint)
        {
            Debug.Log($"Yeni kayıt noktası {newPoint.gameObject.name}: " + newPoint.position);
            _currentRespawnPoint = newPoint;
        }

        private void CancelTasks()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void HandlePlayerRespawn()
        {
            RespawnSequenceAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid RespawnSequenceAsync(CancellationToken token)
        {
            if (_playerController != null)
            {
                _playerController.OnStartRespawn();
            }
            
            await _transitionManager.PlayCloseTransitionAsync(_playerHealth.transform.position, token);

            _playerHealth.transform.position = _currentRespawnPoint.position;
        
            if (_playerController != null)
            {
                _playerController.Velocity = Vector2.zero;
            }

            _playerHealth.gameObject.SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(_cameraCatchUpDelay), cancellationToken: token);

            await _transitionManager.PlayOpenTransitionAsync(_currentRespawnPoint.position, token);
            
            if (_playerController != null)
            {
                _playerController.OnEndRespawn();
            }
        }
        
        public void OverrideInitialSpawnPoint(Transform newSpawnPoint)
        {
            _initialSpawnPoint = newSpawnPoint;
            _currentRespawnPoint = newSpawnPoint;
            Debug.Log($"Başlangıç noktası {newSpawnPoint.gameObject.name} olarak güncellendi.");
        }
    }
}