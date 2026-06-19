using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Death;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Toplanabilir parçaların fiziksel, görsel ve durum yönetimini yapan ana bileşen.
    /// ScriptableObject Checkpoint olay kanalından ve PlayerHealth.OnDeath olayından haberdardır.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Oyuncu öldüğünde (checkpoint kaydı yoksa) bu parça geri gelsin mi?")]
        [SerializeField] private bool resetOnDeath = true;
        
        [Tooltip("Toplandıktan kısa süre sonra kendiliğinden yeniden doğsun mu? (örn: slingshot refill)")]
        [SerializeField] private bool respawnsAfterDelay = false;
        
        [Tooltip("Yeniden doğma gecikmesi süresi (saniye).")]
        [SerializeField] private float respawnDelay = 3f;

        [Tooltip("Toplandığında görsel nesne otomatik gizlensin mi?")]
        [SerializeField] private bool hideVisualOnCollect = true;

        [Header("References")]
        [Tooltip("Checkpoint tetiklenmelerini dinleyen kanal.")]
        [SerializeField] private TransformEventChannelSO checkpointActivatedChannel;
        
        [Tooltip("Gizlenecek görsel GameObject (Sprite, Işık vb. içeren alt obje).")]
        [SerializeField] private GameObject visualObject;
        
        [Tooltip("Kapsayan 2D Collider (Otomatik atanmazsa buradan seçilebilir).")]
        [SerializeField] private Collider2D collectibleCollider;

        public event Action<Collectible> OnCollected;

        private CollectibleEffect[] effects;
        private PlayerHealth playerHealth;

        private bool isCollected;
        private bool isCommitted;
        private CancellationTokenSource respawnCts;

        public bool IsCollected => isCollected;
        public bool IsCommitted => isCommitted;
        public bool HideVisualOnCollect { get => hideVisualOnCollect; set => hideVisualOnCollect = value; }
        public GameObject VisualObject => visualObject;

        private void Awake()
        {
            effects = GetComponents<CollectibleEffect>();
            if (collectibleCollider == null)
            {
                collectibleCollider = GetComponent<Collider2D>();
            }
        }

        private void Start()
        {
            // PlayerHealth ve DeathZone ilişkileri sahneden dinamik bulunur
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandlePlayerDeath;
            }

            if (checkpointActivatedChannel != null)
            {
                checkpointActivatedChannel.OnEventRaised += HandleCheckpointActivated;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandlePlayerDeath;
            }

            if (checkpointActivatedChannel != null)
            {
                checkpointActivatedChannel.OnEventRaised -= HandleCheckpointActivated;
            }
            CancelRespawnTask();
        }

        private void OnDisable()
        {
            CancelRespawnTask();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.TryGetComponent(out PlayerController player))
            {
                Collect(player);
            }
        }

        private void Collect(PlayerController player)
        {
            isCollected = true;
            
            // Görselleri ve fizik algılamayı kapat
            if (hideVisualOnCollect && visualObject != null) visualObject.SetActive(false);
            if (collectibleCollider != null) collectibleCollider.enabled = false;

            // Tüm bağlı efektleri tetikle (composition)
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.Apply(player);
                }
            }

            OnCollected?.Invoke(this);

            if (respawnsAfterDelay)
            {
                CancelRespawnTask();
                respawnCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                RespawnRoutineAsync(respawnCts.Token).Forget();
            }
        }

        private async UniTaskVoid RespawnRoutineAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(respawnDelay), cancellationToken: token);
                ResetCollectible();
            }
            catch (OperationCanceledException)
            {
                // Görev iptal edildiğinde hiçbir şey yapma
            }
        }

        private void CancelRespawnTask()
        {
            if (respawnCts != null)
            {
                respawnCts.Cancel();
                respawnCts.Dispose();
                respawnCts = null;
            }
        }

        private void ResetCollectible()
        {
            isCollected = false;
            if (visualObject != null) visualObject.SetActive(true);
            if (collectibleCollider != null) collectibleCollider.enabled = true;
        }

        public void ForceReset()
        {
            CancelRespawnTask();
            isCommitted = false;
            ResetCollectible();
        }

        private void HandleCheckpointActivated(Transform checkpoint)
        {
            // Eğer toplanmış ve ölünce resetlenen türdense, kalıcı (committed) yap
            if (isCollected && resetOnDeath)
            {
                isCommitted = true;
            }
        }

        private void HandlePlayerDeath()
        {
            // Eğer toplanmış ama henüz checkpoint kaydedilmemişse veya respawn olan bir nesneyse sıfırla
            if ((isCollected && resetOnDeath && !isCommitted) || respawnsAfterDelay)
            {
                CancelRespawnTask();
                ResetCollectible();
            }
        }
    }
}
