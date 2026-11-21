using New_Scripts.Player.Visual;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class ArmShapeMathTests
    {
        private const float Tol = 1e-4f;

        private static void AssertVec(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tol);
            Assert.AreEqual(expected.y, actual.y, Tol);
        }

        [Test]
        public void Bezier_Endpoints_AreAAndB()
        {
            AssertVec(new Vector2(0, 0), ArmShapeMath.Bezier(Vector2.zero, new Vector2(1, 5), new Vector2(2, 0), 0f));
            AssertVec(new Vector2(2, 0), ArmShapeMath.Bezier(Vector2.zero, new Vector2(1, 5), new Vector2(2, 0), 1f));
        }

        [Test]
        public void Bezier_Mid_IsHalfwayToControl()
        {
            AssertVec(new Vector2(1, 2), ArmShapeMath.Bezier(Vector2.zero, new Vector2(1, 4), new Vector2(2, 0), 0.5f));
        }

        [Test]
        public void BowForLength_AtIdleReach_IsIdleBow()
        {
            Assert.AreEqual(0.25f, ArmShapeMath.BowForLength(0.6f, 0.6f, 0.25f), Tol);
        }

        [Test]
        public void BowForLength_LongArm_ShrinksTowardZero()
        {
            Assert.AreEqual(0.01f, ArmShapeMath.BowForLength(15f, 0.6f, 0.25f), Tol);
        }

        [Test]
        public void BowForLength_ZeroLength_IsIdleBowNotNaN()
        {
            float bow = ArmShapeMath.BowForLength(0f, 0.6f, 0.25f);
            Assert.IsFalse(float.IsNaN(bow));
            Assert.AreEqual(0.25f, bow, Tol);
        }

        [Test]
        public void IdleHandTarget_WithAim_ReachesAlongAim()
        {
            AssertVec(new Vector2(1, 1.6f), ArmShapeMath.IdleHandTarget(new Vector2(1, 1), new Vector2(0, 3), Vector2.zero, 0.6f, 0.04f));
        }

        [Test]
        public void IdleHandTarget_NoAim_DroopsDownWhenStill()
        {
            AssertVec(new Vector2(1, 0.4f), ArmShapeMath.IdleHandTarget(new Vector2(1, 1), Vector2.zero, Vector2.zero, 0.6f, 0.04f));
        }

        [Test]
        public void IdleHandTarget_NoAim_TrailsBehindVelocity()
        {
            var shoulder = new Vector2(1, 1);
            var target = ArmShapeMath.IdleHandTarget(shoulder, Vector2.zero, new Vector2(10, 0), 0.6f, 0.04f);
            Assert.Less(target.x, 1f);
            Assert.LessOrEqual((target - shoulder).magnitude, 0.6f + Tol);
        }

        [Test]
        public void GroundDragTarget_Still_RestsOnGroundToTheSide()
        {
            // Omuz y=1, zemin y=0.5, el zeminden 0.2 yukarıda; sağ kol (side=+1) 0.7 yana.
            AssertVec(new Vector2(1.7f, 0.7f), ArmShapeMath.GroundDragTarget(new Vector2(1, 1), 1f, 0.5f, 0.2f, 0.7f, 0f, 0.1f, 1f));
            AssertVec(new Vector2(0.3f, 0.7f), ArmShapeMath.GroundDragTarget(new Vector2(1, 1), -1f, 0.5f, 0.2f, 0.7f, 0f, 0.1f, 1f));
        }

        [Test]
        public void GroundDragTarget_Moving_TrailsBehindVelocity()
        {
            var still = ArmShapeMath.GroundDragTarget(new Vector2(1, 1), 1f, 0.5f, 0.2f, 0.7f, 0f, 0.05f, 1f);
            var running = ArmShapeMath.GroundDragTarget(new Vector2(1, 1), 1f, 0.5f, 0.2f, 0.7f, 6f, 0.05f, 1f);
            Assert.Less(running.x, still.x);
            Assert.AreEqual(still.y, running.y, Tol);
        }

        [Test]
        public void GroundDragTarget_NeverBeyondReach_KeepsHandOnGround()
        {
            var shoulder = new Vector2(1, 1);
            var target = ArmShapeMath.GroundDragTarget(shoulder, 1f, 0.5f, 0.2f, 0.7f, -30f, 0.2f, 1f);
            Assert.LessOrEqual((target - shoulder).magnitude, 1f + Tol);
            Assert.AreEqual(0.7f, target.y, Tol);
        }

        [Test]
        public void WallHandOffsetY_GripSlidesDown_ThenReachesUp()
        {
            // low=-0.1, high=0.9: tutarken (faz 0..0.7) el yukarıdan aşağı iner, sonra hızla yukarı uzanır.
            Assert.AreEqual(0.9f, ArmShapeMath.WallHandOffsetY(0f, -0.1f, 0.9f), Tol);
            Assert.AreEqual(-0.1f, ArmShapeMath.WallHandOffsetY(0.7f, -0.1f, 0.9f), Tol);
            Assert.AreEqual(0.9f, ArmShapeMath.WallHandOffsetY(1f, -0.1f, 0.9f), Tol);
            Assert.AreEqual(0.9f, ArmShapeMath.WallHandOffsetY(2f, -0.1f, 0.9f), Tol);
        }

        [Test]
        public void WallHandOffsetY_NegativePhase_Wraps()
        {
            Assert.AreEqual(ArmShapeMath.WallHandOffsetY(0.25f, -0.1f, 0.9f), ArmShapeMath.WallHandOffsetY(-0.75f, -0.1f, 0.9f), Tol);
        }

        [Test]
        public void IsWallGrip_TrueWhileGripping_FalseWhileReaching()
        {
            Assert.IsTrue(ArmShapeMath.IsWallGrip(0.3f));
            Assert.IsFalse(ArmShapeMath.IsWallGrip(0.85f));
            Assert.IsTrue(ArmShapeMath.IsWallGrip(-0.8f));
        }

        [Test]
        public void WallPhaseStep_BodyTravelEqualsHandSlideWhileGripping()
        {
            // Gövde 0.5 yükselince tutan el dünyada sabit kalmalı: ofset tam 0.5 düşer.
            float phase = ArmShapeMath.WallPhaseStep(0f, 0.5f, -0.1f, 0.9f);
            float before = ArmShapeMath.WallHandOffsetY(0f, -0.1f, 0.9f);
            float after = ArmShapeMath.WallHandOffsetY(phase, -0.1f, 0.9f);
            Assert.AreEqual(0.5f, before - after, Tol);
        }

        [Test]
        public void ExtendProgress_Bounds()
        {
            Assert.AreEqual(0f, ArmShapeMath.ExtendProgress(0f, 0.1f), Tol);
            Assert.AreEqual(1f, ArmShapeMath.ExtendProgress(0.1f, 0.1f), Tol);
            Assert.AreEqual(1f, ArmShapeMath.ExtendProgress(0.2f, 0.1f), Tol);
            Assert.AreEqual(0.875f, ArmShapeMath.ExtendProgress(0.05f, 0.1f), Tol);
            Assert.AreEqual(1f, ArmShapeMath.ExtendProgress(0.05f, 0f), Tol);
        }

        // --- Review Focus 1 ve 5 ---

        [Test]
        public void IsNewGrab_SameMovingNode_IsFalse()
        {
            var node = new GameObject("n").transform;
            try
            {
                Assert.IsFalse(ArmShapeMath.IsNewGrab(true, node, node, new Vector2(1, 1), new Vector2(2, 2)));
            }
            finally { Object.DestroyImmediate(node.gameObject); }
        }

        [Test]
        public void IsNewGrab_FromNothing_IsTrue()
        {
            Assert.IsTrue(ArmShapeMath.IsNewGrab(false, null, null, null, Vector2.one));
        }

        [Test]
        public void IsNewGrab_StaticPointMoved_IsTrue()
        {
            Assert.IsTrue(ArmShapeMath.IsNewGrab(true, null, null, new Vector2(1, 1), new Vector2(5, 5)));
        }

        [Test]
        public void ShouldSnap_Teleport_IsTrue()
        {
            Assert.IsTrue(ArmShapeMath.ShouldSnap(Vector2.zero, new Vector2(10, 0), 0.6f));
        }

        [Test]
        public void ShouldSnap_NormalSway_IsFalse()
        {
            Assert.IsFalse(ArmShapeMath.ShouldSnap(Vector2.zero, new Vector2(1, 0), 0.6f));
        }
    }
}
