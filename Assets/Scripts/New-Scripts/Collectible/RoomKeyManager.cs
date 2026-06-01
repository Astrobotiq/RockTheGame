using System.Collections.Generic;
using New_Scripts.Door;
using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Odadaki anahtar parçalarının toplanma durumunu izler ve hepsi toplandığında kapıyı açar.
    /// Oyuncu öldüğünde anahtarlar kaydedilmediyse kapıyı geri kapatır.
    /// </summary>
    public class RoomKeyManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Bu kapıyı açmak için toplanması gereken anahtarlar.")]
        [SerializeField] private List<Collectible> keys = new();
        
        [Tooltip("Anahtarlar toplandığında açılacak kapı.")]
        [SerializeField] private RoomDoor door;

        private PlayerHealth playerHealth;

        private void OnEnable()
        {
            foreach (var key in keys)
            {
                if (key != null)
                {
                    key.OnCollected += HandleKeyCollected;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var key in keys)
            {
                if (key != null)
                {
                    key.OnCollected -= HandleKeyCollected;
                }
            }
        }

        private void Start()
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandlePlayerDeath;
            }
        }

        private void HandleKeyCollected(Collectible key)
        {
            int collectedCount = 0;
            foreach (var k in keys)
            {
                if (k != null && k.IsCollected)
                {
                    collectedCount++;
                }
            }

            if (collectedCount >= keys.Count)
            {
                if (door != null)
                {
                    door.Open();
                    Debug.Log("Tüm anahtarlar toplandı, kapı açıldı!");
                }
            }
        }

        private void HandlePlayerDeath()
        {
            // Eğer kapı açılmışsa ama tüm anahtarlar henüz checkpoint ile kalıcı (committed) yapılmadıysa kapıyı geri kapat
            if (door != null && door.IsOpen)
            {
                bool allKeysCommitted = true;
                foreach (var k in keys)
                {
                    if (k != null && (!k.IsCollected || !k.IsCommitted))
                    {
                        allKeysCommitted = false;
                        break;
                    }
                }

                if (!allKeysCommitted)
                {
                    door.Close();
                    Debug.Log("Oyuncu öldü ve anahtarlar checkpoint öncesi toplandığı için kapı tekrar kapatıldı.");
                }
            }
        }
    }
}
