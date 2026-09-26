using New_Scripts.Player.Nodes.Rotation;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// FullRotationTracker saf bir hesap sinifi: dönen node'un 360° ödülünün doğruluğunu korur.
    /// </summary>
    public class FullRotationTrackerTests
    {
        private static readonly Vector2 Anchor = Vector2.zero;

        private static Vector2 PositionAt(float degrees, float radius = 3f)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return Anchor + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }

        /// <summary>startDegrees'ten başlayıp her adımda stepDegrees döner; true dönen adım sayısını verir.</summary>
        private static int CountCompletions(FullRotationTracker tracker, float startDegrees, float stepDegrees, int steps)
        {
            int completions = 0;
            tracker.Tick(PositionAt(startDegrees), Anchor); // ilk çağrı yalnızca başlatır
            for (int i = 1; i <= steps; i++)
            {
                if (tracker.Tick(PositionAt(startDegrees + stepDegrees * i), Anchor))
                    completions++;
            }
            return completions;
        }

        [Test]
        public void FirstTick_OnlyInitializes_AndReturnsFalse()
        {
            var tracker = new FullRotationTracker();

            Assert.IsFalse(tracker.Tick(PositionAt(0f), Anchor));
            Assert.AreEqual(0f, tracker.Progress, 0.0001f);
        }

        [Test]
        public void CounterClockwiseFullTurn_CompletesExactlyOnce()
        {
            var tracker = new FullRotationTracker(360f);

            // 45 adim x 9° = 405°: bir tam tur + artan 45° (ikinci tura yetmez)
            Assert.AreEqual(1, CountCompletions(tracker, 0f, 9f, 45));
        }

        [Test]
        public void ClockwiseFullTurn_CompletesExactlyOnce()
        {
            var tracker = new FullRotationTracker(360f);

            Assert.AreEqual(1, CountCompletions(tracker, 0f, -9f, 45));
        }

        [Test]
        public void PartialTurn_DoesNotComplete_AndReportsProgress()
        {
            var tracker = new FullRotationTracker(360f);

            // 30 adim x 10° = 300°
            Assert.AreEqual(0, CountCompletions(tracker, 0f, 10f, 30));
            Assert.AreEqual(300f / 360f, tracker.Progress, 0.01f);
        }

        [Test]
        public void ReversingDirection_CancelsAccumulatedProgress()
        {
            var tracker = new FullRotationTracker(360f);

            CountCompletions(tracker, 0f, 10f, 18);     // +180°
            for (int i = 1; i <= 18; i++)               // -180° geri
                tracker.Tick(PositionAt(180f - 10f * i), Anchor);

            Assert.AreEqual(0f, tracker.Progress, 0.01f);
        }

        [Test]
        public void CrossingTheMinus180To180Boundary_DoesNotBreakAccumulation()
        {
            var tracker = new FullRotationTracker(360f);

            // 150°'den basla: SignedAngle'in +-180 sinirini gecerek 22 adim x 18° = 396° ilerle.
            // Tam 360° hedeflenmez: float birikimi 359.99998'e düşüp testi kırılgan yapar.
            int completions = CountCompletions(tracker, 150f, 18f, 22);

            Assert.AreEqual(1, completions);
        }

        [Test]
        public void Reset_ClearsProgress_AndRequiresReinitialization()
        {
            var tracker = new FullRotationTracker(360f);
            CountCompletions(tracker, 0f, 10f, 20);

            tracker.Reset();

            Assert.AreEqual(0f, tracker.Progress, 0.0001f);
            Assert.IsFalse(tracker.Tick(PositionAt(90f), Anchor), "Reset sonrası ilk Tick yalnızca başlatmalı");
        }

        [Test]
        public void CustomTarget_CompletesAtThatAngle()
        {
            var tracker = new FullRotationTracker(180f);

            // 25 adim x 9° = 225° -> 180° hedefi bir kez tamamlanir
            Assert.AreEqual(1, CountCompletions(tracker, 0f, 9f, 25));
        }
    }
}
