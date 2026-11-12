using New_Scripts.NPC;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class DialogueSlabMathTests
    {
        [Test]
        public void ResolveSide_PlayerFarFromSlab_KeepsSide()
        {
            Assert.That(DialogueSlabMath.ResolveSide(0.8f, SlabSide.Left, 0.1f), Is.EqualTo(SlabSide.Left));
            Assert.That(DialogueSlabMath.ResolveSide(0.2f, SlabSide.Right, 0.1f), Is.EqualTo(SlabSide.Right));
        }

        [Test]
        public void ResolveSide_PlayerEntersSlabHalfPastHysteresis_Flips()
        {
            Assert.That(DialogueSlabMath.ResolveSide(0.35f, SlabSide.Left, 0.1f), Is.EqualTo(SlabSide.Right));
            Assert.That(DialogueSlabMath.ResolveSide(0.65f, SlabSide.Right, 0.1f), Is.EqualTo(SlabSide.Left));
        }

        [Test]
        public void ResolveSide_InsideHysteresisBand_DoesNotFlip()
        {
            // Orta çizgi etrafında 0,4..0,6 arası taraf korunur (sınırın tam üstü float kayması yüzünden test edilmez).
            Assert.That(DialogueSlabMath.ResolveSide(0.41f, SlabSide.Left, 0.1f), Is.EqualTo(SlabSide.Left));
            Assert.That(DialogueSlabMath.ResolveSide(0.5f, SlabSide.Left, 0.1f), Is.EqualTo(SlabSide.Left));
            Assert.That(DialogueSlabMath.ResolveSide(0.5f, SlabSide.Right, 0.1f), Is.EqualTo(SlabSide.Right));
            Assert.That(DialogueSlabMath.ResolveSide(0.59f, SlabSide.Right, 0.1f), Is.EqualTo(SlabSide.Right));
        }

        [Test]
        public void ResolveSide_HoveringAroundCenter_NeverPingPongs()
        {
            // Oyuncu orta çizgide gidip gelirken (0,45 ↔ 0,55) taraf hiç değişmez.
            var side = SlabSide.Left;
            for (int i = 0; i < 20; i++)
            {
                float x = i % 2 == 0 ? 0.45f : 0.55f;
                side = DialogueSlabMath.ResolveSide(x, side, 0.1f);
                Assert.That(side, Is.EqualTo(SlabSide.Left));
            }
        }

        [Test]
        public void ResolveSide_NegativeHysteresis_TreatedAsZero()
        {
            Assert.That(DialogueSlabMath.ResolveSide(0.49f, SlabSide.Left, -1f), Is.EqualTo(SlabSide.Right));
        }

        [Test]
        public void Overlaps_PointInsideSlab_IsTrue()
        {
            var slab = new Rect(0.02f, 0.05f, 0.3f, 0.2f);
            Assert.That(DialogueSlabMath.Overlaps(slab, new Vector2(0.1f, 0.1f), 0f), Is.True);
        }

        [Test]
        public void Overlaps_PointOutside_IsFalse_UntilClearanceCoversIt()
        {
            var slab = new Rect(0.02f, 0.05f, 0.3f, 0.2f);
            var player = new Vector2(0.35f, 0.1f);
            Assert.That(DialogueSlabMath.Overlaps(slab, player, 0f), Is.False);
            Assert.That(DialogueSlabMath.Overlaps(slab, player, 0.05f), Is.True);
        }

        [Test]
        public void Overlaps_PlayerAboveSlab_IsFalse()
        {
            var slab = new Rect(0.02f, 0.05f, 0.3f, 0.2f);
            Assert.That(DialogueSlabMath.Overlaps(slab, new Vector2(0.1f, 0.6f), 0.05f), Is.False);
        }
    }
}
