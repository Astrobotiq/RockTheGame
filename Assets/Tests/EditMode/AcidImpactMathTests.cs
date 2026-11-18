using New_Scripts.Death;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class AcidImpactMathTests
    {
        // Acid.prefab: varsayılan Square sprite, yerel rect -0.5..0.5
        private static readonly Rect UnitRect = new Rect(-0.5f, -0.5f, 1f, 1f);

        [Test]
        public void Backtrack_StepsAgainstVelocity()
        {
            Vector2 p = AcidImpactMath.BacktrackPoint(new Vector2(1f, -1f), new Vector2(0f, -32f), 0.02f);
            Assert.That(p.x, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(p.y, Is.EqualTo(-0.36f).Within(1e-5f));
        }

        [Test]
        public void LocalToPixel_CornersMapToPixelCentres()
        {
            var scale = new Vector2(48f, 64f);
            Vector2 min = AcidImpactMath.LocalToPixel(new Vector2(-0.5f, -0.5f), UnitRect, scale);
            Vector2 max = AcidImpactMath.LocalToPixel(new Vector2(0.5f, 0.5f), UnitRect, scale);
            Vector2 mid = AcidImpactMath.LocalToPixel(Vector2.zero, UnitRect, scale);
            Assert.That(min, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(max, Is.EqualTo(new Vector2(47.5f, 63.5f)));
            Assert.That(mid, Is.EqualTo(new Vector2(24f, 32f)));
        }

        [Test]
        public void LocalToPixel_OutsidePointIsClamped()
        {
            Vector2 p = AcidImpactMath.LocalToPixel(new Vector2(3f, 0.9f), UnitRect, new Vector2(48f, 64f));
            Assert.That(p.x, Is.EqualTo(47.5f).Within(1e-4f));
            Assert.That(p.y, Is.EqualTo(63.5f).Within(1e-4f));
        }

        [Test]
        public void LocalToPixel_FractionalScale()
        {
            // Oda 18: 41.1 × 16 = 657.6 piksel genişlik
            Vector2 p = AcidImpactMath.LocalToPixel(new Vector2(0.25f, 0f), UnitRect, new Vector2(657.6f, 64f));
            Assert.That(p.x, Is.EqualTo(493.2f).Within(1e-3f));
            Vector2 edge = AcidImpactMath.LocalToPixel(new Vector2(0.5f, 0f), UnitRect, new Vector2(657.6f, 64f));
            Assert.That(edge.x, Is.EqualTo(657.1f).Within(1e-3f));
        }

        [Test]
        public void PixelToLocal_InvertsLocalToPixel()
        {
            var scale = new Vector2(1200f, 160f);
            Vector2 local = new Vector2(0.123f, 0.4f);
            Vector2 back = AcidImpactMath.PixelToLocal(AcidImpactMath.LocalToPixel(local, UnitRect, scale), UnitRect, scale);
            Assert.That(back.x, Is.EqualTo(local.x).Within(1e-4f));
            Assert.That(back.y, Is.EqualTo(local.y).Within(1e-4f));
            // Yüzey satırı (144 px) yerel 0.4'e döner
            Assert.That(AcidImpactMath.PixelToLocal(new Vector2(600f, 144f), UnitRect, scale).y, Is.EqualTo(0.4f).Within(1e-5f));
        }

        [Test]
        public void SurfaceContact_LocksToSurfaceRow()
        {
            Vector2 p = AcidImpactMath.SurfaceContact(new Vector2(10f, 40f), 0.9f, 64f, true);
            Assert.That(p, Is.EqualTo(new Vector2(10f, 57f)));
        }

        [Test]
        public void SurfaceContact_NoSurfaceKeepsPoint()
        {
            Vector2 p = AcidImpactMath.SurfaceContact(new Vector2(3f, 20f), 0.9f, 56f, false);
            Assert.That(p, Is.EqualTo(new Vector2(3f, 20f)));
        }

        [Test]
        public void Strength_FloorAndCeiling()
        {
            Assert.That(AcidImpactMath.Strength(0f, 4f, 40f, 0.35f), Is.EqualTo(0.35f).Within(1e-5f));
            Assert.That(AcidImpactMath.Strength(4f, 4f, 40f, 0.35f), Is.EqualTo(0.35f).Within(1e-5f));
            Assert.That(AcidImpactMath.Strength(40f, 4f, 40f, 0.35f), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(AcidImpactMath.Strength(65f, 4f, 40f, 0.35f), Is.EqualTo(1f).Within(1e-5f));
            Assert.That(AcidImpactMath.Strength(22f, 4f, 40f, 0.35f), Is.EqualTo(0.675f).Within(1e-4f));
        }

        [Test]
        public void CrestHeadroom_LowAndTallAcid()
        {
            // Oda 22: yükseklik 2 birim → 32 px; 32·0.1 − 2 − 1 = 0.2
            Assert.That(AcidImpactMath.CrestHeadroom(32f, 0.9f, 2f), Is.EqualTo(0.2f).Within(1e-4f));
            // Oda 5: yükseklik 10 birim → 160 px; 16 − 2 − 1 = 13
            Assert.That(AcidImpactMath.CrestHeadroom(160f, 0.9f, 2f), Is.EqualTo(13f).Within(1e-4f));
            // Pay negatife düşmez
            Assert.That(AcidImpactMath.CrestHeadroom(16f, 0.95f, 2f), Is.EqualTo(0f));
        }

        [Test]
        public void SplashVelocity_ConeCentreIsNormal()
        {
            Vector2 v = AcidImpactMath.SplashVelocity(Vector2.up, 35f, 0.5f, 5f, 1f, Vector2.zero, 0.3f, 0.25f, 9f);
            Assert.That(v.x, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(v.y, Is.EqualTo(5f).Within(1e-4f));
        }

        [Test]
        public void SplashVelocity_ConeEdgesAreSymmetric()
        {
            Vector2 left = AcidImpactMath.SplashVelocity(Vector2.up, 35f, 0f, 5f, 1f, Vector2.zero, 0f, 0f, 9f);
            Vector2 right = AcidImpactMath.SplashVelocity(Vector2.up, 35f, 1f, 5f, 1f, Vector2.zero, 0f, 0f, 9f);
            Assert.That(left.x, Is.EqualTo(-right.x).Within(1e-4f));
            Assert.That(left.y, Is.EqualTo(right.y).Within(1e-4f));
            Assert.That(Vector2.Angle(Vector2.up, right), Is.EqualTo(35f).Within(1e-3f));
        }

        [Test]
        public void SplashVelocity_InheritsPlayerMotion()
        {
            // Sağa koşarak düşen oyuncu: damla sağa eğilir, düşüş hızı yukarı fırlatmayı büyütür
            Vector2 v = AcidImpactMath.SplashVelocity(Vector2.up, 35f, 0.5f, 4f, 1f, new Vector2(10f, -8f), 0.3f, 0.25f, 20f);
            Assert.That(v.x, Is.EqualTo(3f).Within(1e-4f));
            Assert.That(v.y, Is.EqualTo(6f).Within(1e-4f));
        }

        [Test]
        public void SplashVelocity_ClampedToMax()
        {
            Vector2 v = AcidImpactMath.SplashVelocity(Vector2.up, 0f, 0.5f, 7f, 1f, new Vector2(65f, -32f), 0.3f, 0.25f, 9f);
            Assert.That(v.magnitude, Is.EqualTo(9f).Within(1e-4f));
        }

        [Test]
        public void SplashVelocity_ZeroNormalFallsBackToUp()
        {
            Vector2 v = AcidImpactMath.SplashVelocity(Vector2.zero, 0f, 0.5f, 5f, 1f, Vector2.zero, 0f, 0f, 9f);
            Assert.That(v, Is.EqualTo(new Vector2(0f, 5f)));
        }

        [Test]
        public void ImpactLive_Bounds()
        {
            Assert.That(AcidImpactMath.ImpactLive(0f, 0.6f, 1f), Is.True);
            Assert.That(AcidImpactMath.ImpactLive(0.59f, 0.6f, 1f), Is.True);
            Assert.That(AcidImpactMath.ImpactLive(0.6f, 0.6f, 1f), Is.False);
            Assert.That(AcidImpactMath.ImpactLive(-0.01f, 0.6f, 1f), Is.False);
            Assert.That(AcidImpactMath.ImpactLive(0.1f, 0.6f, 0f), Is.False);
        }
    }
}
