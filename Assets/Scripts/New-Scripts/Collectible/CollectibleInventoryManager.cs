using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Checkpoint olaylarını ve oyuncu ölümünü dinleyerek, envanter ScriptableObject'inin
    /// commit/revert (kaydet/geri al) mekanizmasını koordine eden sahne yöneticisi.
    /// </summary>
    public class CollectibleInventoryManager : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private CollectibleInventorySO inventory;
        
        [Header("Event Channels")]
        [SerializeField] private TransformEventChannelSO checkpointActivatedChannel;

        private PlayerHealth playerHealth;

        private void OnEnable()
        {
            if (checkpointActivatedChannel != null)
            {
                checkpointActivatedChannel.OnEventRaised += HandleCheckpointActivated;
            }

            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (checkpointActivatedChannel != null)
            {
                checkpointActivatedChannel.OnEventRaised -= HandleCheckpointActivated;
            }

            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Start()
        {
            if (inventory != null)
            {
                inventory.ResetInventory();
            }
        }

        private void HandleCheckpointActivated(Transform checkpoint)
        {
            if (inventory != null)
            {
                inventory.Commit();
                Debug.Log($"Checkpoint alındı! Toplanan altınlar kalıcı hale getirildi. Toplam: {inventory.TotalCoins}");
            }
        }

        private void HandlePlayerDeath()
        {
            if (inventory != null)
            {
                inventory.Revert();
                Debug.Log($"Oyuncu öldü! Checkpoint sonrası toplanan altınlar geri alındı. Mevcut: {inventory.TotalCoins}");
            }
        }
    }
}
