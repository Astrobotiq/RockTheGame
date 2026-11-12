using New_Scripts.UI.Hud;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class StaminaRingMathTests
    {
        [Test]
        public void FullStamina_AllPiecesIntact() => Assert.That(StaminaRingMath.IntactPieces(1f, 6), Is.EqualTo(6));

        [Test]
        public void EmptyStamina_NoPiecesLeft() => Assert.That(StaminaRingMath.IntactPieces(0f, 6), Is.EqualTo(0));

        [Test]
        public void SmallestLoss_KeepsAllPiecesUntilFirstBoundary()
        {
            Assert.That(StaminaRingMath.IntactPieces(0.99f, 6), Is.EqualTo(6));
            Assert.That(StaminaRingMath.IntactPieces(5f / 6f + 0.001f, 6), Is.EqualTo(6));
        }

        [Test]
        public void CrossingBoundary_BreaksExactlyOnePiece()
        {
            Assert.That(StaminaRingMath.IntactPieces(5f / 6f - 0.001f, 6), Is.EqualTo(5));
            Assert.That(StaminaRingMath.IntactPieces(0.5f - 0.001f, 6), Is.EqualTo(3));
            Assert.That(StaminaRingMath.IntactPieces(1f / 6f - 0.001f, 6), Is.EqualTo(1));
        }

        [Test]
        public void ExactBoundary_DoesNotBreakEarlyFromFloatError()
        {
            // 5/6 ve 3/6 tam sınır: kayan nokta hatası bir parçayı erken kırmamalı / geç bırakmamalı.
            Assert.That(StaminaRingMath.IntactPieces(5f / 6f, 6), Is.EqualTo(5));
            Assert.That(StaminaRingMath.IntactPieces(0.5f, 6), Is.EqualTo(3));
        }

        [Test]
        public void OutOfRangeFraction_IsClamped()
        {
            Assert.That(StaminaRingMath.IntactPieces(2f, 6), Is.EqualTo(6));
            Assert.That(StaminaRingMath.IntactPieces(-1f, 6), Is.EqualTo(0));
        }

        [Test]
        public void IntactPieces_NeverIncreasesAsStaminaDrops()
        {
            int previous = 6;
            for (float f = 1f; f >= 0f; f -= 0.01f)
            {
                int now = StaminaRingMath.IntactPieces(f, 6);
                Assert.That(now, Is.LessThanOrEqualTo(previous), $"fraction {f}");
                previous = now;
            }
        }

        [Test]
        public void SixSecondsAtOneUnitPerSecond_BreaksOnePiecePerSecond()
        {
            const float max = 6f;
            for (int second = 0; second <= 6; second++)
            {
                float current = max - second - 0.01f;
                Assert.That(StaminaRingMath.IntactPieces(current / max, 6), Is.EqualTo(6 - second), $"{second}. saniye");
            }
        }

        [Test]
        public void ZeroCount_ReturnsZero() => Assert.That(StaminaRingMath.IntactPieces(1f, 0), Is.EqualTo(0));

        [Test]
        public void PieceAngles_StartAtTwelveClockwise_WithHalfGapMargin()
        {
            Assert.That(StaminaRingMath.PieceStartDegrees(0, 6, 14f), Is.EqualTo(7f).Within(1e-4f));
            Assert.That(StaminaRingMath.PieceStartDegrees(1, 6, 14f), Is.EqualTo(67f).Within(1e-4f));
            Assert.That(StaminaRingMath.PieceSweepDegrees(6, 14f), Is.EqualTo(46f).Within(1e-4f));
        }

        [Test]
        public void PieceSweep_NeverNegative() => Assert.That(StaminaRingMath.PieceSweepDegrees(6, 90f), Is.EqualTo(0f));

        [Test]
        public void PieceDirections_FirstPieceIsUpperRight_ThirdIsLowerRight()
        {
            Vector2 first = StaminaRingMath.DirectionOf(StaminaRingMath.PieceMidDegrees(0, 6));
            Assert.That(first.x, Is.GreaterThan(0f));
            Assert.That(first.y, Is.GreaterThan(0f));
            Vector2 third = StaminaRingMath.DirectionOf(StaminaRingMath.PieceMidDegrees(2, 6));
            Assert.That(third.x, Is.GreaterThan(0f));
            Assert.That(third.y, Is.LessThan(0f));
        }
    }
}
