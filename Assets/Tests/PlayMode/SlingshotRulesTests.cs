using System.Collections;
using New_Scripts.Collectible;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md L1-L5.</summary>
    public class SlingshotRulesTests : PlayerRuleTestBase
    {
        private static readonly Vector2 NodePosition = Origin + new Vector2(0f, 4f);

        private IEnumerator AimSecondHookAtSameNode()
        {
            Input.RightStick = (NodePosition - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator L1_Charge_IsSpentWhenPullStarts_AndNotReturnedOnCancel()
        {
            yield return GrabWithLeft(NodePosition);
            yield return AimSecondHookAtSameNode();
            Assert.IsInstanceOf<SlingshotState>(Player.CurrentState);
            Assert.IsFalse(Player.CanSlingshot);

            Input.IsLeftTriggerHeld = false;
            Input.IsRightTriggerHeld = false;
            yield return WaitForState<AirborneState>();

            Assert.IsFalse(Player.CanSlingshot);
        }

        [UnityTest]
        public IEnumerator L2_Landing_RefillsSlingshot()
        {
            Player.UseSlingshot();

            yield return LandOnFloor();

            Assert.IsTrue(Player.CanSlingshot);
        }

        [UnityTest]
        public IEnumerator L2_RefillCollectible_RefillsSlingshot()
        {
            Player.UseSlingshot();
            var effectHolder = SpawnNode(Origin + new Vector2(50f, 50f));
            var effect = effectHolder.AddComponent<SlingshotRefillEffect>();

            effect.Apply(Player);
            yield return null;

            Assert.IsTrue(Player.CanSlingshot);
        }

        [UnityTest]
        public IEnumerator L2_LaunchPad_RefillsSlingshot()
        {
            Player.UseSlingshot();

            yield return TouchLaunchPad();

            Assert.IsTrue(Player.CanSlingshot);
        }

        [UnityTest]
        public IEnumerator L2_Respawn_RefillsSlingshot()
        {
            Player.UseSlingshot();

            yield return ResetPlayerAt(Origin);

            Assert.IsTrue(Player.CanSlingshot);
        }

        [UnityTest]
        public IEnumerator L3_GrabbingAHook_DoesNotRefillSlingshot()
        {
            Player.UseSlingshot();

            yield return GrabWithRight(Origin + new Vector2(3f, 4f));
            Assert.IsFalse(Player.CanSlingshot, "tek kanca");

            var left = Origin + new Vector2(-3f, 4f);
            SpawnNode(left);
            Input.LeftStick = (left - Player.PlayerRigidbody.position).normalized;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<DualSwingingState>();

            Assert.IsFalse(Player.CanSlingshot, "cift kanca");
        }

        /// <summary>Slingshot'i firlatir, cikistan sonra en yuksek noktanin cikis konumuna gore yuksekligini dondurur.</summary>
        private IEnumerator LaunchAndMeasureRise(bool jumpHeld, System.Action<float> rise)
        {
            yield return ResetPlayerAt(Origin);
            Input.IsJumpHeld = jumpHeld;
            yield return GrabWithLeft(NodePosition);
            yield return AimSecondHookAtSameNode();
            Assert.IsInstanceOf<SlingshotState>(Player.CurrentState);

            float deadline = Time.unscaledTime + Timeout;
            while (Player.CurrentState is SlingshotState && Time.unscaledTime < deadline)
                yield return new WaitForFixedUpdate();
            Assert.IsInstanceOf<AirborneState>(Player.CurrentState, "slingshot firlatisi bitmedi");

            // Kilit bitince tutulan tetikler yeniden kancaya tutunmasin.
            Input.IsLeftTriggerHeld = Input.IsRightTriggerHeld = false;
            Input.LeftStick = Input.RightStick = Vector2.zero;

            float startY = Player.PlayerRigidbody.position.y;
            float maxY = startY;
            for (int i = 0; i < 100; i++)
            {
                yield return new WaitForFixedUpdate();
                maxY = Mathf.Max(maxY, Player.PlayerRigidbody.position.y);
            }
            rise(maxY - startY);
            Input.IsJumpHeld = false;
            yield return WaitForHitStopEnd();
        }

        [UnityTest]
        public IEnumerator L5_LaunchHeight_DoesNotDependOnJumpButton()
        {
            float released = 0f, held = 0f;

            yield return LaunchAndMeasureRise(false, r => released = r);
            yield return LaunchAndMeasureRise(true, r => held = r);

            Assert.Greater(released, 0.1f, "slingshot yukari firlatmali");
            Assert.AreEqual(released, held, 0.05f, "ziplama tusunu basili tutmak yuksekligi degistirmemeli");
        }

        [UnityTest]
        public IEnumerator L4_WithoutCharge_TwoHooksOnSameNode_BecomesDualSwing()
        {
            Player.UseSlingshot();

            yield return GrabWithLeft(NodePosition);
            yield return AimSecondHookAtSameNode();

            Assert.IsInstanceOf<DualSwingingState>(Player.CurrentState);
        }
    }
}
