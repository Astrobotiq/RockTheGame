using System.Collections;
using System.Collections.Generic;
using New_Scripts.Player.Nodes.Spiral;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md "Spiral node" (SP1-SP7). Tasarim: docs/spiral-node-tasarimi.md.</summary>
    public class SpiralNodeRulesTests : PlayerRuleTestBase
    {
        private const float Rope = 4f;

        private SpiralNode SpawnSpiral(Vector2 position, SpinDirection direction, float reverseEvery = 0f)
        {
            GameObject node = SpawnNode(position);
            var spiral = node.AddComponent<SpiralNode>();
            SetField(spiral, "direction", direction);
            SetField(spiral, "reverseEveryDegrees", reverseEvery);
            return spiral;
        }

        /// <summary>Oyuncu havada durgun, spiral rope kadar ustunde; sag kolla tutunur.</summary>
        private IEnumerator GrabFromRestBelow(SpiralNode spiral, float rope = Rope)
        {
            Teleport((Vector2)spiral.transform.position - new Vector2(0f, rope));
            EnterAir(Vector2.zero, AirEntry.Fall());
            yield return GrabExistingWithRight(spiral.gameObject);
        }

        /// <summary>Oyuncunun spiral etrafindaki teget hizi; pozitif = CCW.</summary>
        private float TangentialSpeed(Vector2 anchor)
        {
            Vector2 d = (anchor - Player.PlayerRigidbody.position).normalized;
            return Vector2.Dot(Player.Velocity, new Vector2(d.y, -d.x));
        }

        private Vector2 SpinTangent(Vector2 anchor, int spinSign)
        {
            Vector2 d = (anchor - Player.PlayerRigidbody.position).normalized;
            return new Vector2(d.y, -d.x) * spinSign;
        }

        // ---- SP1 ----

        [UnityTest]
        public IEnumerator SP1_FromRest_CompletesLoopInSpinDirection(
            [Values(2f, 6f)] float rope, [Values(SpinDirection.CounterClockwise, SpinDirection.Clockwise)] SpinDirection direction)
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, rope), direction);
            yield return GrabFromRestBelow(spiral, rope);

            float angle = 0f;
            yield return MeasureRotation(spiral.transform.position, 300, a => angle = a);

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState, "spiralden dusmemeli");
            Assert.GreaterOrEqual(Mathf.Abs(angle), 360f, $"6 sn icinde tam tur yok, aci {angle}");
            Assert.AreEqual(spiral.SpinSign, (int)Mathf.Sign(angle), $"donus yonu spiralinkiyle ayni olmali, aci {angle}");
        }

        // ---- SP2 ----

        [UnityTest]
        public IEnumerator SP2_GrabAgainstSpin_SlowsThenReverses()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            Vector2 anchor = spiral.transform.position;
            EnterAir(new Vector2(-12f, 0f), AirEntry.Fall());
            yield return GrabExistingWithRight(spiral.gameObject);

            yield return new WaitForFixedUpdate();
            Assert.Less(TangentialSpeed(anchor), 0f, "tutunur tutunmaz donmemeli");

            int steps = 0;
            while (TangentialSpeed(anchor) <= 0f && steps < 100)
            {
                yield return new WaitForFixedUpdate();
                steps++;
            }
            Assert.Less(steps, 100, "2 sn icinde spiral yonune donmedi");

            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.Greater(TangentialSpeed(anchor), 0f, $"dondukten {i} adim sonra yine ters yone gecti");
            }

            // Serbest sarkac da geri salinir; spiral ise oyuncuyu kendi yonunde tam tura tasir.
            float angle = 0f;
            yield return MeasureRotation(anchor, 250, a => angle = a);
            Assert.GreaterOrEqual(angle, 360f, "dondukten sonra spiral yonunde tam tur atmali");
        }

        [UnityTest]
        public IEnumerator SP2b_GroundGrabAgainstSpin_ReversesAfterHop()
        {
            yield return LandOnFloor();
            yield return WaitForHitStopEnd();
            // Node neredeyse ustte, biraz sagda: hop saga gider ve oyuncuyu node'un altindan CCW'ye sokar.
            var spiral = SpawnSpiral(Player.PlayerRigidbody.position + new Vector2(1f, 4f), SpinDirection.Clockwise);
            Vector2 anchor = spiral.transform.position;
            yield return GrabExistingWithRight(spiral.gameObject);

            int steps = 0;
            while (TangentialSpeed(anchor) > -1f && steps < 125 && Player.CurrentState is SwingingState)
            {
                yield return new WaitForFixedUpdate();
                steps++;
            }
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState, "spiralden dusmemeli");
            Assert.Less(steps, 125, "2.5 sn icinde CW donuse gecmedi");

            float angle = 0f;
            yield return MeasureRotation(anchor, 300, a => angle = a);
            Assert.LessOrEqual(angle, -360f, "dondukten sonra spiral yonunde (CW) tam tur atmali");
        }

        // ---- SP3 ----

        [UnityTest]
        public IEnumerator SP3_StickAgainstSpin_NeverStopsOrReverses()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            Vector2 anchor = spiral.transform.position;
            yield return GrabFromRestBelow(spiral);
            yield return WaitFixedSteps(50);

            Vector2 previous = Player.PlayerRigidbody.position - anchor;
            for (int i = 0; i < 150; i++)
            {
                Input.RightStick = -SpinTangent(anchor, spiral.SpinSign);
                yield return new WaitForFixedUpdate();
                Vector2 current = Player.PlayerRigidbody.position - anchor;
                float delta = Vector2.SignedAngle(previous, current);
                previous = current;
                Assert.GreaterOrEqual(delta, -0.01f, $"adim {i}: stick donusu durdurdu ya da cevirdi");
            }
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
        }

        private IEnumerator AverageSpeedFrom2To4Seconds(SpiralNode spiral, bool stickWithSpin, System.Action<float> result)
        {
            Vector2 anchor = spiral.transform.position;
            Input.RightStick = Vector2.zero;
            yield return GrabFromRestBelow(spiral);
            float sum = 0f;
            int count = 0;
            for (int i = 0; i < 200; i++)
            {
                Input.RightStick = stickWithSpin ? SpinTangent(anchor, spiral.SpinSign) : Vector2.zero;
                yield return new WaitForFixedUpdate();
                if (i < 100) continue;
                sum += Mathf.Abs(TangentialSpeed(anchor));
                count++;
            }
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
            result(sum / count);

            Input.Clear();
            yield return WaitForAir();
            yield return ResetPlayerAt(Origin);
        }

        [UnityTest]
        public IEnumerator SP3b_StickWithSpin_FasterThanNeutral()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            float neutral = 0f, withSpin = 0f;
            yield return AverageSpeedFrom2To4Seconds(spiral, false, v => neutral = v);
            yield return AverageSpeedFrom2To4Seconds(spiral, true, v => withSpin = v);

            Assert.Greater(neutral, 12f, "stick nötrken de spiral döndürmeli");
            Assert.Greater(withSpin, neutral * 1.15f, $"nötr {neutral}, spin yönünde {withSpin}");
        }

        // ---- SP5 ----

        [UnityTest]
        public IEnumerator SP5_Release_IsNormalSwingRelease()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            Vector2 anchor = spiral.transform.position;
            Input.IsJumpHeld = true;
            yield return GrabFromRestBelow(spiral);
            yield return WaitFixedSteps(75);

            // CCW: sag tarafta yukselirken birak (H2 ile ayni nokta).
            int guard = 0;
            while (!(Player.PlayerRigidbody.position.x > anchor.x && Player.Velocity.y > 2f * Player.Stats.ApexThreshold) && guard++ < 300)
                yield return new WaitForFixedUpdate();
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState, $"birakma noktasina varilmadi, hiz {Player.Velocity}");

            Vector2 releaseVelocity = Player.Velocity;
            Input.IsRightTriggerHeld = false;
            yield return null;
            AssertInAir("birakinca normal havaya gecmeli");
            Assert.AreEqual(releaseVelocity.x, Player.Velocity.x, 0.5f, "birakista yatay hiz korunmali");
            Assert.AreEqual(releaseVelocity.y, Player.Velocity.y, 0.5f, "birakista dikey hiz korunmali");

            // H2: tus basili olsa da yukselis kesik (JumpEndEarlyGravityMultiplier).
            float baseStep = Player.Stats.Gravity * Time.fixedDeltaTime;
            float before = Player.Velocity.y;
            float multiplier = float.NaN;
            for (int i = 0; i < 30 && Player.IsInNormalAir; i++)
            {
                yield return new WaitForFixedUpdate();
                float now = Player.Velocity.y;
                if (before > Player.Stats.ApexThreshold)
                {
                    multiplier = (now - before) / baseStep;
                    break;
                }
                before = now;
            }
            Assert.AreEqual(Player.Stats.JumpEndEarlyGravityMultiplier, multiplier, 0.01f, "spiral birakisi H2 gibi kesik yukselmeli");
        }
    }
}
