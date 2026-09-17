using UnityEngine;

namespace New_Scripts.Player
{
    [System.Serializable]
    public struct RumbleProfile
    {
        public float duration;
        public AnimationCurve leftMotorCurve;
        public AnimationCurve rightMotorCurve;
    }

    [System.Serializable]
    public struct ContinuousRumbleProfile
    {
        [Range(0f, 1f)] public float leftMotorSpeed;
        [Range(0f, 1f)] public float rightMotorSpeed;
    }
}
