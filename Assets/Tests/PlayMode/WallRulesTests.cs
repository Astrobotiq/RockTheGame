using System.Collections;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md W1-W9. ~6 sn bandini PlayerStatsSanityTests korur.</summary>
    public class WallRulesTests : PlayerRuleTestBase
    {
        [UnityTest]
        public IEnumerator W1_Climbing_ConsumesStamina_AndRunningOutDropsThePlayer()
        {
            yield return ClimbRightWall();
            float before = Player.CurrentWallStamina;
            yield return new WaitForSeconds(0.2f);
            Assert.Less(Player.CurrentWallStamina, before);

            Player.ConsumeWallStamina(Player.CurrentWallStamina);
            yield return WaitForAir();

            Assert.IsFalse(Player.CanWallClimb);
        }

        [UnityTest]
        public IEnumerator W2_Landing_RefillsStamina()
        {
            Player.ConsumeWallStamina(Player.Stats.MaxWallStamina);
            Assert.IsFalse(Player.CanWallClimb);

            yield return LandOnFloor();

            Assert.AreEqual(Player.Stats.MaxWallStamina, Player.CurrentWallStamina, 0.0001f);
            Assert.IsTrue(Player.CanWallClimb);
        }

        [UnityTest]
        public IEnumerator W2_LaunchPad_RefillsStamina()
        {
            Player.ConsumeWallStamina(Player.Stats.MaxWallStamina);

            yield return TouchLaunchPad();

            Assert.AreEqual(Player.Stats.MaxWallStamina, Player.CurrentWallStamina, 0.0001f);
            Assert.IsTrue(Player.CanWallClimb);
        }

        [UnityTest]
        public IEnumerator W2_Respawn_RefillsStamina()
        {
            Player.ConsumeWallStamina(Player.Stats.MaxWallStamina);

            yield return ResetPlayerAt(Origin);

            Assert.AreEqual(Player.Stats.MaxWallStamina, Player.CurrentWallStamina, 0.0001f);
            Assert.IsTrue(Player.CanWallClimb);
        }

        [UnityTest]
        public IEnumerator W2_WallJump_DoesNotRefillStamina()
        {
            Player.ConsumeWallStamina(1f);
            float before = Player.CurrentWallStamina;
            yield return SlideOnRightWall();

            yield return PressJump();

            AssertInAir();
            Assert.AreEqual(before, Player.CurrentWallStamina, 0.0001f);
        }

        [UnityTest]
        public IEnumerator W3_Climbing_CannotUseHook()
        {
            yield return ClimbRightWall();
            var node = Origin + new Vector2(-3f, 3f);
            SpawnNode(node);
            Input.RightStick = (node - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;

            yield return WaitFixedSteps(5);

            Assert.IsInstanceOf<WallClimbingState>(Player.CurrentState);
            Assert.IsNull(Player.RightAnchor);
        }

        [UnityTest]
        public IEnumerator W3_Sliding_CanUseHook()
        {
            yield return SlideOnRightWall();
            var node = Player.PlayerRigidbody.position + new Vector2(-3f, 3f);
            SpawnNode(node);
            Input.RightStick = (node - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;

            yield return WaitForState<SwingingState>();

            Assert.IsNotNull(Player.RightAnchor);
        }

        [UnityTest]
        public IEnumerator W4_WallJumpFromSlide_RenewsSlideTime()
        {
            yield return SlideOnRightWall();
            Player.ConsumeWallSlideTime(1f);
            Assert.Less(Player.CurrentWallSlideTime, Player.Stats.MaxWallSlideTime);

            yield return PressJump();

            Assert.AreEqual(Player.Stats.MaxWallSlideTime, Player.CurrentWallSlideTime, 0.0001f);
        }

        [UnityTest]
        public IEnumerator W5_JumpWhileClimbing_GoesStraightUp()
        {
            yield return ClimbRightWall();

            yield return PressJump();

            AssertInAir();
            Assert.AreEqual(0f, Player.Velocity.x, 0.0001f);
            Assert.Greater(Player.Velocity.y, 0f);
        }

        [UnityTest]
        public IEnumerator W5_JumpWhileSliding_GoesAwayFromTheWall()
        {
            yield return SlideOnRightWall();

            yield return PressJump();

            AssertInAir();
            Assert.Less(Player.Velocity.x, 0f);
            Assert.Greater(Player.Velocity.y, 0f);
        }

        /// <summary>Sol yuzeyi oyuncunun sag kenarindan gap kadar uzakta bir duvar (temas yok).</summary>
        private void SpawnWallAtGap(float gap)
        {
            SpawnWall(Player.PlayerRigidbody.position.x + 0.5f + gap);
        }

        [UnityTest]
        public IEnumerator W7_JumpNearWallWhileRising_IsWallJump()
        {
            SpawnWallAtGap(Player.Stats.WallJumpCheckDistance * 0.5f);
            EnterAir(new Vector2(0f, 5f), AirEntry.Fall());

            yield return PressJump();

            AssertInAir();
            Assert.AreEqual(-Player.Stats.WallSlideJumpForce.x, Player.Velocity.x, 0.01f, "duvardan disari atmali");
            Assert.AreEqual(Player.Stats.WallSlideJumpForce.y, Player.Velocity.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator W7_JumpFartherFromWall_IsNotWallJump()
        {
            SpawnWallAtGap(Player.Stats.WallJumpCheckDistance + 0.25f);
            EnterAir(Vector2.zero, AirEntry.Fall());

            yield return PressJump();
            yield return new WaitForSeconds(Player.Stats.JumpBufferDuration * 2f);

            AssertInAir();
            Assert.AreEqual(0f, Player.Velocity.x, 0.01f);
            Assert.LessOrEqual(Player.Velocity.y, 0f, "duvar ziplamasi ya da normal ziplama olmamali");
        }

        [UnityTest]
        public IEnumerator W8_JumpPressedBeforeReachingWall_BecomesWallJump()
        {
            // MoveSpeed (10) ile 0.6 birim 3 fizik adimi: basis karesinde menzil disinda, buffer (0.1 sn) bitmeden menzilde.
            SpawnWallAtGap(Player.Stats.WallJumpCheckDistance + 0.6f);
            Input.LeftStick = Vector2.right;
            EnterAir(new Vector2(Player.Stats.MoveSpeed, 0f), AirEntry.Fall());

            yield return PressJump();
            Assert.Greater(Player.Velocity.x, 0f, "basildigi anda duvar menzilde olmamali");

            float deadline = Time.unscaledTime + Player.Stats.JumpBufferDuration + 0.1f;
            while (Player.Velocity.x >= 0f && Time.unscaledTime < deadline)
                yield return null;

            Assert.Less(Player.Velocity.x, 0f, "buffer'daki ziplama duvara varinca duvar ziplamasi olmali");
            Assert.Greater(Player.Velocity.y, 0f);
        }

        [UnityTest]
        public IEnumerator W9_JumpRightAfterLettingGoOfWall_IsWallJump()
        {
            yield return SlideOnRightWall();
            Input.LeftStick = Vector2.left;
            yield return WaitForAir();
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(Player.IsWallWithin(1, Player.Stats.WallJumpCheckDistance), "duvar hala menzilde olmali");

            // Zıplama tek karede basılı: frame fazından bas, bir Update görsün (oyundaki WasPressedThisFrame gibi).
            yield return null;
            yield return PressJump();
            yield return new WaitForFixedUpdate();

            Assert.Less(Player.Velocity.x, -Player.Stats.WallSlideJumpForce.x * 0.5f, $"duvardan disari atmali, hiz {Player.Velocity}");
        }

        private IEnumerator ClimbToLedge()
        {
            // Bel isini duvara carpmali, bas isini bosa gitmeli: duvarin ustu ikisinin arasinda.
            float top = Player.PlayerRigidbody.position.y + 0.15f;
            SpawnWall(Origin.x + WallTouchOffset, top);
            Input.IsRightBumperHeld = true;
            yield return WaitForState<WallClimbingState>();
        }

        [UnityTest]
        public IEnumerator W6_LedgeClimb_NeedsStickHeldUp()
        {
            yield return ClimbToLedge();

            Input.LeftStick = Vector2.up;
            yield return null;
            Assert.IsInstanceOf<WallClimbingState>(Player.CurrentState, "tek karelik basis cikmamali");

            yield return WaitForState<LedgeClimbState>();
        }

        [UnityTest]
        public IEnumerator W6_LedgeClimb_DoesNotStartWithoutStick()
        {
            yield return ClimbToLedge();

            yield return new WaitForSeconds(Player.Stats.LedgeClimbHoldTime * 2f);

            Assert.IsInstanceOf<WallClimbingState>(Player.CurrentState);
        }
    }
}
