using System.Collections;
using New_Scripts.Collectible;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md L1-L4.</summary>
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
