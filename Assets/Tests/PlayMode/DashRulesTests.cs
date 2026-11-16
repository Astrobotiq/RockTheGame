using System.Collections;
using System.Collections.Generic;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md D1, D2 ve D4.</summary>
    public class DashRulesTests : PlayerRuleTestBase
    {
        [UnityTest]
        public IEnumerator D1_Dash_SpendsTheOnlyCharge_SecondPressDoesNothing()
        {
            yield return PressDash();
            Assert.IsInstanceOf<DashState>(Player.CurrentState);
            Assert.IsFalse(Player.HasDashCharge);

            yield return WaitForAir();
            yield return PressDash();

            AssertInAir();
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
        public IEnumerator D4_DashDistance_DoesNotDependOnFrameRate()
        {
            int oldRate = Application.targetFrameRate;
            int oldVSync = QualitySettings.vSyncCount;
            QualitySettings.vSyncCount = 0;
            var distances = new List<float>();

            try
            {
                foreach (int rate in new[] { 30, 60, 144 })
                {
                    Application.targetFrameRate = rate;
                    yield return ResetPlayerAt(Origin);
                    yield return null;

                    Input.LeftStick = Vector2.right;
                    yield return PressDash();
                    Assert.IsInstanceOf<DashState>(Player.CurrentState);
                    float startX = Player.PlayerRigidbody.position.x;

                    // Fizik adimi adimi bekle: kare sinirinda Airborne'da fazladan adim sayilmasin.
                    float deadline = Time.unscaledTime + Timeout;
                    while (Player.CurrentState is DashState && Time.unscaledTime < deadline)
                        yield return new WaitForFixedUpdate();

                    distances.Add(Player.PlayerRigidbody.position.x - startX);
                    Input.LeftStick = Vector2.zero;
                    yield return WaitForHitStopEnd();
                }
            }
            finally
            {
                Application.targetFrameRate = oldRate;
                QualitySettings.vSyncCount = oldVSync;
            }

            float step = Player.Stats.DashSpeed * Time.fixedDeltaTime;
            Assert.AreEqual(Player.Stats.DashDistance, distances[0], step, "mesafe DashDistance'a bir adim icinde olmali");
            Assert.AreEqual(distances[0], distances[1], 1e-3f, "30 ve 60 fps farkli mesafe");
            Assert.AreEqual(distances[0], distances[2], 1e-3f, "30 ve 144 fps farkli mesafe");
        }

        [UnityTest]
        public IEnumerator D2_WallJump_DoesNotRefillDash()
        {
            yield return SlideOnRightWall();
            Player.UseDash();

            yield return PressJump();

            AssertInAir();
            Assert.IsFalse(Player.HasDashCharge);
        }
    }
}
