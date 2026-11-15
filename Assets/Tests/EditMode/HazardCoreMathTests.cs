using New_Scripts.Death;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class HazardCoreMathTests
    {
        [Test]
        public void UniformParent_CoreIsFractionOfSize()
        {
            Vector2 s = HazardCoreMath.CoreLocalScale(new Vector2(3f, 3f), 0.5f);
            Assert.That(s.x * 3f, Is.EqualTo(1.5f).Within(1e-5f));
            Assert.That(s.y * 3f, Is.EqualTo(1.5f).Within(1e-5f));
        }

        [Test]
        public void StretchedParent_CoreStaysSquareFromShortSide()
        {
            // 4x7 gerilmiş blok (oda 16): çekirdek dünyada 0.5 * 4 = 2 birim kare olmalı
            Vector2 s = HazardCoreMath.CoreLocalScale(new Vector2(4f, 7f), 0.5f);
            Assert.That(s.x * 4f, Is.EqualTo(2f).Within(1e-5f));
            Assert.That(s.y * 7f, Is.EqualTo(2f).Within(1e-5f));
        }

        [Test]
        public void NegativeOrZeroScale_DoesNotBreak()
        {
            Vector2 flipped = HazardCoreMath.CoreLocalScale(new Vector2(-2f, 2f), 0.5f);
            Assert.That(flipped.x * 2f, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(HazardCoreMath.CoreLocalScale(Vector2.zero, 0.5f), Is.EqualTo(Vector2.one));
        }
    }
}
