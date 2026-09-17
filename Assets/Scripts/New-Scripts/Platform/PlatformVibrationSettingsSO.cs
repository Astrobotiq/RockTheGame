using UnityEngine;
using New_Scripts.Player;

namespace New_Scripts.Platform
{
    [CreateAssetMenu(fileName = "PlatformVibrationSettings", menuName = "Platform/Vibration Settings")]
    public class PlatformVibrationSettingsSO : ScriptableObject
    {
        [Header("LaunchPad Profiles")]
        public RumbleProfile LaunchPadProfile;

        [Header("Breakable Platform Profiles")]
        public ContinuousRumbleProfile BreakablePlatformProfile;
    }
}
