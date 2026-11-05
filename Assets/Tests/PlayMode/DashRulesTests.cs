using System.Collections;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md D1 ve D2.</summary>
    public class DashRulesTests : PlayerRuleTestBase
    {
        [UnityTest]
        public IEnumerator D1_Dash_SpendsTheOnlyCharge_SecondPressDoesNothing()
        {
            yield return PressDash();
            Assert.IsInstanceOf<DashState>(Player.CurrentState);
            Assert.IsFalse(Player.HasDashCharge);

            yield return WaitForState<AirborneState>();
            yield return PressDash();

            Assert.IsInstanceOf<AirborneState>(Player.CurrentState);
        }

        [UnityTest]
        public IEnumerator D2_Landing_RefillsDash()
        {
            Player.UseDash();

            yield return LandOnFloor();

            Assert.IsTrue(Player.HasDashCharge);
        }

        [UnityTest]
        public IEnumerator D2_GrabbingWithOneHook_RefillsDash()
        {
            Player.UseDash();

            yield return GrabWithRight(Origin + new Vector2(0f, 4f));

            Assert.IsTrue(Player.HasDashCharge);
        }

        [UnityTest]
        public IEnumerator D2_GrabbingTheSecondHook_RefillsDash()
        {
            yield return GrabWithRight(Origin + new Vector2(3f, 4f));
            Player.UseDash();

            var left = Origin + new Vector2(-3f, 4f);
            SpawnNode(left);
            Input.LeftStick = (left - Player.PlayerRigidbody.position).normalized;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<DualSwingingState>();

            Assert.IsTrue(Player.HasDashCharge);
        }

        [UnityTest]
        public IEnumerator D2_LaunchPad_RefillsDash()
        {
            Player.UseDash();

            yield return TouchLaunchPad();

            Assert.IsTrue(Player.HasDashCharge);
        }

        [UnityTest]
        public IEnumerator D2_Respawn_RefillsDash()
        {
            Player.UseDash();

            yield return ResetPlayerAt(Origin);

            Assert.IsTrue(Player.HasDashCharge);
        }

        [UnityTest]
        public IEnumerator D2_WallJump_DoesNotRefillDash()
        {
            yield return SlideOnRightWall();
            Player.UseDash();

            yield return PressJump();

            Assert.IsInstanceOf<AirborneState>(Player.CurrentState);
            Assert.IsFalse(Player.HasDashCharge);
        }
    }
}
