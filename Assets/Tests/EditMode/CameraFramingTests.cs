using New_Scripts.Player;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class CameraFramingTests
    {
        private const float Aspect = 16f / 9f;

        [Test]
        public void WideFrame_SizedByWidth()
        {
            // 64x10: genişlik baskın (64/2/1.777 = 18), yükseklik yarısı 5
            var (center, size) = CameraFraming.Fit(new Rect(10f, 20f, 64f, 10f), Aspect, 0f);
            Assert.That(size, Is.EqualTo(18f).Within(1e-4f));
            Assert.That(center.x, Is.EqualTo(42f).Within(1e-5f));
            Assert.That(center.y, Is.EqualTo(25f).Within(1e-5f));
        }

        [Test]
        public void TallFrame_SizedByHeight()
        {
            // 10x40: yükseklik baskın (20), genişlik 10/2/1.777 = 2.8
            var (_, size) = CameraFraming.Fit(new Rect(0f, 0f, 10f, 40f), Aspect, 0f);
            Assert.That(size, Is.EqualTo(20f).Within(1e-4f));
        }

        [Test]
        public void Padding_IsAddedOnTop()
        {
            var (_, size) = CameraFraming.Fit(new Rect(0f, 0f, 10f, 40f), Aspect, 1.5f);
            Assert.That(size, Is.EqualTo(21.5f).Within(1e-4f));
        }

        [Test]
        public void ZeroSizeFrame_ReturnsOnlyPadding()
        {
            var (center, size) = CameraFraming.Fit(new Rect(3f, 4f, 0f, 0f), Aspect, 1f);
            Assert.That(size, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(center, Is.EqualTo(new Vector2(3f, 4f)));
        }

        [Test]
        public void NegativeSizeFrame_TreatedAsPositive()
        {
            var (center, size) = CameraFraming.Fit(new Rect(10f, 10f, -10f, -40f), Aspect, 0f);
            Assert.That(size, Is.EqualTo(20f).Within(1e-4f));
            Assert.That(center.x, Is.EqualTo(5f).Within(1e-5f));
            Assert.That(center.y, Is.EqualTo(-10f).Within(1e-5f));
        }

        [Test]
        public void InvalidAspectAndPadding_FallBackSafely()
        {
            var (_, size) = CameraFraming.Fit(new Rect(0f, 0f, 64f, 10f), 0f, -3f);
            Assert.That(size, Is.EqualTo(18f).Within(1e-4f));
            Assert.That(float.IsNaN(size), Is.False);
        }

        [Test]
        public void ToWorldRect_NullSpace_ReturnsLocalFrame()
        {
            var r = new Rect(1f, 2f, 3f, 4f);
            Assert.That(CameraFraming.ToWorldRect(null, r), Is.EqualTo(r));
        }

        [Test]
        public void ToWorldRect_FollowsTranslationAndScale()
        {
            var go = new GameObject("space");
            try
            {
                go.transform.position = new Vector3(100f, 50f, 0f);
                go.transform.localScale = new Vector3(2f, 2f, 1f);

                Rect world = CameraFraming.ToWorldRect(go.transform, new Rect(-5f, -3f, 10f, 6f));

                Assert.That(world.xMin, Is.EqualTo(90f).Within(1e-4f));
                Assert.That(world.xMax, Is.EqualTo(110f).Within(1e-4f));
                Assert.That(world.yMin, Is.EqualTo(44f).Within(1e-4f));
                Assert.That(world.yMax, Is.EqualTo(56f).Within(1e-4f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
