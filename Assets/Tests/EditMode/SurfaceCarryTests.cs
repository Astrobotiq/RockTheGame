using New_Scripts.Player;
using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Taşıyan yüzeyden ayrılırken hız aktarımının saf hesabı (docs/systems/platforms.md "Ayrılırken hız aktarımı").
    /// </summary>
    public class SurfaceCarryTests
    {
        private const float Dt = 0.02f;

        private class FakeSurface : IMovingSurface
        {
            public Vector2 DeltaPosition { get; set; }
            public Vector2 SurfaceVelocity { get; set; }
            public float JumpBoostMultiplier { get; set; }
        }

        [Test]
        public void Velocity_NullSurface_IsZero()
        {
            Assert.AreEqual(Vector2.zero, SurfaceCarry.Velocity(null, Dt));
        }

        [Test]
        public void Velocity_UsesDeltaPositionOverDt_NotSurfaceVelocity()
        {
            // Level 15 duvarı: 0.08/adım = 4 birim/sn. SurfaceVelocity (son 3 adımın en büyüğü) bilerek farklı.
            var surface = new FakeSurface { DeltaPosition = new Vector2(-0.08f, 0f), SurfaceVelocity = new Vector2(-9f, 0f) };

            Vector2 v = SurfaceCarry.Velocity(surface, Dt);

            Assert.AreEqual(-4f, v.x, 0.0001f);
            Assert.AreEqual(0f, v.y, 0.0001f);
        }

        [Test]
        public void Velocity_StaticSurface_IsZero()
        {
            var surface = new FakeSurface { DeltaPosition = Vector2.zero };
            Assert.AreEqual(Vector2.zero, SurfaceCarry.Velocity(surface, Dt));
        }

        [Test]
        public void Velocity_NonPositiveDt_IsZero()
        {
            var surface = new FakeSurface { DeltaPosition = new Vector2(1f, 1f) };
            Assert.AreEqual(Vector2.zero, SurfaceCarry.Velocity(surface, 0f));
        }

        [Test]
        public void JumpAddition_NoBoost_ReturnsBaseVelocity()
        {
            var surface = new FakeSurface { DeltaPosition = new Vector2(0.08f, 0f), JumpBoostMultiplier = 0f };

            Vector2 v = SurfaceCarry.JumpAddition(surface, Dt, out bool boosted);

            Assert.IsFalse(boosted);
            Assert.AreEqual(4f, v.x, 0.0001f);
        }

        [Test]
        public void JumpAddition_Boost_ReplacesBaseWithOldFormula()
        {
            var surface = new FakeSurface
            {
                DeltaPosition = new Vector2(0.08f, 0f),
                SurfaceVelocity = new Vector2(6f, 0f),
                JumpBoostMultiplier = 1.5f
            };

            Vector2 v = SurfaceCarry.JumpAddition(surface, Dt, out bool boosted);

            Assert.IsTrue(boosted);
            Assert.AreEqual(9f, v.x, 0.0001f, "SurfaceVelocity × çarpan; taban (4) üstüne eklenmez");
        }

        [Test]
        public void JumpAddition_NullSurface_IsZeroAndNotBoosted()
        {
            Vector2 v = SurfaceCarry.JumpAddition(null, Dt, out bool boosted);

            Assert.IsFalse(boosted);
            Assert.AreEqual(Vector2.zero, v);
        }
    }
}
