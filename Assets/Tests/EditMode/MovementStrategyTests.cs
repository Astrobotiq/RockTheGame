using System.Reflection;
using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Platform hareket stratejileri zamanın saf fonksiyonudur: aynı zaman, aynı konum.
    /// Alanlar [SerializeField] private olduğundan reflection ile ayarlanır.
    /// </summary>
    public class MovementStrategyTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private T Create<T>(Vector3 position) where T : MovementStrategy
        {
            _go = new GameObject("StrategyUnderTest");
            _go.transform.position = position;
            return _go.AddComponent<T>();
        }

        private static void Set(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(info, $"{target.GetType().Name}.{field} alani bulunamadi (yeniden adlandirildiysa testi guncelle)");
            info.SetValue(target, value);
        }

        private static void AssertVector(Vector2 expected, Vector2 actual, float tolerance = 0.001f)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance, $"x: beklenen {expected}, gercek {actual}");
            Assert.AreEqual(expected.y, actual.y, tolerance, $"y: beklenen {expected}, gercek {actual}");
        }

        // ---- LinearPingPongStrategy ----

        private LinearPingPongStrategy CreateLinear(float period, float phaseOffset = 0f)
        {
            var s = Create<LinearPingPongStrategy>(Vector3.zero);
            Set(s, "startPoint", new Vector2(0f, 0f));
            Set(s, "endPoint", new Vector2(10f, 0f));
            Set(s, "period", period);
            Set(s, "phaseOffset", phaseOffset);
            return s;
        }

        [Test]
        public void Linear_StartsAtStartPoint_ReachesEndAtHalfPeriod_ReturnsAtFullPeriod()
        {
            var s = CreateLinear(period: 4f);

            AssertVector(new Vector2(0f, 0f), s.GetPositionAtTime(0f));
            AssertVector(new Vector2(5f, 0f), s.GetPositionAtTime(1f));
            AssertVector(new Vector2(10f, 0f), s.GetPositionAtTime(2f));
            AssertVector(new Vector2(0f, 0f), s.GetPositionAtTime(4f));
        }

        [Test]
        public void Linear_IsPeriodic()
        {
            var s = CreateLinear(period: 4f);

            AssertVector(s.GetPositionAtTime(1.3f), s.GetPositionAtTime(1.3f + 4f));
            AssertVector(s.GetPositionAtTime(1.3f), s.GetPositionAtTime(1.3f + 8f));
        }

        [Test]
        public void Linear_PhaseOffset_ShiftsTheCycle()
        {
            var s = CreateLinear(period: 4f, phaseOffset: 2f);

            // offset yarim periyot: t=0'da zaten uc noktada
            AssertVector(new Vector2(10f, 0f), s.GetPositionAtTime(0f));
        }

        [Test]
        public void Linear_ExposesPeriod()
        {
            Assert.AreEqual(4f, CreateLinear(period: 4f).Period, 0.0001f);
        }

        // ---- CircularMovementStrategy ----

        private CircularMovementStrategy CreateCircular(bool clockwise, float initialAngle = 0f)
        {
            var s = Create<CircularMovementStrategy>(new Vector3(5f, 5f, 0f));
            Set(s, "radius", 5f);
            Set(s, "period", 4f);
            Set(s, "clockwise", clockwise);
            Set(s, "initialAngle", initialAngle);
            return s;
        }

        [Test]
        public void Circular_StartsOnTheRightOfTheCenter()
        {
            var s = CreateCircular(clockwise: false);

            AssertVector(new Vector2(10f, 5f), s.GetPositionAtTime(0f));
        }

        [Test]
        public void Circular_CounterClockwise_QuarterPeriodMovesUp()
        {
            var s = CreateCircular(clockwise: false);

            AssertVector(new Vector2(5f, 10f), s.GetPositionAtTime(1f));
        }

        [Test]
        public void Circular_Clockwise_QuarterPeriodMovesDown()
        {
            var s = CreateCircular(clockwise: true);

            AssertVector(new Vector2(5f, 0f), s.GetPositionAtTime(1f));
        }

        [Test]
        public void Circular_ReturnsToStartAfterOnePeriod()
        {
            var s = CreateCircular(clockwise: true, initialAngle: 30f);

            AssertVector(s.GetPositionAtTime(0f), s.GetPositionAtTime(4f));
        }

        [Test]
        public void Circular_StaysOnTheCircle()
        {
            var s = CreateCircular(clockwise: true);
            var center = new Vector2(5f, 5f);

            for (float t = 0f; t < 4f; t += 0.37f)
                Assert.AreEqual(5f, Vector2.Distance(center, s.GetPositionAtTime(t)), 0.001f);
        }
    }
}
