using New_Scripts.Player.Nodes.Rotation;
using New_Scripts.Player.Nodes.Spiral;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Spiral motorunun saf hesapları: hedef hız, sınırlı ivmeyle yaklaşma, döngü garantisi ve takılma ölçütü.
    /// </summary>
    public class SpiralDriveMathTests
    {
        private const float Dt = 0.02f;

        [TestCase(1)]
        [TestCase(-1)]
        public void EffectiveTarget_InputNeverFlipsSign(int spinSign)
        {
            foreach (float input in new[] { -1f, 0f, 1f })
            {
                float target = SpiralDriveMath.EffectiveTarget(spinSign, 18f, input, 0.4f, 1f);
                Assert.AreEqual(spinSign, (int)Mathf.Sign(target), $"input {input}");
            }
            Assert.AreEqual(spinSign * 18f * 1.4f, SpiralDriveMath.EffectiveTarget(spinSign, 18f, 1f, 0.4f, 1f), 0.0001f);
            Assert.AreEqual(spinSign * 18f * 0.6f, SpiralDriveMath.EffectiveTarget(spinSign, 18f, -1f, 0.4f, 1f), 0.0001f);
        }

        [Test]
        public void Step_SameDirectionBelow_AcceleratesAtDrive()
        {
            Assert.AreEqual(1.2f, SpiralDriveMath.StepTangentialSpeed(0f, 18f, 60f, 45f, 0f, Dt), 0.0001f);
        }

        [Test]
        public void Step_SameDirectionAbove_KeepsSpeed()
        {
            Assert.AreEqual(25f, SpiralDriveMath.StepTangentialSpeed(25f, 18f, 60f, 45f, 0f, Dt), 0.0001f);
            Assert.AreEqual(24.8f, SpiralDriveMath.StepTangentialSpeed(25f, 18f, 60f, 45f, 10f, Dt), 0.0001f);
        }

        [Test]
        public void Step_Opposite_UsesOpposeAccel()
        {
            Assert.AreEqual(-24.1f, SpiralDriveMath.StepTangentialSpeed(-25f, 18f, 60f, 45f, 0f, Dt), 0.0001f);
        }

        [Test]
        public void Step_Opposite_ReversesWithinExpectedTime()
        {
            float speed = -25f;
            float t = 0f;
            while (speed <= 0f && t < 5f)
            {
                speed = SpiralDriveMath.StepTangentialSpeed(speed, 18f, 60f, 45f, 0f, Dt);
                t += Dt;
            }
            Assert.AreEqual(25f / 45f, t, 0.0201f);
        }

        [Test]
        public void GuaranteesLoop()
        {
            Assert.IsTrue(SpiralDriveMath.GuaranteesLoop(60f, 45f, -32f));
            Assert.IsFalse(SpiralDriveMath.GuaranteesLoop(60f, 30f, -32f));
        }

        /// <summary>Anchor altında durgun bir nokta: yerçekimi teğete izdüşülür, motor adımı, ip çemberine oturtma. Biriken işaretli açıyı döndürür.</summary>
        private static float SimulateLoop(float rope, float seconds, out bool completed)
        {
            Vector2 anchor = Vector2.zero;
            Vector2 pos = anchor + Vector2.down * rope;
            float speed = 0f, accumulated = 0f;
            Vector2 prevDir = (pos - anchor).normalized;
            var tracker = new FullRotationTracker(360f);
            tracker.Tick(pos, anchor);
            completed = false;

            for (float t = 0f; t < seconds && !completed; t += Dt)
            {
                Vector2 d = (anchor - pos).normalized;
                Vector2 tangent = new Vector2(d.y, -d.x);
                speed += Vector2.Dot(new Vector2(0f, -32f), tangent) * Dt;
                speed = SpiralDriveMath.StepTangentialSpeed(speed, 18f, 60f, 45f, 0f, Dt);
                pos = anchor + (pos + tangent * speed * Dt - anchor).normalized * rope;
                Vector2 dir = (pos - anchor).normalized;
                accumulated += Vector2.SignedAngle(prevDir, dir);
                prevDir = dir;
                completed = tracker.Tick(pos, anchor);
            }
            return accumulated;
        }

        [TestCase(2f)]
        [TestCase(5f)]
        [TestCase(10f)]
        public void Simulated_FullLoop_AnyRopeLength(float rope)
        {
            float accumulated = SimulateLoop(rope, 6f, out bool completed);

            Assert.IsTrue(completed, "6 sn içinde tam tur yok");
            Assert.Greater(accumulated, 0f, "pozitif hedef CCW (pozitif açı) döndürmeli");
        }

        [Test]
        public void IsBlocked()
        {
            Assert.IsTrue(SpiralDriveMath.IsBlocked(18f, 2f, 0.25f));
            Assert.IsFalse(SpiralDriveMath.IsBlocked(18f, 10f, 0.25f));
            Assert.IsFalse(SpiralDriveMath.IsBlocked(0.2f, 0f, 0.25f));
            Assert.IsTrue(SpiralDriveMath.IsBlocked(-18f, -2f, 0.25f));
        }
    }
}
