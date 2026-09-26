using System.Collections.Generic;
using New_Scripts.Door;
using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Collectible
{
    public enum KeyActionType
    {
        Open,
        Close,
        Toggle
    }

    /// <summary>
    /// Odadaki anahtar parçalarının toplanma durumunu izler ve hepsi toplandığında kapıya ilgili eylemi uygular.
    /// Oyuncu öldüğünde anahtarlar kaydedilmediyse kapıyı eski haline getirir.
    /// </summary>
    public class RoomKeyManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Bu kapıyı etkilemek için toplanması gereken anahtarlar.")]
        [SerializeField] private List<Collectible> keys = new();
        
        [Tooltip("Anahtarlar toplandığında tetiklenecek kapı.")]
        [SerializeField] private RoomDoor door;

        [Header("Settings")]
        [Tooltip("Anahtarlar toplandığında kapıya uygulanacak eylem.")]
        [SerializeField] private KeyActionType actionType = KeyActionType.Open;

        private PlayerHealth playerHealth;
        private bool wasActionExecuted = false;
        private bool doorStateBeforeAction = false;

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

            if (collectedCount >= keys.Count && !wasActionExecuted)
            {
                if (door != null)
                {
                    doorStateBeforeAction = door.IsOpen;
                    wasActionExecuted = true;
                    ExecuteDoorAction();
                }
            }
        }

        private void ExecuteDoorAction()
        {
            switch (actionType)
            {
                case KeyActionType.Open:
                    door.Open();
                    Debug.Log("Tüm anahtarlar toplandı, kapı açıldı!");
                    break;
                case KeyActionType.Close:
                    door.Close();
                    Debug.Log("Tüm anahtarlar toplandı, kapı kapatıldı!");
                    break;
                case KeyActionType.Toggle:
                    if (door.IsOpen)
                    {
                        door.Close();
                        Debug.Log("Tüm anahtarlar toplandı, kapı kapatıldı (Toggle)!");
                    }
                    else
                    {
                        door.Open();
                        Debug.Log("Tüm anahtarlar toplandı, kapı açıldı (Toggle)!");
                    }
                    break;
            }
        }

        private void HandlePlayerDeath()
        {
            // Eğer aksiyon çalışmışsa ama tüm anahtarlar henüz checkpoint ile kalıcı (committed) yapılmadıysa kapıyı eski haline döndür
            if (door != null && wasActionExecuted)
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
                    if (doorStateBeforeAction)
                    {
                        door.Open();
                    }
                    else
                    {
                        door.Close();
                    }
                    Debug.Log("Oyuncu öldü ve anahtarlar checkpoint öncesi toplandığı için kapı eski durumuna getirildi.");
                }
            }

            wasActionExecuted = false;
        }
    }
}
