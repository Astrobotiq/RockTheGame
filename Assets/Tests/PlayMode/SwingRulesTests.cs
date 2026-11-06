using System.Collections;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md S1-S4.</summary>
    public class SwingRulesTests : PlayerRuleTestBase
    {
        private static readonly Vector2 SwingAnchor = Origin + new Vector2(3f, 3f);

        [UnityTest]
        public IEnumerator S1_HittingGentleSlope_Lands([Values(0f, 15f, 20f, 30f, 40f)] float slopeDegrees)
        {
            yield return SwingIntoSlope(slopeDegrees);

            Assert.IsInstanceOf<GroundedState>(Player.CurrentState);
        }

        [UnityTest]
        public IEnumerator S2_HittingSteepSlope_BreaksTheHook([Values(50f, 60f)] float slopeDegrees)
        {
            yield return SwingIntoSlope(slopeDegrees, spendDash: true);

            Assert.IsInstanceOf<AirborneState>(Player.CurrentState);
            Assert.IsFalse(Player.HasDashCharge);
            Assert.IsNull(Player.RightAnchor);
        }

        private IEnumerator SwingIntoSlope(float slopeDegrees, bool spendDash = false)
        {
            // Yay, capanin altina (Origin + (3, -1.24)) iner; yuzey bu noktanin ustunden gecer.
            Vector2 surfacePoint = Origin + new Vector2(3f, -1f);
            Vector2 normal = Quaternion.Euler(0f, 0f, slopeDegrees) * Vector2.up;
            SpawnBox("Slope", surfacePoint - normal * 0.5f, new Vector2(60f, 1f), slopeDegrees);

            StartSwingFrom(SwingAnchor);
            // Yere inmek hakki yenilerdi: hak harcanmis kalirsa karakter inmemis demektir.
            if (spendDash) Player.UseDash();
            yield return WaitWhileState<SwingingState>();
        }

        [UnityTest]
        public IEnumerator S2_HittingWall_BreaksTheHook()
        {
            SpawnWall(Origin.x + 2f);

            StartSwingFrom(SwingAnchor);
            // Yere inmek hakki yenilerdi: hak harcanmis kalirsa karakter inmemis, kanca kopmus demektir.
            Player.UseDash();
            yield return WaitWhileState<SwingingState>();

            Assert.IsInstanceOf<AirborneState>(Player.CurrentState);
            Assert.IsFalse(Player.HasDashCharge);
            Assert.IsNull(Player.RightAnchor);
            Assert.IsNull(Player.LeftAnchor);
        }

        [UnityTest]
        public IEnumerator S3_Swinging_CannotDash()
        {
            StartSwingFrom(Origin + new Vector2(0f, 3f));
            yield return WaitFixedSteps(3);
            Assert.IsTrue(Player.HasDashCharge);

            yield return PressDash();
            yield return null;

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
            Assert.IsTrue(Player.HasDashCharge);
        }

        [UnityTest]
        public IEnumerator S4_Swinging_CannotGrabWall()
        {
            SpawnWall(Origin.x + WallTouchOffset);
            StartSwingFrom(Origin + new Vector2(0f, 3f));
            yield return WaitFixedSteps(3);
            Assert.IsTrue(Player.IsTouchingRightWall, "Test duzeni: oyuncu duvara degmeli");

            Input.IsRightBumperHeld = true;
            yield return WaitFixedSteps(5);

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
        }
    }
}
