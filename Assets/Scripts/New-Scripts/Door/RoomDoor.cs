using System;
using UnityEngine;

namespace New_Scripts.Door
{
    /// <summary>
    /// Kapının sadece durumunu ve fiziksel varlığını yönetir.
    /// Görsel olarak neye benzeyeceğini veya nasıl titreyeceğini bilmez.
    /// </summary>
    public class RoomDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Collider2D doorCollider;

        [Header("Settings")]
        [SerializeField] private bool startOpen = false;

        public event Action OnOpened;
        public event Action OnClosed;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (startOpen) Open();
            else Close();
        }

        [ContextMenu("TEST: Kapıyı Aç")]
        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;

            if (doorCollider != null) doorCollider.enabled = false;
            
            OnOpened?.Invoke();
        }

        [ContextMenu("TEST: Kapıyı Kapat")]
        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (doorCollider != null) doorCollider.enabled = true;

            OnClosed?.Invoke();
        }
    }
}