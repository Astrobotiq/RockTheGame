using NUnit.Framework;
using New_Scripts.Multiplayer.Penalty;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Balon cezasının ses ve titreşim bağlantıları gerçekten asset'e bağlı mı (docs/multiplayer-ceza-balonu-plani.md → BB5).
    /// Kod eksik atamayı sessizce yutar (ses çalmaz); bu test eksik atamayı yakalar.
    /// </summary>
    public class PenaltyBubbleAssetsTests
    {
        private static PenaltyBubbleSettingsSO Asset()
        {
            var asset = Resources.Load<PenaltyBubbleSettingsSO>(PenaltyBubbleSettingsSO.ResourcePath);
            Assert.IsNotNull(asset, "Resources/Multiplayer/PenaltyBubbleSettings.asset yok");
            return asset;
        }

        [Test]
        public void Asset_HasAudioChannelsAssigned()
        {
            var s = Asset();
            Assert.IsNotNull(s.sfxPlayChannel, "SFX-Play kanalı");
            Assert.IsNotNull(s.sfxStopChannel, "SFX-Stop kanalı");
        }

        [Test]
        public void Asset_HasThreeCuesWithClips()
        {
            var s = Asset();
            foreach (var (name, cue) in new[] { ("inflate", s.inflateCue), ("travel", s.travelCue), ("pop", s.popCue) })
            {
                Assert.IsNotNull(cue, $"{name} cue atanmamış");
                Assert.IsNotNull(cue.GetRandomClip(), $"{name} cue'nun klibi yok");
            }
        }

        [Test]
        public void Asset_TravelCueLoops_OthersDoNot()
        {
            var s = Asset();
            Assert.IsTrue(s.travelCue.Loop, "yolculuk uğultusu döngü olmalı");
            Assert.IsFalse(s.inflateCue.Loop);
            Assert.IsFalse(s.popCue.Loop);
        }

        [Test]
        public void Asset_PenaltySoundsAreNot3D()
        {
            // Ceza yerel oyuncunun kendi cezası; konuma bağlı ses kamera dışına çıkınca kısılırdı.
            var s = Asset();
            Assert.IsFalse(s.inflateCue.Is3D);
            Assert.IsFalse(s.travelCue.Is3D);
            Assert.IsFalse(s.popCue.Is3D);
        }

        [Test]
        public void Asset_HasBubbleMaterialWithTheBubbleShader()
        {
            var material = Asset().bubbleMaterial;
            Assert.IsNotNull(material, "balon materyali atanmamış: balon düz daire olarak çizilir");
            Assert.AreEqual("Sprites/PenaltyBubble", material.shader.name);
            Assert.IsTrue(material.shader.isSupported, "shader derlenmemiş ya da desteklenmiyor");
        }

        [Test]
        public void Asset_StretchSettingsAreSane()
        {
            var s = Asset();
            Assert.Greater(s.maxStretch, 0f);
            Assert.Less(s.maxStretch, 0.5f, "balon çok esnemesin");
            Assert.Greater(s.stretchReferenceSpeed, 0f);
        }

        [Test]
        public void Asset_HasVibrationProfilesWithCurves()
        {
            var v = Asset().vibration;
            Assert.IsNotNull(v, "titreşim ayarı atanmamış");
            foreach (var (name, profile) in new[] { ("inflate", v.InflateProfile), ("pop", v.PopProfile) })
            {
                Assert.Greater(profile.duration, 0f, $"{name} süresi");
                Assert.IsTrue(profile.leftMotorCurve.length > 0 || profile.rightMotorCurve.length > 0, $"{name} eğrisi boş");
            }
        }

        [Test]
        public void Asset_VibrationIsLight()
        {
            // Plan: "hafif darbe". Hiçbir motor eğrisi 0,8'i aşmamalı.
            var v = Asset().vibration;
            foreach (var profile in new[] { v.InflateProfile, v.PopProfile })
                for (float t = 0f; t <= 1f; t += 0.05f)
                {
                    Assert.LessOrEqual(profile.leftMotorCurve.Evaluate(t), 0.8f);
                    Assert.LessOrEqual(profile.rightMotorCurve.Evaluate(t), 0.8f);
                }
        }
    }
}
