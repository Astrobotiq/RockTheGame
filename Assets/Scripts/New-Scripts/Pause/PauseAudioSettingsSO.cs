using UnityEngine;
using UnityEngine.Audio;

namespace New_Scripts.Pause
{
    /// <summary>
    /// Pause sırasında müziğe uygulanan "uzaktaki odadan geliyor" efektinin ayarları.
    /// Efektler <c>MainMixer</c>'daki Music grubunda (Lowpass Simple + SFX Reverb); parametreler exposed.
    /// Kulakla ayarlanır: Play Mode'da bu asset'in değerlerini değiştirip pause'a basarak dene.
    /// </summary>
    [CreateAssetMenu(fileName = "PauseAudioSettings", menuName = "RockTheGame/Audio/Pause Audio Settings")]
    public class PauseAudioSettingsSO : ScriptableObject
    {
        [Header("Mixer")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private string cutoffParam = "MusicLowpassCutoff";
        [SerializeField] private string reverbRoomParam = "MusicReverbRoom";
        [SerializeField] private string dryLevelParam = "MusicDryLevel";

        [Header("Pause değerleri (efekt açık)")]
        [Tooltip("Düşük geçiren filtre kesme frekansı (Hz). Küçüldükçe daha boğuk, duvarın ardından gibi.")]
        [SerializeField, Range(200f, 22000f)] private float pausedCutoffHz = 1000f;
        [Tooltip("Yankı seviyesi (mB, -10000 = kapalı, 0 = en yüksek).")]
        [SerializeField, Range(-10000f, 0f)] private float pausedReverbRoomMb = -1800f;
        [Tooltip("Kuru ses seviyesi (mB, 0 = değişmez). Düştükçe müzik uzaklaşır, yankı öne çıkar.")]
        [SerializeField, Range(-10000f, 0f)] private float pausedDryLevelMb = -600f;

        [Header("Geçiş")]
        [SerializeField, Min(0f)] private float fadeSeconds = 0.3f;

        // Efekt kapalıyken değerler (mixer'daki varsayılanlar).
        public const float NormalCutoffHz = 22000f;
        public const float NormalReverbRoomMb = -10000f;
        public const float NormalDryLevelMb = 0f;

        public AudioMixer Mixer => mixer;
        public string CutoffParam => cutoffParam;
        public string ReverbRoomParam => reverbRoomParam;
        public string DryLevelParam => dryLevelParam;
        public float PausedCutoffHz => pausedCutoffHz;
        public float PausedReverbRoomMb => pausedReverbRoomMb;
        public float PausedDryLevelMb => pausedDryLevelMb;
        public float FadeSeconds => fadeSeconds;
    }
}
