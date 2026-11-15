using New_Scripts.Death;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class IrisMathTests
    {
        private const float Aspect = 16f / 9f;

        [Test]
        public void CenteredIris_NeedsHalfDiagonal()
        {
            // Merkez ortada: köşe uzaklığı = sqrt((aspect/2)^2 + 0.5^2)
            float expected = Mathf.Sqrt((Aspect * 0.5f) * (Aspect * 0.5f) + 0.25f);
            Assert.That(IrisMath.RequiredRadius(new Vector2(0.5f, 0.5f), Aspect, 0f), Is.EqualTo(expected).Within(1e-4f));
        }

        [Test]
        public void CornerCenter_NeedsFullDiagonal()
        {
            float expected = Mathf.Sqrt(Aspect * Aspect + 1f);
            Assert.That(IrisMath.RequiredRadius(Vector2.zero, Aspect, 0f), Is.EqualTo(expected).Within(1e-4f));
        }

        [Test]
        public void OffscreenCenterBelowScreen_ExceedsDefaultMaxRadius()
        {
            // Odadaki hata: respawn noktası kamera blend'indeyken ekranın altında (viewport 0.24, -0.88).
            // Varsayılan _maxRadius 2 üst sağ köşeyi açamıyordu.
            float needed = IrisMath.RequiredRadius(new Vector2(0.24f, -0.88f), Aspect, 0.02f);
            Assert.That(needed, Is.GreaterThan(2.02f));
        }

        [Test]
        public void EdgeSmoothing_IsAdded()
        {
            float a = IrisMath.RequiredRadius(new Vector2(0.5f, 0.5f), Aspect, 0f);
            float b = IrisMath.RequiredRadius(new Vector2(0.5f, 0.5f), Aspect, 0.05f);
            Assert.That(b - a, Is.EqualTo(0.05f).Within(1e-5f));
        }

        [Test]
        public void InvalidAspect_FallsBackTo16x9()
        {
            Assert.That(IrisMath.RequiredRadius(new Vector2(0.5f, 0.5f), 0f, 0f),
                Is.EqualTo(IrisMath.RequiredRadius(new Vector2(0.5f, 0.5f), Aspect, 0f)).Within(1e-5f));
        }
    }
}
