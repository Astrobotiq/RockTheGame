using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class LaunchDirectionMathTests
    {
        private static void AssertVec(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(1e-5f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(1e-5f));
        }

        [Test]
        public void TransformUp_IgnoresContactPoint()
        {
            var d = LaunchDirectionMath.Resolve(LaunchDirectionMode.TransformUp, Vector2.zero, new Vector2(5f, -3f), Vector2.up);
            AssertVec(d, Vector2.up);
        }

        [Test]
        public void TransformUp_NormalizesRotatedUp()
        {
            var d = LaunchDirectionMath.Resolve(LaunchDirectionMode.TransformUp, Vector2.zero, Vector2.zero, new Vector2(3f, 0f));
            AssertVec(d, Vector2.right);
        }

        [Test]
        public void Outward_PointsFromCenterToContact()
        {
            var d = LaunchDirectionMath.Resolve(LaunchDirectionMode.OutwardFromCenter, new Vector2(10f, 10f), new Vector2(10f, 12f), Vector2.up);
            AssertVec(d, Vector2.up);
            d = LaunchDirectionMath.Resolve(LaunchDirectionMode.OutwardFromCenter, new Vector2(10f, 10f), new Vector2(13f, 14f), Vector2.up);
            AssertVec(d, new Vector2(0.6f, 0.8f));
        }

        [Test]
        public void Outward_ContactBelowCenter_LaunchesDownward()
        {
            var d = LaunchDirectionMath.Resolve(LaunchDirectionMode.OutwardFromCenter, Vector2.zero, new Vector2(0f, -2f), Vector2.up);
            AssertVec(d, Vector2.down);
        }

        [Test]
        public void Outward_ContactAtCenter_FallsBackToUp()
        {
            var d = LaunchDirectionMath.Resolve(LaunchDirectionMode.OutwardFromCenter, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.up);
            AssertVec(d, Vector2.up);
        }

        [Test]
        public void Outward_ResultIsAlwaysUnitLength()
        {
            for (float a = 0f; a < 360f; a += 17f)
            {
                var contact = Quaternion.Euler(0, 0, a) * new Vector3(7.3f, 0f, 0f);
                var d = LaunchDirectionMath.Resolve(LaunchDirectionMode.OutwardFromCenter, Vector2.zero, contact, Vector2.up);
                Assert.That(d.magnitude, Is.EqualTo(1f).Within(1e-5f));
            }
        }

        [Test]
        public void FanDirections_IsSymmetricAroundUp()
        {
            var fan = LaunchDirectionMath.FanDirections(Vector2.up, 60f, 5);
            Assert.That(fan.Length, Is.EqualTo(5));
            AssertVec(fan[2], Vector2.up);
            Assert.That(fan[0].x, Is.EqualTo(-fan[4].x).Within(1e-5f));
            Assert.That(fan[0].y, Is.EqualTo(fan[4].y).Within(1e-5f));
            Assert.That(Vector2.Angle(fan[0], Vector2.up), Is.EqualTo(60f).Within(1e-3f));
        }

        [Test]
        public void FanDirections_SingleCount_IsJustUp()
        {
            var fan = LaunchDirectionMath.FanDirections(Vector2.up, 60f, 1);
            Assert.That(fan.Length, Is.EqualTo(1));
            AssertVec(fan[0], Vector2.up);
        }
    }
}
