using New_Scripts.Player;
using New_Scripts.Player.Visual;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayaletin göz, kol ve dash efekti kaynaklarını oyuncudan okur (yalnız okunur özellikler).</summary>
    public static class GhostEyeSourceFactory
    {
        public static GhostArmSource ArmsFrom(PlayerController player)
        {
            if (player.LeftArm == null && player.RightArm == null) return null;
            return new GhostArmSource
            {
                LeftPivot = player.LeftArm != null ? player.LeftArm.transform : null,
                LeftSprite = player.LeftArm != null ? player.LeftArm.GetComponentInChildren<UnityEngine.SpriteRenderer>(true) : null,
                RightPivot = player.RightArm != null ? player.RightArm.transform : null,
                RightSprite = player.RightArm != null ? player.RightArm.GetComponentInChildren<UnityEngine.SpriteRenderer>(true) : null
            };
        }

        public static GhostDashSource DashFrom(PlayerController player)
        {
            var vfx = player.GetComponentInChildren<PlayerVFXManager>(true);
            if (vfx == null) return null;
            return new GhostDashSource
            {
                SpawnInterval = vfx.GhostSpawnRate,
                Duration = vfx.GhostDuration,
                Color = vfx.GhostColor,
                Trail = vfx.MovementTrail
            };
        }

        public static GhostEyeSource From(PlayerController player)
        {
            var eyes = player.GetComponentInChildren<PlayerEyesController>(true);
            if (eyes == null) return null;

            var source = new GhostEyeSource
            {
                Root = eyes.transform,
                DefaultRootLocalPosition = eyes.DefaultLocalPosition,
                AnimatedAnchor = eyes.AnimatedAnchor,
                DefaultAnchorLocalPosition = eyes.DefaultAnchorLocalPosition,
                VelocitySensitivity = eyes.VelocitySensitivity,
                MaxPupilOffset = eyes.MaxPupilOffset,
                PupilSmoothSpeed = eyes.PupilSmoothSpeed,
                FlipParentTransform = eyes.FlipParentTransform
            };
            foreach (var eye in eyes.Eyes)
            {
                if (eye == null) continue;
                source.Eyes.Add(new GhostEyeSource.Eye
                {
                    Container = eye.eyeContainer,
                    Background = eye.eyeBgRenderer,
                    Pupil = eye.pupilRenderer,
                    Blink = eye.blinkRenderer,
                    DefaultContainerLocalPosition = eye.defaultContainerLocalPos,
                    DefaultPupilLocalPosition = eye.defaultPupilLocalPosition
                });
            }
            return source;
        }
    }
}
