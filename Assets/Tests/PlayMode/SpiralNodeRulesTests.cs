using System.Collections;
using System.Collections.Generic;
using New_Scripts.Player.Nodes.Spiral;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md "Spiral node" (SP1-SP8). Tasarim: docs/spiral-node-tasarimi.md.</summary>
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

        // ---- SP5 / SP8 (docs/spiral-cikis-hizi.md) ----

        /// <summary>Spirale durgun tutunur, 1.5 sn sonra kosul saglaninca birakir. Birakis anindaki hizi ve spin tegetini verir.</summary>
        private IEnumerator ReleaseWhen(SpiralNode spiral, System.Func<Vector2, bool> condition, System.Action<Vector2, Vector2> result)
        {
            Vector2 anchor = spiral.transform.position;
            yield return GrabFromRestBelow(spiral);
            yield return WaitFixedSteps(75);

            int guard = 0;
            while (!condition(anchor) && guard++ < 300)
                yield return new WaitForFixedUpdate();
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState, $"birakma noktasina varilmadi, hiz {Player.Velocity}");

            Vector2 releaseVelocity = Player.Velocity;
            Vector2 tangent = SpinTangent(anchor, spiral.SpinSign);
            Input.IsRightTriggerHeld = false;
            yield return null;
            AssertInAir("birakinca normal havaya gecmeli");
            result(releaseVelocity, tangent);
        }

        // CCW: sag tarafta yukselirken (H2 ile ayni nokta).
        private bool RisingOnRight(Vector2 anchor) =>
            Player.PlayerRigidbody.position.x > anchor.x && Player.Velocity.y > 2f * Player.Stats.ApexThreshold;

        // CCW: tabanda saga giderken; hiz neredeyse tamamen yatay.
        private bool AtBottomMovingRight(Vector2 anchor) =>
            Player.PlayerRigidbody.position.y < anchor.y - Rope * 0.97f && Player.Velocity.x > 0f;

        private bool InAirBrakeHold => Player.CurrentState is NormalState { IsInAirBrakeHold: true };

        [UnityTest]
        public IEnumerator SP5_Release_KeepsH2Gravity()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            Input.IsJumpHeld = true;
            yield return ReleaseWhen(spiral, RisingOnRight, (v, t) => { });

            // H2: tus basili olsa da yukselis kesik (JumpEndEarlyGravityMultiplier); SP8 yercekimini degistirmez.
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

        [UnityTest]
        public IEnumerator SP8a_Release_AddsBoostAlongSpinTangent()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            Vector2 release = default, tangent = default;
            yield return ReleaseWhen(spiral, RisingOnRight, (v, t) => { release = v; tangent = t; });

            float boost = Player.Stats.SpiralExitBoost;
            Assert.Greater(boost, 0f, "kurulum: SpiralExitBoost > 0");
            Assert.GreaterOrEqual(Player.Velocity.magnitude, release.magnitude + boost - 0.5f,
                $"cikis hizi birakis + itme olmali; birakis {release}, simdi {Player.Velocity}");
            Assert.AreEqual(boost, Vector2.Dot(Player.Velocity - release, tangent), 0.5f, "itme spin tegetinde olmali");
            Assert.IsTrue(InAirBrakeHold, "spiral cikisinda fren tutma suresi baslamali");
        }

        [UnityTest]
        public IEnumerator SP8b_HoldKeepsHorizontalSpeed_ThenBrakeReturns()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            yield return ReleaseWhen(spiral, AtBottomMovingRight, (v, t) => { });

            float startX = Player.Velocity.x;
            Assert.Greater(startX, Player.Stats.MoveSpeed * Player.Stats.AirControlMultiplier, "kurulum: hava sinirinin ustunde");

            int holdSteps = Mathf.FloorToInt(Player.Stats.SpiralExitHoldTime / Time.fixedDeltaTime) - 2;
            for (int i = 0; i < holdSteps; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(startX, Player.Velocity.x, 0.01f, $"adim {i}: tutma suresinde stick notrken yatay hiz korunmali");
            }

            yield return WaitFixedSteps(4);
            Assert.IsFalse(InAirBrakeHold, "tutma suresi bitmeli");
            Assert.Less(Player.Velocity.x, startX - 0.5f, "tutma bitince hava freni geri gelmeli");
        }

        [UnityTest]
        public IEnumerator SP8c_ReleaseAgainstSpin_NoBoost()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            EnterAir(new Vector2(-12f, 0f), AirEntry.Fall());
            yield return GrabExistingWithRight(spiral.gameObject);
            yield return WaitFixedSteps(2);
            Assert.Less(TangentialSpeed(spiral.transform.position), 0f, "kurulum: hala spinin tersine");

            Vector2 release = Player.Velocity;
            Input.IsRightTriggerHeld = false;
            yield return null;
            AssertInAir();
            Assert.AreEqual(release.x, Player.Velocity.x, 0.5f, "spinin tersine birakista itme olmamali");
            Assert.AreEqual(release.y, Player.Velocity.y, 0.5f);
            Assert.IsFalse(InAirBrakeHold, "spinin tersine birakista fren tutma olmamali");
        }

        [UnityTest]
        public IEnumerator SP8d_NormalNodeRelease_Unchanged()
        {
            GameObject node = SpawnNode(Origin + new Vector2(0f, Rope));
            Teleport(Origin + new Vector2(-2f, 0f));
            EnterAir(new Vector2(12f, 0f), AirEntry.Fall());
            yield return GrabExistingWithRight(node);
            yield return WaitFixedSteps(10);
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);

            Vector2 release = Player.Velocity;
            Input.IsRightTriggerHeld = false;
            yield return null;
            AssertInAir();
            Assert.AreEqual(release.x, Player.Velocity.x, 0.5f, "normal node birakisi degismemeli");
            Assert.AreEqual(release.y, Player.Velocity.y, 0.5f);
            Assert.IsFalse(InAirBrakeHold, "normal node birakisinda fren tutma olmamali");
        }

        // ---- SP4 ----

        [UnityTest]
        public IEnumerator SP4_SpiralPushesIntoWall_BreaksGrapple([Values(1f, 2f, 3f, 4.2f)] float wallOffset, [Values(4f, 7f)] float rope)
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, rope), SpinDirection.CounterClockwise);
            // CCW oyuncuyu saga goturur; duvar donus dairesinin icinde.
            SpawnWall(spiral.transform.position.x + wallOffset);
            yield return GrabFromRestBelow(spiral, rope);

            int steps = 0;
            while (Player.CurrentState is SwingingState && steps < 75)
            {
                yield return new WaitForFixedUpdate();
                steps++;
            }
            Assert.IsFalse(Player.CurrentState is SwingingState,
                $"1.5 sn icinde kanca kopmadi, oyuncu duvarda takili; konum {Player.PlayerRigidbody.position - Origin}");
            Assert.IsNull(Player.RightAnchor, "kopunca anchor silinmeli");
            Assert.IsFalse(InAirBrakeHold, "kopmada spiral cikis hizi verilmemeli (SP8)");

            Input.IsRightTriggerHeld = false;
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.IsNotInstanceOf<SwingingState>(Player.CurrentState, "kopunca tekrar spirale donmemeli");
            }
        }

        // ---- SP6 ----

        [UnityTest]
        public IEnumerator SP6_DualSwing_MotorOff()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            yield return GrabFromRestBelow(spiral);
            Assert.IsTrue(spiral.IsConnected, "tek kancada spiral bagli olmali");

            Vector2 other = (Vector2)spiral.transform.position + new Vector2(-4f, 0f);
            SpawnNode(other);
            Input.LeftStick = (other - Player.PlayerRigidbody.position).normalized;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<DualSwingingState>();

            Assert.IsFalse(spiral.IsConnected, "cift kancada motor calismamali");
        }

        [UnityTest]
        public IEnumerator SP6b_SameSpiralBothArms_IsNormalSlingshot()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            yield return GrabFromRestBelow(spiral);
            Assert.IsTrue(Player.CanSlingshot, "slingshot hakki dolu olmali");

            Input.LeftStick = ((Vector2)spiral.transform.position - Player.PlayerRigidbody.position).normalized;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<SlingshotState>();
            Assert.IsFalse(spiral.IsConnected);
        }

        // ---- SP7 ----

        /// <summary>Spin isareti degisene kadar bekler (en fazla maxSteps); o ana kadar biriken isaretli aciyi verir.</summary>
        private IEnumerator WaitForFlip(SpiralNode spiral, int maxSteps, System.Action<float, bool> result)
        {
            int startSign = spiral.SpinSign;
            Vector2 anchor = spiral.transform.position;
            Vector2 previous = Player.PlayerRigidbody.position - anchor;
            float total = 0f;
            for (int i = 0; i < maxSteps; i++)
            {
                yield return new WaitForFixedUpdate();
                Vector2 current = Player.PlayerRigidbody.position - anchor;
                total += Vector2.SignedAngle(previous, current);
                previous = current;
                if (spiral.SpinSign != startSign)
                {
                    result(total, true);
                    yield break;
                }
            }
            result(total, false);
        }

        [UnityTest]
        public IEnumerator SP7_Reversing_FlipsAfter360AndRepeats()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise, 360f);
            Vector2 anchor = spiral.transform.position;
            int reversedEvents = 0;
            spiral.Reversed += () => reversedEvents++;
            yield return GrabFromRestBelow(spiral);
            Assert.AreEqual(1, spiral.SpinSign);

            float angle = 0f;
            bool flipped = false;
            yield return WaitForFlip(spiral, 300, (a, f) => { angle = a; flipped = f; });
            Assert.IsTrue(flipped, $"6 sn icinde yon degismedi, aci {angle}");
            Assert.AreEqual(-1, spiral.SpinSign);
            // Olcum tutunma frame'inden baslar, sayac ilk motor adimindan: aradaki birkac adim (adim basina ~5-8°) ust payda.
            Assert.That(angle, Is.InRange(350f, 400f), "ilk 360° CCW birikince cevrilmeli");
            Assert.AreEqual(1, reversedEvents, "Reversed olayi bir kez");

            int steps = 0;
            while (TangentialSpeed(anchor) > -1f && steps < 150)
            {
                yield return new WaitForFixedUpdate();
                steps++;
            }
            Assert.Less(steps, 150, "cevrildikten sonra 3 sn icinde CW donmeye baslamadi");

            yield return WaitForFlip(spiral, 450, (a, f) => { angle = a; flipped = f; });
            Assert.IsTrue(flipped, "tutundukca ikinci kez cevrilmeli");
            Assert.AreEqual(1, spiral.SpinSign);
            Assert.AreEqual(2, reversedEvents);
            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
        }

        private IEnumerator GrabReversingAndFlipOnce(System.Action<SpiralNode> onFlipped)
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise, 360f);
            yield return GrabFromRestBelow(spiral);
            bool flipped = false;
            yield return WaitForFlip(spiral, 300, (a, f) => flipped = f);
            Assert.IsTrue(flipped, "kurulum: spiral cevrilmeli");
            Assert.AreEqual(-1, spiral.SpinSign);
            onFlipped(spiral);
        }

        [UnityTest]
        public IEnumerator SP7b_Release_RestoresInitialDirection()
        {
            SpiralNode spiral = null;
            yield return GrabReversingAndFlipOnce(s => spiral = s);

            Input.IsRightTriggerHeld = false;
            yield return WaitForAir();

            Assert.AreEqual(1, spiral.SpinSign, "birakinca ilk yonune donmeli");
            Assert.IsFalse(spiral.IsConnected);
            Assert.AreEqual(0f, spiral.ReverseProgress, 0.0001f);
        }

        [UnityTest]
        public IEnumerator SP7c_Respawn_RestoresInitialDirection()
        {
            SpiralNode spiral = null;
            yield return GrabReversingAndFlipOnce(s => spiral = s);

            // Respawn yolu (ResetPlayerAt ile ayni) ama frame beklemeden: tetik hala basili, oyuncu bir sonraki
            // frame'de yakindaki spirale yeniden tutunabilir.
            Player.OnStartRespawn();
            Teleport(Origin);
            Player.OnEndRespawn();

            Assert.AreEqual(1, spiral.SpinSign, "respawn'da ilk yonune donmeli");
            Assert.IsFalse(spiral.IsConnected);
        }

        [UnityTest]
        public IEnumerator SP_RespawnTransition_ReleasesFacingLockImmediately()
        {
            // Ölüm geçişi (iris kapanıp açılırken) oyuncu duraklatılır; SwingingState ancak OnEndRespawn'da çıkar.
            // Yön kilidi ve döngü sesi geçiş boyunca sürmemeli.
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.Clockwise);
            yield return GrabFromRestBelow(spiral);
            yield return WaitFixedSteps(10);
            Assert.AreEqual(-1, Player.SpiralFacingSign, "kurulum: spiralde yon kilitli");

            Player.OnStartRespawn();
            for (int i = 0; i < 10; i++) yield return null;
            Assert.AreEqual(0, Player.SpiralFacingSign, "respawn gecisinde yon kilidi kalkmali");

            Input.Clear();
            Teleport(Origin);
            Player.OnEndRespawn();
        }

        [UnityTest]
        public IEnumerator SP7d_NormalSpiral_NeverFlips()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            yield return GrabFromRestBelow(spiral);

            float angle = 0f;
            bool flipped = false;
            yield return WaitForFlip(spiral, 500, (a, f) => { angle = a; flipped = f; });

            Assert.IsFalse(flipped, "normal spiral hic yon degistirmemeli");
            Assert.GreaterOrEqual(angle, 720f, "kurulum: en az iki tur donmeli");
        }

        // ---- Sprite yonu (tasarim §8) ----

        [UnityTest]
        public IEnumerator SP_FacingLockedToSpin()
        {
            var animation = Player.GetComponentInChildren<New_Scripts.Player.Visual.PlayerAnimationController>();
            Assert.IsNotNull(animation, "PlayerAnimationController bulunamadi");
            var field = typeof(New_Scripts.Player.Visual.PlayerAnimationController).GetField("spriteRenderer",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var sprite = (SpriteRenderer)field.GetValue(animation);
            Assert.IsNotNull(sprite);

            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.Clockwise);
            yield return GrabFromRestBelow(spiral);

            // Tam bir turu kapsar: tepeden gecerken hiz saga dondugunde de sprite sola bakmali.
            for (int i = 0; i < 150; i++)
            {
                yield return null;
                Assert.IsTrue(sprite.flipX, $"frame {i}: CW spiralde sprite sola bakmali, hiz {Player.Velocity}");
            }

            Input.IsRightTriggerHeld = false;
            yield return WaitForAir();
            Assert.AreEqual(0, Player.SpiralFacingSign, "birakinca yon kilidi kalkmali");
        }

        // ---- Dikkat edilecekler ----

        [UnityTest]
        public IEnumerator SP_NoFullRotationOnSpiral()
        {
            GameObject node = SpawnNode(Origin + new Vector2(0f, Rope));
            node.SetActive(false);
            var trigger = node.AddComponent<New_Scripts.Player.Nodes.Rotation.RotationEventTrigger>();
            trigger.OnRotationCompleted = new UnityEngine.Events.UnityEvent();
            int completions = 0;
            trigger.OnRotationCompleted.AddListener(() => completions++);
            var rotation = node.AddComponent<New_Scripts.Player.Nodes.Rotation.FullRotationNode>();
            SetField(rotation, "rotationEffectComponent", trigger);
            var spiral = node.AddComponent<SpiralNode>();
            node.SetActive(true);
            yield return null;

            yield return GrabFromRestBelow(spiral);
            float angle = 0f;
            yield return MeasureRotation(node.transform.position, 300, a => angle = a);

            Assert.GreaterOrEqual(angle, 360f, "test kurulumu: spiral en az bir tur dondurmeli");
            Assert.AreEqual(0, completions, "spiral varken donen node efekti tetiklenmemeli");
        }

        [UnityTest]
        public IEnumerator SP_NodeDeactivated_NoException()
        {
            var spiral = SpawnSpiral(Origin + new Vector2(0f, Rope), SpinDirection.CounterClockwise);
            yield return GrabFromRestBelow(spiral);
            yield return WaitFixedSteps(10);

            spiral.gameObject.SetActive(false);
            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.IsInstanceOf<SwingingState>(Player.CurrentState, $"adim {i}: node pasiflesince kanca kopmamali (M1)");
            }
        }
    }
}
