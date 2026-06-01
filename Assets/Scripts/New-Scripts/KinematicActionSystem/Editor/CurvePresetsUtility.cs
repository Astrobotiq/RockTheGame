using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Editor
{
    /// <summary>
    /// Bounce, Spring ve EaseInOut interpolasyon eğrilerini üreten editör yardımcı sınıfı.
    /// </summary>
    public static class CurvePresetsUtility
    {
        public static AnimationCurve GetEaseInOut()
        {
            return AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        public static AnimationCurve GetBounce()
        {
            Keyframe[] keys = new Keyframe[5];
            keys[0] = new Keyframe(0f, 0f, 0f, 4f);
            keys[1] = new Keyframe(0.3f, 1f, 0f, 0f);
            keys[2] = new Keyframe(0.6f, 0.7f, -1f, -1f);
            keys[3] = new Keyframe(0.85f, 1f, 0f, 0f);
            keys[4] = new Keyframe(1f, 1f, 0f, 0f);
            return new AnimationCurve(keys);
        }

        public static AnimationCurve GetSpring()
        {
            Keyframe[] keys = new Keyframe[6];
            keys[0] = new Keyframe(0f, 0f, 0f, 6f);
            keys[1] = new Keyframe(0.4f, 1.2f, 0f, 0f);
            keys[2] = new Keyframe(0.6f, 0.9f, 0f, 0f);
            keys[3] = new Keyframe(0.8f, 1.05f, 0f, 0f);
            keys[4] = new Keyframe(0.9f, 0.98f, 0f, 0f);
            keys[5] = new Keyframe(1f, 1f, 0f, 0f);
            return new AnimationCurve(keys);
        }
    }
}
