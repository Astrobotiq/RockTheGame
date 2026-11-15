using New_Scripts.Player;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class CameraZoneMathTests
    {
        [Test]
        public void Inside_IsAlwaysActive()
        {
            Assert.That(CameraZoneMath.IsActive(false, true, 0f, 1f), Is.True);
            Assert.That(CameraZoneMath.IsActive(true, true, 0f, 0f), Is.True);
        }

        [Test]
        public void Outside_WasInactive_StaysInactive()
        {
            // Girişte histerezis yok: collider'a değmeden aktifleşmez.
            Assert.That(CameraZoneMath.IsActive(false, false, 0.1f, 1f), Is.False);
        }

        [Test]
        public void Outside_WasActive_WithinMargin_StaysActive()
        {
            Assert.That(CameraZoneMath.IsActive(true, false, 0.9f, 1f), Is.True);
            Assert.That(CameraZoneMath.IsActive(true, false, 1f, 1f), Is.True);
        }

        [Test]
        public void Outside_WasActive_BeyondMargin_Deactivates()
        {
            Assert.That(CameraZoneMath.IsActive(true, false, 1.01f, 1f), Is.False);
        }

        [Test]
        public void NegativeMargin_TreatedAsZero()
        {
            Assert.That(CameraZoneMath.IsActive(true, false, 0.1f, -2f), Is.False);
        }
    }
}
