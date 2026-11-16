using New_Scripts.Player;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests.EditMode
{
    /// <summary>
    /// AirEntry fabrikaları, taşınmadan önceki AirborneState parametre kombinasyonlarını birebir üretmeli
    /// (yer-hava-birlestirme-plani.md Aşama 1: davranış değişmez).
    /// </summary>
    public class AirEntryTests
    {
        private static void AssertEntry(AirEntry e, bool isJumping, bool bypass = false, float endEarly = 1f,
            float grapple = 0f, float wallClimb = 0f, float coyote = 0f, float horizontal = 0f, Vector2 carried = default, bool variable = false)
        {
            Assert.AreEqual(isJumping, e.IsJumping, "IsJumping");
            Assert.AreEqual(bypass, e.BypassJumpGravity, "BypassJumpGravity");
            Assert.AreEqual(endEarly, e.EndEarlyGravityMultiplier, "EndEarlyGravityMultiplier");
            Assert.AreEqual(grapple, e.GrappleLockout, "GrappleLockout");
            Assert.AreEqual(wallClimb, e.WallClimbLockout, "WallClimbLockout");
            Assert.AreEqual(coyote, e.Coyote, "Coyote");
            Assert.AreEqual(horizontal, e.HorizontalLockout, "HorizontalLockout");
            Assert.AreEqual(carried, e.CarriedVelocity, "CarriedVelocity");
            Assert.AreEqual(variable, e.VariableJump, "VariableJump");
        }

        [Test]
        public void Jump_PlainAndBoosted()
        {
            AssertEntry(AirEntry.Jump(false), true, variable: true);
            AssertEntry(AirEntry.Jump(true), true, bypass: true, endEarly: 0.5f);
        }

        [Test]
        public void WalkOff_FullRuleWithCoyote()
        {
            AssertEntry(AirEntry.WalkOff(0.1f), true, coyote: 0.1f);
        }

        [Test]
        public void WallJump_HorizontalLockoutAndCarry()
        {
            var carried = new Vector2(2f, 1f);
            AssertEntry(AirEntry.WallJump(0.15f, false, carried), true, horizontal: 0.15f, carried: carried, variable: true);
            AssertEntry(AirEntry.WallJump(0.15f, true, Vector2.zero), true, bypass: true, endEarly: 0.5f, horizontal: 0.15f);
        }

        [Test]
        public void ClimbJump_WallClimbLockoutAndCarry()
        {
            var carried = new Vector2(0f, 3f);
            AssertEntry(AirEntry.ClimbJump(false, carried), true, wallClimb: 0.2f, carried: carried, variable: true);
            AssertEntry(AirEntry.ClimbJump(true, Vector2.zero), true, bypass: true, endEarly: 0.5f, wallClimb: 0.2f);
        }

        [Test]
        public void HookExits_AndLaunchPad_BypassJumpGravity()
        {
            AssertEntry(AirEntry.SwingRelease(), true, bypass: true);
            AssertEntry(AirEntry.LaunchPad(), true, bypass: true);
            AssertEntry(AirEntry.SlingshotLaunch(0.25f), true, bypass: true, grapple: 0.25f);
        }

        [Test]
        public void Fall_AndWallDrop_ConstantGravity()
        {
            AssertEntry(AirEntry.Fall(), false);
            var carried = new Vector2(1f, -1f);
            AssertEntry(AirEntry.WallDrop(carried), false, carried: carried);
            AssertEntry(AirEntry.WallDrop(carried, 0.2f), false, wallClimb: 0.2f, carried: carried);
        }

        [TestCase(16f, 32f, 0.2f)]
        [TestCase(15f, 32f, 0.2f)]
        [TestCase(12f, 50f, 0.1f)]
        public void HoldJumpSpeed_ReachesSameApexHeight(float v0, float g, float t)
        {
            float v = PlayerStatsSO.HoldJumpSpeed(v0, g, t);
            float heldHeight = v * t + v * v / (2f * g);
            Assert.Less(v, v0);
            Assert.AreEqual(v0 * v0 / (2f * g), heldHeight, 1e-3f);
        }

        [Test]
        public void HoldJumpSpeed_NoHoldOrNoUpwardSpeed_Unchanged()
        {
            Assert.AreEqual(16f, PlayerStatsSO.HoldJumpSpeed(16f, 32f, 0f));
            Assert.AreEqual(-2f, PlayerStatsSO.HoldJumpSpeed(-2f, 32f, 0.2f));
        }
    }
}
