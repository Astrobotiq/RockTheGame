using New_Scripts.Player;
using New_Scripts.Player.Visual;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayalet gözlerinin kaynağını oyuncunun PlayerEyesController'ından okur (yalnız okunur özellikler).</summary>
    public static class GhostEyeSourceFactory
    {
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
