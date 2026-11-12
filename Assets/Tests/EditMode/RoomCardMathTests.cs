using New_Scripts.UI.Hud;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class RoomCardMathTests
    {
        [Test]
        public void FormatNumber_PadsToTwoDigits()
        {
            Assert.That(RoomCardMath.FormatNumber(7), Is.EqualTo("07"));
            Assert.That(RoomCardMath.FormatNumber(12), Is.EqualTo("12"));
            Assert.That(RoomCardMath.FormatNumber(104), Is.EqualTo("104"));
        }

        [Test]
        public void FormatNumber_NoNumber_IsEmpty()
        {
            Assert.That(RoomCardMath.FormatNumber(0), Is.Empty);
            Assert.That(RoomCardMath.FormatNumber(-3), Is.Empty);
        }

        [Test]
        public void BeamAngle_RisingToTheRight_IsPositive()
        {
            // Mockup kirişi: (0,206) -> (480,122), y aşağı. Sağa doğru yükselir.
            float angle = RoomCardMath.BeamAngleDegrees(new Vector2(0f, 206f), new Vector2(480f, 122f));
            Assert.That(angle, Is.EqualTo(9.93f).Within(0.05f));
            Assert.That(RoomCardMath.BeamAngleDegrees(new Vector2(0f, 0f), new Vector2(10f, 0f)), Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void LineY_InterpolatesAlongLine()
        {
            Assert.That(RoomCardMath.LineY(new Vector2(0f, 206f), new Vector2(480f, 122f), 240f), Is.EqualTo(164f).Within(1e-3f));
        }

        [Test]
        public void ClippedDisc_StaysInsideClipBoundsAndKeepsSomeArea()
        {
            var pts = RoomCardMath.ClippedDisc(new Vector2(26f, 208f), 92f, 0f, 270f, 48);
            Assert.That(pts.Length, Is.EqualTo(48));
            float maxX = float.MinValue;
            foreach (var p in pts)
            {
                Assert.That(p.x, Is.GreaterThanOrEqualTo(0f));
                Assert.That(p.y, Is.LessThanOrEqualTo(270f));
                maxX = Mathf.Max(maxX, p.x);
            }

            Assert.That(maxX, Is.EqualTo(118f).Within(0.01f)); // sağ uç dairenin kendisi (kırpılmaz)
        }
    }
}
