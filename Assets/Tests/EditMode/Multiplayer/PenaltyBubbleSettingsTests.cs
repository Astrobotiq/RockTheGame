using NUnit.Framework;
using New_Scripts.Multiplayer.Penalty;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>Balon ayarlarının varsayılanları plandaki başlangıç değerleriyle uyumlu (docs/multiplayer-ceza-balonu-plani.md → Dizi tablosu).</summary>
    public class PenaltyBubbleSettingsTests
    {
        private PenaltyBubbleSettingsSO _settings;

        [SetUp] public void SetUp() => _settings = ScriptableObject.CreateInstance<PenaltyBubbleSettingsSO>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(_settings);

        [Test]
        public void Defaults_MatchThePlanTable()
        {
            Assert.AreEqual(0.35f, _settings.liftSeconds, 0.0001f);
            Assert.AreEqual(1.5f, _settings.liftHeight, 0.0001f);
            Assert.AreEqual(70f, _settings.poseAngleDegrees, 0.0001f);
            Assert.AreEqual(0.25f, _settings.inflateSeconds, 0.0001f);
            Assert.AreEqual(1.15f, _settings.inflateOvershoot, 0.0001f);
            Assert.AreEqual(2.5f, _settings.minTravelSeconds, 0.0001f);
            Assert.AreEqual(5f, _settings.maxTravelSeconds, 0.0001f);
            Assert.AreEqual(0.25f, _settings.wobbleAmplitude, 0.0001f);
            Assert.AreEqual(1.2f, _settings.wobbleHz, 0.0001f);
            Assert.AreEqual(1.5f, _settings.arriveHeight, 0.0001f);
            Assert.AreEqual(0.3f, _settings.arriveHoldSeconds, 0.0001f);
        }

        [Test]
        public void Defaults_AreInternallyConsistent()
        {
            Assert.Greater(_settings.cruiseSpeed, 0f);
            Assert.Less(_settings.minTravelSeconds, _settings.maxTravelSeconds);
            Assert.GreaterOrEqual(_settings.inflateOvershoot, 1f);
        }

        [Test]
        public void TravelSeconds_UsesTheSettingsBounds()
        {
            Assert.AreEqual(_settings.minTravelSeconds, _settings.TravelSeconds(1f), 0.0001f);
            Assert.AreEqual(_settings.maxTravelSeconds, _settings.TravelSeconds(100000f), 0.0001f);
        }

        [Test]
        public void Load_WithoutAsset_StillReturnsDefaults()
        {
            var loaded = PenaltyBubbleSettingsSO.Load();

            Assert.IsNotNull(loaded);
            Assert.Greater(loaded.cruiseSpeed, 0f);
        }
    }
}
