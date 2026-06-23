using System;
using UnityEngine;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Devam eden veya döngüsel sesleri durdurma taleplerini dinleyicilere (AudioManager) ileten olay kanalı.
    /// </summary>
    [CreateAssetMenu(fileName = "StopAudioCueEventChannel", menuName = "Events/Audio Cue Stop Channel")]
    public class AudioCueStopEventChannelSO : ScriptableObject
    {
        public event Action<AudioCueSO> OnStopRequested;

        /// <summary>
        /// Belirli bir sesin tüm aktif kopyalarını durdurma olayını tetikler.
        /// </summary>
        public void RaiseStopEvent(AudioCueSO audioCue)
        {
            if (audioCue == null)
            {
                Debug.LogWarning($"[{name}] Durdurulmak istenen AudioCue null!");
                return;
            }
            OnStopRequested?.Invoke(audioCue);
        }
    }
}
