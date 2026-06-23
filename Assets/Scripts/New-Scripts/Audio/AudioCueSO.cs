using UnityEngine;
using UnityEngine.Audio;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Bir ses efektinin, müzik veya ortam sesinin kliplerini, ses/perde varyasyonlarını ve yönlendirme ayarlarını tutan ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAudioCue", menuName = "Audio/Audio Cue")]
    public class AudioCueSO : ScriptableObject
    {
        [Header("Clips")]
        [Tooltip("Çalınacak ses klipleri. Birden fazla girilirse rastgele seçilir.")]
        [SerializeField] private AudioClip[] audioClips;

        [Header("Routing")]
        [Tooltip("Sesin yönlendirileceği AudioMixerGroup.")]
        [SerializeField] private AudioMixerGroup mixerGroup;

        [Header("Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float volumeMin = 0.9f;
        [Range(0f, 1f)]
        [SerializeField] private float volumeMax = 1.0f;

        [Range(0.1f, 3f)]
        [SerializeField] private float pitchMin = 0.95f;
        [Range(0.1f, 3f)]
        [SerializeField] private float pitchMax = 1.05f;

        [SerializeField] private bool loop = false;

        [Header("Spatial settings")]
        [SerializeField] private bool is3D = false;
        [Range(0f, 1f)]
        [SerializeField] private float spatialBlend = 1.0f; // 0 = 2D, 1 = 3D
        [SerializeField] private float minDistance = 1.0f;
        [SerializeField] private float maxDistance = 30.0f;

        public AudioClip[] AudioClips => audioClips;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public float VolumeMin => volumeMin;
        public float VolumeMax => volumeMax;
        public float PitchMin => pitchMin;
        public float PitchMax => pitchMax;
        public bool Loop => loop;
        public bool Is3D => is3D;
        public float SpatialBlend => spatialBlend;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;

        /// <summary>
        /// Klipler arasından rastgele bir klibi döndürür.
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (audioClips == null || audioClips.Length == 0)
            {
                return null;
            }
            if (audioClips.Length == 1)
            {
                return audioClips[0];
            }
            return audioClips[Random.Range(0, audioClips.Length)];
        }
    }
}
