using System;
using UnityEngine;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Ses oynatma taleplerini dinleyicilere (AudioManager) ileten olay kanalı.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayAudioCueEventChannel", menuName = "Events/Audio Cue Play Channel")]
    public class AudioCuePlayEventChannelSO : ScriptableObject
    {
        public event Action<AudioCueSO, AudioCuePlayParams> OnPlayRequested;

        /// <summary>
        /// Ses çalma olayını tetikler.
        /// </summary>
        public void RaisePlayEvent(AudioCueSO audioCue, AudioCuePlayParams playParams)
        {
            if (audioCue == null)
            {
                Debug.LogWarning($"[{name}] Oynatılmak istenen AudioCue null!");
                return;
            }
            OnPlayRequested?.Invoke(audioCue, playParams);
        }

        /// <summary>
        /// 2D ses çalma olayını tetikler.
        /// </summary>
        public void RaisePlayEvent(AudioCueSO audioCue)
        {
            RaisePlayEvent(audioCue, AudioCuePlayParams.Default);
        }

        /// <summary>
        /// Belirli bir 3D pozisyonda ses çalma olayını tetikler.
        /// </summary>
        public void RaisePlayEvent(AudioCueSO audioCue, Vector3 position)
        {
            var parameters = AudioCuePlayParams.Default;
            parameters.Position = position;
            parameters.Is3D = true;
            RaisePlayEvent(audioCue, parameters);
        }

        /// <summary>
        /// Belirli bir Transform'a bağlı 3D ses çalma olayını tetikler.
        /// </summary>
        public void RaisePlayEvent(AudioCueSO audioCue, Transform parent)
        {
            var parameters = AudioCuePlayParams.Default;
            parameters.Parent = parent;
            parameters.Position = parent != null ? parent.position : Vector3.zero;
            parameters.Is3D = true;
            RaisePlayEvent(audioCue, parameters);
        }
    }
}
