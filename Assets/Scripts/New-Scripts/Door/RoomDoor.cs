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
        [SerializeField] private bool disableColliderOnOpen = true;

        [Header("Audio")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;
        [SerializeField] private AudioCueSO openSoundCue;
        [SerializeField] private AudioCueSO closeSoundCue;

        public event Action OnOpened;
        public event Action OnClosed;

        public bool IsOpen { get; private set; }

        /// <summary>Kapı ilk girişte açık mı (sahne ayarı). Ceza sıfırlaması açık başlayan kapıları yeniden açar.</summary>
        public bool StartsOpen => startOpen;

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
        public void Open() => Open(true);

        [ContextMenu("TEST: Kapıyı Kapat")]
        public void Close() => Close(true);

        /// <summary>
        /// Kapıyı ilk girişteki hâline (<c>startOpen</c>) döndürür, ses çalmaz (ceza iris'i kapalıyken kapı sesi duyulmasın).
        /// Durum değişirse <see cref="OnOpened"/>/<see cref="OnClosed"/> yine yayınlanır, görsel doğru pozu alır.
        /// </summary>
        public void ResetToInitial()
        {
            if (startOpen) Open(false);
            else Close(false);
        }

        private void Open(bool playSound)
        {
            if (IsOpen) return;
            IsOpen = true;

            if (doorCollider != null && disableColliderOnOpen) doorCollider.enabled = false;

            if (playSound && _isInitialized && sfxPlayChannel != null && openSoundCue != null)
            {
                sfxPlayChannel.RaisePlayEvent(openSoundCue, transform.position);
            }

            OnOpened?.Invoke();
        }

        private void Close(bool playSound)
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (doorCollider != null && disableColliderOnOpen) doorCollider.enabled = true;

            if (playSound && _isInitialized && sfxPlayChannel != null && closeSoundCue != null)
            {
                sfxPlayChannel.RaisePlayEvent(closeSoundCue, transform.position);
            }

            OnClosed?.Invoke();
        }
    }
}