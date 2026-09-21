using New_Scripts.Player;
using NUnit.Framework;
using UnityEditor;

namespace RockTheGame.Tests
{
    /// <summary>
    /// PlayerStats.asset için "GDD nöbetçisi": değerler bilinçli olarak değiştirilebilir, ama
    /// bir AI ajanı ya da elle yapılan kazara bir değişiklik tasarım bandının dışına çıkarsa haber verir.
    /// Bir değeri bilerek bu bandın dışına çıkarıyorsan önce docs/GDD.md'yi, sonra bu testi güncelle.
    /// </summary>
    public class PlayerStatsSanityTests
    {
        private const string AssetPath = "Assets/Scripts/New-Scripts/Player/SO/PlayerStats.asset";

        private PlayerStatsSO _stats;

        [SetUp]
        public void SetUp()
        {
            _stats = AssetDatabase.LoadAssetAtPath<PlayerStatsSO>(AssetPath);
            Assert.IsNotNull(_stats, $"{AssetPath} bulunamadi");
        }

        [Test]
        public void WallStamina_MatchesGdd_AboutSixSeconds()
        {
            Assert.That(_stats.MaxWallStamina, Is.InRange(5f, 7f), "GDD 5.2: tırmanma dayanıklılığı ~6 sn");
        }

        [Test]
        public void WallSlideTime_MatchesGdd_AboutTwoSeconds()
        {
            Assert.That(_stats.MaxWallSlideTime, Is.InRange(1.5f, 2.5f), "GDD 5.2: duvar kayması en fazla ~2 sn");
        }

        [Test]
        public void StaminaWarning_IsPositive_AndLowerThanMaxStamina()
        {
            Assert.That(_stats.StaminaWarningThreshold, Is.GreaterThan(0f));
            Assert.That(_stats.StaminaWarningThreshold, Is.LessThan(_stats.MaxWallStamina));
        }

        [Test]
        public void CoyoteTimeAndJumpBuffer_AreShortPositiveWindows()
        {
            Assert.That(_stats.CoyoteTimeDuration, Is.InRange(0.01f, 0.25f));
            Assert.That(_stats.JumpBufferDuration, Is.InRange(0.01f, 0.25f));
        }

        [Test]
        public void Dash_HasPositiveDistanceAndDuration_AndConsistentSpeed()
        {
            Assert.That(_stats.DashDistance, Is.GreaterThan(0f));
            Assert.That(_stats.DashDuration, Is.GreaterThan(0f));
            Assert.AreEqual(_stats.DashDistance / _stats.DashDuration, _stats.DashSpeed, 0.0001f);
        }
    }
}
