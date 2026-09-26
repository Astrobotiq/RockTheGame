using New_Scripts.LevelChange;
using UnityEngine;

namespace New_Scripts.Audio
{
    public class RoomAudioController : MonoBehaviour
    {
        [Header("Audio Channels")]
        [SerializeField] private AudioCuePlayEventChannelSO musicPlayChannel;
        [SerializeField] private AudioCueStopEventChannelSO musicStopChannel;
        [SerializeField] private AudioCuePlayEventChannelSO ambientPlayChannel;
        [SerializeField] private AudioCueStopEventChannelSO ambientStopChannel;

        [Header("Room Music Settings")]
        [Tooltip("Odaya girildiğinde çalınacak müzik. Boş bırakılırsa mevcut müzik çalmaya devam eder.")]
        [SerializeField] private AudioCueSO roomMusicCue;
        [Tooltip("Odadan çıkıldığında bu odaya özel müziğin durdurulup durdurulmayacağı.")]
        [SerializeField] private bool stopMusicOnExit = false;

        [Header("Room Ambient Settings")]
        [Tooltip("Odaya girildiğinde çalınacak ortam sesi.")]
        [SerializeField] private AudioCueSO roomAmbientCue;
        [Tooltip("Odadan çıkıldığında bu odaya özel ortam sesinin durdurulup durdurulmayacağı.")]
        [SerializeField] private bool stopAmbientOnExit = true;

        private Room roomComponent;

        private void Awake()
        {
            roomComponent = GetComponent<Room>();
            if (roomComponent != null)
            {
                roomComponent.OnRoomEntered.AddListener(HandleRoomEntered);
                roomComponent.OnRoomExited.AddListener(HandleRoomExited);
            }
            else
            {
                Debug.LogWarning($"[{name}] RoomAudioController bir Room bileşeni bulamadı. " +
                                 $"Olayları manuel olarak tetiklemeniz gerekecektir.", this);
            }
        }

        private void OnDestroy()
        {
            if (roomComponent != null)
            {
                roomComponent.OnRoomEntered.RemoveListener(HandleRoomEntered);
                roomComponent.OnRoomExited.RemoveListener(HandleRoomExited);
            }
        }

        /// <summary>
        /// Odaya girildiğinde tetiklenen dinleyici.
        /// </summary>
        public void HandleRoomEntered()
        {
            // Müzik değiştir
            if (roomMusicCue != null && musicPlayChannel != null)
            {
                musicPlayChannel.RaisePlayEvent(roomMusicCue);
            }

            // Ambient (ortam sesi) değiştir
            if (roomAmbientCue != null && ambientPlayChannel != null)
            {
                ambientPlayChannel.RaisePlayEvent(roomAmbientCue);
            }
        }

        /// <summary>
        /// Odadan çıkıldığında tetiklenen dinleyici.
        /// </summary>
        public void HandleRoomExited()
        {
            // Müzik durdur
            if (stopMusicOnExit && roomMusicCue != null && musicStopChannel != null)
            {
                musicStopChannel.RaiseStopEvent(roomMusicCue);
            }

            // Ambient durdur
            if (stopAmbientOnExit && roomAmbientCue != null && ambientStopChannel != null)
            {
                ambientStopChannel.RaiseStopEvent(roomAmbientCue);
            }
        }
    }
}
