using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Balon cezasının titreşimi (docs/multiplayer-ceza-balonu-plani.md → BB5). Sistem başına bir ayar SO'su deseni
    /// (PlatformVibrationSettingsSO ile aynı). Eğri zamanı 0–1 normalize; his testi BB7'de.
    /// </summary>
    [CreateAssetMenu(fileName = "PenaltyVibrationSettings", menuName = "Rock The Game/Multiplayer/Penalty Vibration Settings")]
    public sealed class PenaltyVibrationSettingsSO : ScriptableObject
    {
        [Tooltip("Balon şişerken hafif bir darbe.")]
        public RumbleProfile InflateProfile;
        [Tooltip("Balon patlarken kısa, biraz daha belirgin bir darbe.")]
        public RumbleProfile PopProfile;
    }
}
