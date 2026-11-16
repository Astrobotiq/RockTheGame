using System;
using System.Collections;
using System.Collections.Generic;
using New_Scripts.Platform;
using New_Scripts.Player;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// docs/bozulmamali-davranislar.md H1-H4: havaya hangi yoldan girildiyse o yerçekimi kuralı.
    /// Her fizik adımındaki dikey hız değişimi taban adıma (Gravity x fixedDeltaTime) bölünerek çarpan ölçülür.
    /// </summary>
    public class AirEntryRulesTests : PlayerRuleTestBase
    {
        private const float MultiplierTolerance = 0.01f;

        private float BaseStep => Player.Stats.Gravity * Time.fixedDeltaTime;

        /// <summary>Oyuncu havadayken her fizik adımından sonraki dikey hızı kaydeder.</summary>
        private IEnumerator SampleAirborne(List<float> samples, int maxSteps = 120)
        {
            samples.Add(Player.Velocity.y);
            for (int i = 0; i < maxSteps && Player.CurrentState is AirborneState; i++)
            {
                yield return new WaitForFixedUpdate();
                if (!(Player.CurrentState is AirborneState)) break;
                samples.Add(Player.Velocity.y);
            }
        }

        /// <summary>Önceki hızı koşulu sağlayan ilk adımın çarpanı.</summary>
        private float MultiplierWhere(List<float> samples, Func<float, bool> before, string what)
        {
            for (int i = 1; i < samples.Count; i++)
            {
                float prev = samples[i - 1];
                float next = samples[i];
                if (before(prev) && Math.Sign(prev) == Math.Sign(next))
                    return (next - prev) / BaseStep;
            }
            Assert.Fail($"{what} adımı yok; ölçülen hızlar: {string.Join(", ", samples)}");
            return 0f;
        }

        private float Rising(List<float> s) =>
            MultiplierWhere(s, vy => vy > Player.Stats.ApexThreshold, "yükseliş (tepeden uzak)");

        private float NearApexRising(List<float> s) =>
            MultiplierWhere(s, vy => vy > 0f && vy < Player.Stats.ApexThreshold, "tepeye yakın yükseliş");

        private float Falling(List<float> s) =>
            MultiplierWhere(s, vy => vy < 0f, "düşüş");

        private IEnumerator JumpFromFloor(bool jumpHeld, List<float> samples)
        {
            yield return LandOnFloor();
            yield return WaitForHitStopEnd();
            Input.IsJumpHeld = jumpHeld;
            yield return PressJump();
            yield return WaitForState<AirborneState>();
            yield return SampleAirborne(samples);
        }

        [UnityTest]
        public IEnumerator H1_GroundJump_HeldButton_FullRiseApexHangAndFastFall()
        {
            var s = new List<float>();
            yield return JumpFromFloor(true, s);

            Assert.AreEqual(1f, Rising(s), MultiplierTolerance, "basılı yükseliş");
            Assert.AreEqual(Player.Stats.ApexHangGravityMultiplier, NearApexRising(s), MultiplierTolerance, "tepede asılı kalma");
            Assert.AreEqual(Player.Stats.FallGravityMultiplier, Falling(s), MultiplierTolerance, "düşüş");
        }

        [UnityTest]
        public IEnumerator H1_GroundJump_ReleasedButton_CutsRise()
        {
            var s = new List<float>();
            yield return JumpFromFloor(false, s);

            Assert.AreEqual(Player.Stats.JumpEndEarlyGravityMultiplier, Rising(s), MultiplierTolerance, "bırakılmış yükseliş");
            Assert.AreEqual(Player.Stats.FallGravityMultiplier, Falling(s), MultiplierTolerance, "düşüş");
        }

        [UnityTest]
        public IEnumerator H2_SwingRelease_IgnoresJumpButton()
        {
            var anchor = Origin + new Vector2(3f, 0f);
            Input.IsJumpHeld = true;
            StartSwingFrom(anchor);

            // Dibi geçip karşı tarafta yükselirken bırak.
            float deadline = Time.unscaledTime + Timeout;
            while (!(Player.PlayerRigidbody.position.x > anchor.x && Player.Velocity.y > 2f * Player.Stats.ApexThreshold)
                   && Player.CurrentState is SwingingState && Time.unscaledTime < deadline)
                yield return new WaitForFixedUpdate();
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState, $"bırakma noktasına varılmadı, hız {Player.Velocity}");

            Input.IsRightTriggerHeld = false;
            yield return WaitForState<AirborneState>();
            var s = new List<float>();
            yield return SampleAirborne(s);

            float cut = Player.Stats.JumpEndEarlyGravityMultiplier;
            Assert.AreEqual(cut, Rising(s), MultiplierTolerance, "tuş basılı olsa da yükseliş kesik");
            Assert.AreEqual(cut, NearApexRising(s), MultiplierTolerance, "tepede asılı kalma yok");
            Assert.AreEqual(Player.Stats.FallGravityMultiplier, Falling(s), MultiplierTolerance, "düşüş");
        }

        [UnityTest]
        public IEnumerator H2_LaunchPad_IgnoresJumpButton()
        {
            Input.IsJumpHeld = true;
            yield return TouchLaunchPad();
            Assert.IsInstanceOf<AirborneState>(Player.CurrentState);

            var s = new List<float>();
            yield return SampleAirborne(s, 20);

            Assert.AreEqual(Player.Stats.JumpEndEarlyGravityMultiplier, Rising(s), MultiplierTolerance,
                "tuş basılı olsa da yükseliş kesik");
        }

        /// <summary>Zıplama boost'u olan, kendisi hareket etmeyen yüzey.</summary>
        private sealed class BoostSurface : MonoBehaviour, IMovingSurface
        {
            public Vector2 DeltaPosition => Vector2.zero;
            public Vector2 SurfaceVelocity => Vector2.zero;
            public float JumpBoostMultiplier => 1f;
        }

        [UnityTest]
        public IEnumerator H3_BoostSurfaceJump_SofterCutRise_NoApexHang()
        {
            var floor = SpawnFloor(Player.PlayerRigidbody.position.y - 1f);
            floor.AddComponent<BoostSurface>();
            yield return WaitForState<GroundedState>();
            yield return WaitForHitStopEnd();

            Input.IsJumpHeld = true;
            yield return PressJump();
            yield return WaitForState<AirborneState>();
            var s = new List<float>();
            yield return SampleAirborne(s);

            // GroundedState boost'lu yüzeyde endEarlyGravityMultiplier 0.5 veriyor.
            float soft = Player.Stats.JumpEndEarlyGravityMultiplier * 0.5f;
            Assert.AreEqual(soft, Rising(s), MultiplierTolerance, "tuş basılı olsa da yumuşak kesik yükseliş");
            Assert.AreEqual(soft, NearApexRising(s), MultiplierTolerance, "tepede asılı kalma yok");
            Assert.AreEqual(Player.Stats.FallGravityMultiplier, Falling(s), MultiplierTolerance, "düşüş");
        }

        [UnityTest]
        public IEnumerator H4_DashEnd_ConstantGravity()
        {
            Input.IsJumpHeld = true;
            Input.LeftStick = Vector2.up;
            yield return PressDash();
            yield return WaitForState<DashState>();
            yield return WaitForState<AirborneState>();
            Input.LeftStick = Vector2.zero;

            var s = new List<float>();
            yield return SampleAirborne(s);

            Assert.AreEqual(1f, Rising(s), MultiplierTolerance, "yükseliş");
            Assert.AreEqual(1f, NearApexRising(s), MultiplierTolerance, "tepe");
            Assert.AreEqual(1f, Falling(s), MultiplierTolerance, "düşüş hızlanmamalı");
        }
    }
}
