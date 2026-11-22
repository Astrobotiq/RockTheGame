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
    }
}
