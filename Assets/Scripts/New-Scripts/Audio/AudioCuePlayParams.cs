using UnityEngine;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Holds optional arguments when requesting sound playback via event channels.
    /// </summary>
    [System.Serializable]
    public struct AudioCuePlayParams
    {
        public Vector3 Position;
        public Transform Parent;
        public bool Is3D;
        public float VolumeMultiplier;
        public float PitchMultiplier;

        public static AudioCuePlayParams Default => new AudioCuePlayParams
        {
            Position = Vector3.zero,
            Parent = null,
            Is3D = false,
            VolumeMultiplier = 1f,
            PitchMultiplier = 1f
        };
    }
}
