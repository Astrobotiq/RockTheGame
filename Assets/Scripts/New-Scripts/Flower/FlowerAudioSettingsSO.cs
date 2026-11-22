using New_Scripts.Audio;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçek sesleri (docs/flower-hareket-plani.md T7). Cue'larda klip yoksa o ses sessizdir (klipler sonra eklenir).
    /// Değerler başlangıçtır; his kullanıcının kararı.
    /// </summary>
    [CreateAssetMenu(fileName = "FlowerAudioSettings", menuName = "RockTheGame/Flower/Audio Settings")]
    public class FlowerAudioSettingsSO : ScriptableObject
    {
        [Header("Uyarı tıkları")]
        [Tooltip("Uyarıda çalan, her seferinde tizleşen tık. İlk tık uyarı başlarken.")]
        public AudioCueSO warnTickCue;
        [Range(1, 8)] public int warnTickCount = 4;
        [Tooltip("İlk ve son tıkın perde çarpanı (aradakiler doğrusal).")]
        public Vector2 warnTickPitch = new Vector2(0.9f, 1.4f);

        [Header("Tutunma ve snap")]
        [Tooltip("Çiçeğe kancayla tutunulduğunda (tutunma noktasında).")]
        public AudioCueSO grabCue;
        [Tooltip("Çiçek kapanırken (snap).")]
        public AudioCueSO snapCue;
    }
}
