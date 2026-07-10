using System;
using New_Scripts.Audio;
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

        [Header("Audio")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;
        [SerializeField] private AudioCueSO openSoundCue;
        [SerializeField] private AudioCueSO closeSoundCue;

        public event Action OnOpened;
        public event Action OnClosed;

        public bool IsOpen { get; private set; }

        private bool _isInitialized;

        private void Awake()
        {
            if (startOpen) Open();
            else Close();
        }

        private void Start()
        {
            _isInitialized = true;
        }

        [ContextMenu("TEST: Kapıyı Aç")]
        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;

            if (doorCollider != null) doorCollider.enabled = false;
            
            if (_isInitialized && sfxPlayChannel != null && openSoundCue != null)
            {
                sfxPlayChannel.RaisePlayEvent(openSoundCue, transform.position);
            }

            OnOpened?.Invoke();
        }

        [ContextMenu("TEST: Kapıyı Kapat")]
        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (doorCollider != null) doorCollider.enabled = true;

            if (_isInitialized && sfxPlayChannel != null && closeSoundCue != null)
            {
                sfxPlayChannel.RaisePlayEvent(closeSoundCue, transform.position);
            }

            OnClosed?.Invoke();
        }
    }
}