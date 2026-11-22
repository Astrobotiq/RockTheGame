using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class FlowerCoronaMathTests
    {
        private const float OpenPitch = 0.35f, ClosedPitch = 1.2f, OpenCurl = 0.4f, ClosedCurl = 1.4f;

        private static FlowerCoronaMath.Pose Pose(float open, float warn = 0f) =>
            FlowerCoronaMath.Evaluate(open, warn, OpenPitch, ClosedPitch, OpenCurl, ClosedCurl);

        [Test]
        public void ClosedFlower_HasNoFan_AndIsNotVisible()
        {
            var p = Pose(0f);
            Assert.That(p.Fan, Is.EqualTo(0f));
            Assert.That(p.Visible, Is.False);
            Assert.That(p.Pitch, Is.EqualTo(ClosedPitch).Within(1e-5f));
            Assert.That(p.Curl, Is.EqualTo(ClosedCurl).Within(1e-5f));
        }

        [Test]
        public void Anticipation_KeepsTheCoronaClosed()
        {
            Assert.That(Pose(-0.08f).Fan, Is.EqualTo(0f));
            Assert.That(Pose(-0.2f).Visible, Is.False);
        }

        [Test]
        public void CoronaWaitsForThePetals()
        {
            // Yapraklar önce açılır: eşiğin altında taç hâlâ kapalı.
            Assert.That(Pose(FlowerCoronaMath.DefaultOpenStart).Fan, Is.EqualTo(0f).Within(1e-6f));
            Assert.That(Pose(FlowerCoronaMath.DefaultOpenStart * 0.5f).Visible, Is.False);
        }

        [Test]
        public void FullyOpen_IsExactlyOneAndHasOpenValues()
        {
            var p = Pose(1f);
            Assert.That(p.Fan, Is.EqualTo(1f).Within(1e-6f));
            Assert.That(p.Length, Is.EqualTo(1f).Within(1e-6f));
            Assert.That(p.Radius, Is.EqualTo(1f).Within(1e-6f));
            Assert.That(p.Height, Is.EqualTo(1f).Within(1e-6f));
            Assert.That(p.Pitch, Is.EqualTo(OpenPitch).Within(1e-5f));
            Assert.That(p.Curl, Is.EqualTo(OpenCurl).Within(1e-5f));
        }

        [Test]
        public void Opening_IsMonotonic_InFanLengthRadiusHeight_AndPitchFalls()
        {
            FlowerCoronaMath.Pose prev = Pose(0f);
            for (int i = 1; i <= 40; i++)
            {
                var p = Pose(i / 40f);
                Assert.That(p.Fan, Is.GreaterThanOrEqualTo(prev.Fan - 1e-6f));
                Assert.That(p.Length, Is.GreaterThanOrEqualTo(prev.Length - 1e-6f));
                Assert.That(p.Radius, Is.GreaterThanOrEqualTo(prev.Radius - 1e-6f));
                Assert.That(p.Height, Is.GreaterThanOrEqualTo(prev.Height - 1e-6f));
                Assert.That(p.Pitch, Is.LessThanOrEqualTo(prev.Pitch + 1e-6f), "açıldıkça şeritler yataya yaklaşır");
                prev = p;
            }
        }

        [Test]
        public void Overshoot_OpensALittleMore_ButNeverBeyondOpenPitchOrCurl()
        {
            var open = Pose(1f);
            var over = Pose(1.12f);
            Assert.That(over.Fan, Is.GreaterThan(open.Fan));
            Assert.That(over.Length, Is.GreaterThan(open.Length));
            Assert.That(over.Pitch, Is.EqualTo(OpenPitch).Within(1e-5f));
            Assert.That(over.Curl, Is.EqualTo(OpenCurl).Within(1e-5f));
            Assert.That(over.Height, Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void Warn_ShrinksLengthAndRadius_AndLiftsTheStrips()
        {
            var calm = Pose(1f, 0f);
            var warned = Pose(1f, 1f);
            Assert.That(warned.Length, Is.EqualTo(calm.Length * (1f - FlowerCoronaMath.WarnShrink)).Within(1e-5f));
            Assert.That(warned.Radius, Is.EqualTo(calm.Radius * (1f - FlowerCoronaMath.WarnShrink)).Within(1e-5f));
            Assert.That(warned.Pitch, Is.EqualTo(calm.Pitch + FlowerCoronaMath.WarnLift).Within(1e-5f));
        }

        [Test]
        public void Warn_IsMonotonic()
        {
            float prevLen = float.MaxValue, prevPitch = float.MinValue;
            for (int i = 0; i <= 20; i++)
            {
                var p = Pose(1f, i / 20f);
                Assert.That(p.Length, Is.LessThanOrEqualTo(prevLen + 1e-6f));
                Assert.That(p.Pitch, Is.GreaterThanOrEqualTo(prevPitch - 1e-6f));
                prevLen = p.Length;
                prevPitch = p.Pitch;
            }
        }

        [Test]
        public void WarnOutOfRange_IsClamped()
        {
            Assert.That(Pose(1f, 5f).Length, Is.EqualTo(Pose(1f, 1f).Length));
            Assert.That(Pose(1f, -3f).Length, Is.EqualTo(Pose(1f, 0f).Length));
        }

        [Test]
        public void WarnDuringCurlBack_ClosesTheFanFurther()
        {
            // SetWarn yaprak uçlarını open 0.8'e kadar geri kıvırır; taç da onunla birlikte biraz kapanır.
            Assert.That(Pose(0.8f, 1f).Length, Is.LessThan(Pose(1f, 0f).Length));
            Assert.That(Pose(0.8f).Fan, Is.LessThan(1f));
        }

        [Test]
        public void Azimuths_AreEvenlySpaced()
        {
            const int strips = 10;
            float step = 2f * Mathf.PI / strips;
            for (int i = 1; i < strips; i++)
                Assert.That(FlowerCoronaMath.StripAzimuth(i, strips, 0f) - FlowerCoronaMath.StripAzimuth(i - 1, strips, 0f),
                    Is.EqualTo(step).Within(1e-5f));
            Assert.That(FlowerCoronaMath.StripAzimuth(strips, strips, 0f), Is.EqualTo(2f * Mathf.PI).Within(1e-4f));
        }

        [Test]
        public void InnerRing_IsHalfAStepOffTheOuterRing()
        {
            const int strips = 10;
            float step = 2f * Mathf.PI / strips;
            for (int i = 0; i < strips; i++)
            {
                float outer = FlowerCoronaMath.StripAzimuth(i, strips, 0f);
                float inner = FlowerCoronaMath.StripAzimuth(i, strips, 0.5f);
                Assert.That(inner - outer, Is.EqualTo(0.5f * step).Within(1e-5f));
            }
        }

        [Test]
        public void TooFewStrips_AreRaisedToThree()
        {
            Assert.That(FlowerCoronaMath.StripAzimuth(1, 1, 0f), Is.EqualTo(2f * Mathf.PI / 3f).Within(1e-5f));
        }

        // --- Tutunma yumruğu (grip01) ---

        // grip eklenmeden önceki Evaluate'in formülü (referans kopya): grip01 = 0 iken çıktı bununla birebir aynı olmalı.
        private static FlowerCoronaMath.Pose OldEvaluate(float open, float warn01)
        {
            float fan = FlowerCoronaMath.Fan(open);
            float f = Mathf.Clamp01(fan);
            float over = Mathf.Max(0f, fan - 1f);
            float warn = Mathf.Clamp01(warn01);
            float shrink = 1f - FlowerCoronaMath.WarnShrink * warn;
            float length = Mathf.Lerp(0.1f, 1f, f) * (1f + over * 0.5f) * shrink;
            float radius = Mathf.Lerp(0.5f, 1f, f) * shrink;
            float pitch = Mathf.Lerp(ClosedPitch, OpenPitch, f) + FlowerCoronaMath.WarnLift * warn;
            float curl = Mathf.Lerp(ClosedCurl, OpenCurl, f);
            return new FlowerCoronaMath.Pose(fan, length, radius, f, pitch, curl);
        }

        [Test]
        public void ZeroGrip_IsBitIdenticalToTheOldOutput()
        {
            for (float open = -0.2f; open <= 1.3f; open += 0.05f)
            for (float warn = 0f; warn <= 1f; warn += 0.25f)
            {
                var expected = OldEvaluate(open, warn);
                var plain = Pose(open, warn);
                var withZeroGrip = FlowerCoronaMath.Evaluate(open, warn, OpenPitch, ClosedPitch, OpenCurl, ClosedCurl,
                    FlowerCoronaMath.DefaultOpenStart, 0f, FlowerCoronaMath.GripShape.Default);
                foreach (var p in new[] { plain, withZeroGrip })
                {
                    Assert.AreEqual(expected.Fan, p.Fan, $"fan open={open} warn={warn}");
                    Assert.AreEqual(expected.Length, p.Length, $"length open={open} warn={warn}");
                    Assert.AreEqual(expected.Radius, p.Radius, $"radius open={open} warn={warn}");
                    Assert.AreEqual(expected.Height, p.Height, $"height open={open} warn={warn}");
                    Assert.AreEqual(expected.Pitch, p.Pitch, $"pitch open={open} warn={warn}");
                    Assert.AreEqual(expected.Curl, p.Curl, $"curl open={open} warn={warn}");
                }
            }
        }

        [Test]
        public void FullGrip_LiftsAndCurlsTheStrips_AndPinchesAndShortensThem()
        {
            var grip = FlowerCoronaMath.GripShape.Default;
            var open = Pose(1f);
            var fist = FlowerCoronaMath.Evaluate(1f, 0f, OpenPitch, ClosedPitch, OpenCurl, ClosedCurl,
                FlowerCoronaMath.DefaultOpenStart, 1f, grip);
            Assert.That(fist.Pitch, Is.EqualTo(open.Pitch + grip.Lift).Within(1e-5f));
            Assert.That(fist.Curl, Is.EqualTo(open.Curl + grip.Curl).Within(1e-5f));
            Assert.That(fist.Radius, Is.EqualTo(open.Radius * (1f - grip.Pinch)).Within(1e-5f));
            Assert.That(fist.Length, Is.EqualTo(open.Length * (1f - grip.Shorten)).Within(1e-5f));
            Assert.That(fist.Fan, Is.EqualTo(open.Fan), "açıklık (yelpaze) yumrukla değişmez");
        }

        [Test]
        public void Grip_IsMonotonic_AndClamped()
        {
            var grip = FlowerCoronaMath.GripShape.Default;
            float prevPitch = 0f, prevLength = 10f;
            for (int i = 0; i <= 10; i++)
            {
                var p = FlowerCoronaMath.Evaluate(1f, 0f, OpenPitch, ClosedPitch, OpenCurl, ClosedCurl,
                    FlowerCoronaMath.DefaultOpenStart, i / 10f, grip);
                Assert.That(p.Pitch, Is.GreaterThanOrEqualTo(prevPitch));
                Assert.That(p.Length, Is.LessThanOrEqualTo(prevLength));
                prevPitch = p.Pitch;
                prevLength = p.Length;
            }
            var over = FlowerCoronaMath.Evaluate(1f, 0f, OpenPitch, ClosedPitch, OpenCurl, ClosedCurl,
                FlowerCoronaMath.DefaultOpenStart, 3f, grip);
            var one = FlowerCoronaMath.Evaluate(1f, 0f, OpenPitch, ClosedPitch, OpenCurl, ClosedCurl,
                FlowerCoronaMath.DefaultOpenStart, 1f, grip);
            Assert.AreEqual(one.Pitch, over.Pitch, "grip01 > 1 kıskaçlanır");
        }

        [Test]
        public void ReactionSettingsDefaults_MatchTheMathConstants()
        {
            var so = ScriptableObject.CreateInstance<FlowerReactionSettingsSO>();
            try
            {
                var d = FlowerCoronaMath.GripShape.Default;
                Assert.That(so.GripShape.Lift, Is.EqualTo(d.Lift).Within(0.01f));
                Assert.That(so.GripShape.Curl, Is.EqualTo(d.Curl).Within(1e-6f));
                Assert.That(so.GripShape.Pinch, Is.EqualTo(d.Pinch).Within(1e-6f));
                Assert.That(so.GripShape.Shorten, Is.EqualTo(d.Shorten).Within(1e-6f));
                Assert.That(so.petalStiffness, Is.EqualTo(FlowerReactMath.PetalStiffness));
                Assert.That(so.gripReleaseDamping, Is.LessThan(so.gripDamping), "bırakışta daha az sönüm");
                Assert.That(so.enabled, Is.True);
            }
            finally { Object.DestroyImmediate(so); }
        }
    }
}
