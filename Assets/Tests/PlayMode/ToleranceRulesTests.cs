using System.Collections;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// docs/bozulmamali-davranislar.md T1 ve T2 (kose duzeltmesi). Degme miktarlari PlayerStats degerine gore secilir:
    /// gecen durum degerin yarisi, takilan durum degerin 0.15 fazlasi (carpisma kutusu her yandan 0.05 kucuk).
    /// </summary>
    public class ToleranceRulesTests : PlayerRuleTestBase
    {
        private const float HalfSize = 0.5f;

        private float PassOverlap(float correction) => correction * 0.5f;
        private float BlockOverlap(float correction) => correction + 0.15f;

        /// <summary>Oyuncunun ustunde, sol kenari oyuncunun sag kenarindan overlap kadar iceride bir tavan; alt yuzeyini dondurur.</summary>
        private float SpawnCeilingCorner(float overlap)
        {
            Vector2 p = Player.PlayerRigidbody.position;
            float bottom = p.y + HalfSize + 1f;
            float left = p.x + HalfSize - overlap;
            SpawnBox("Ceiling", new Vector2(left + 5f, bottom + 0.5f), new Vector2(10f, 1f), 0f);
            return bottom;
        }

        private IEnumerator JumpStraightUpAndTrackPeak(System.Action<float> peak)
        {
            Input.IsJumpHeld = true;
            Player.TransitionToState(new AirborneState(Player, new Vector2(0f, Player.Stats.JumpVelocity), isJumping: true));

            float maxY = float.MinValue;
            for (int i = 0; i < 50; i++)
            {
                yield return new WaitForFixedUpdate();
                maxY = Mathf.Max(maxY, Player.PlayerRigidbody.position.y);
            }
            peak(maxY);
        }

        [UnityTest]
        public IEnumerator T1_JumpGrazingCeilingCorner_SlidesPast()
        {
            float bottom = SpawnCeilingCorner(PassOverlap(Player.Stats.CeilingCornerCorrection));
            float maxY = 0f;

            yield return JumpStraightUpAndTrackPeak(y => maxY = y);

            Assert.Greater(maxY, bottom, "oyuncu tavanin yanindan gecip yukari cikmali");
        }

        [UnityTest]
        public IEnumerator T1_JumpHittingCeilingFurther_IsBlocked()
        {
            float bottom = SpawnCeilingCorner(BlockOverlap(Player.Stats.CeilingCornerCorrection));
            float maxY = 0f;

            yield return JumpStraightUpAndTrackPeak(y => maxY = y);

            Assert.Less(maxY, bottom - HalfSize + 0.01f, "oyuncu tavana takilmali");
        }

        /// <summary>2 birim ilerde, ustu oyuncunun alt kenarindan overlap kadar yukarida bir basamak; sol yuzeyini dondurur.</summary>
        private float SpawnStep(float overlap)
        {
            Vector2 p = Player.PlayerRigidbody.position;
            float face = p.x + HalfSize + 1.5f;
            SpawnWall(face, p.y - HalfSize + overlap);
            return face;
        }

        private IEnumerator DashRight()
        {
            Input.LeftStick = Vector2.right;
            yield return PressDash();
            Assert.IsInstanceOf<DashState>(Player.CurrentState);
            yield return WaitWhileState<DashState>();
        }

        [UnityTest]
        public IEnumerator T2_DashGrazingStep_SlidesOver()
        {
            float face = SpawnStep(PassOverlap(Player.Stats.DashCornerCorrection));

            yield return DashRight();

            Assert.Greater(Player.PlayerRigidbody.position.x, face + HalfSize, "oyuncu basamagin ustune cikip gecmeli");
        }

        [UnityTest]
        public IEnumerator T2_DashHittingStepFurther_IsStopped()
        {
            float face = SpawnStep(BlockOverlap(Player.Stats.DashCornerCorrection));

            yield return DashRight();

            Assert.Less(Player.PlayerRigidbody.position.x, face, "oyuncu basamakta durmali");
        }
    }
}
