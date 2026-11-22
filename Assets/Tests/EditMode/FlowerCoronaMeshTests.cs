using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class FlowerCoronaMeshTests
    {
        [Test]
        public void Counts_MatchStripsAndGrid()
        {
            var mesh = FlowerCoronaMesh.Build(10, 0f, 12, 4);
            Assert.That(mesh.vertexCount, Is.EqualTo(10 * 13 * 5));
            Assert.That(mesh.triangles.Length, Is.EqualTo(10 * 12 * 4 * 6));
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void TooFewStrips_AreRaisedToThree()
        {
            var mesh = FlowerCoronaMesh.Build(1, 0f, 4, 2);
            Assert.That(mesh.vertexCount, Is.EqualTo(3 * 5 * 3));
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void UvLayout_IsTheSameGridAsPetals_PerStrip()
        {
            var mesh = FlowerCoronaMesh.Build(6, 0f, 8, 2);
            var uv = mesh.uv;
            int perStrip = 9 * 3;
            for (int s = 0; s < 6; s++)
            {
                Assert.That(uv[s * perStrip], Is.EqualTo(new Vector2(0f, 0f)), "şeridin ilk köşesi (taban, bir kenar)");
                Assert.That(uv[s * perStrip + perStrip - 1], Is.EqualTo(new Vector2(1f, 1f)), "şeridin son köşesi (uç, öbür kenar)");
            }

            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Azimuths_AreEvenlySpaced()
        {
            const int strips = 10;
            var mesh = FlowerCoronaMesh.Build(strips, 0f, 4, 2);
            var uv2 = mesh.uv2;
            int perStrip = 5 * 3;
            float step = 2f * Mathf.PI / strips;
            for (int s = 0; s < strips; s++)
                Assert.That(uv2[s * perStrip].x, Is.EqualTo(s * step).Within(1e-5f));
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Phase_ShiftsTheRingByThatFractionOfAStep()
        {
            const int strips = 10;
            float step = 2f * Mathf.PI / strips;
            var plain = FlowerCoronaMesh.Build(strips, 0f, 4, 2);
            var shifted = FlowerCoronaMesh.Build(strips, 0.5f, 4, 2);
            for (int s = 0; s < strips; s++)
            {
                float a = plain.uv2[s * 15].x;
                float b = shifted.uv2[s * 15].x;
                Assert.That(b - a, Is.EqualTo(0.5f * step).Within(1e-5f));
            }

            Object.DestroyImmediate(plain);
            Object.DestroyImmediate(shifted);
        }

        [Test]
        public void EveryVertexOfAStrip_ShareTheSameAzimuthAndLength()
        {
            var mesh = FlowerCoronaMesh.Build(5, 0f, 6, 3);
            var uv2 = mesh.uv2;
            int perStrip = 7 * 4;
            for (int s = 0; s < 5; s++)
                for (int k = 1; k < perStrip; k++)
                    Assert.That(uv2[s * perStrip + k], Is.EqualTo(uv2[s * perStrip]));
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void LengthMultipliers_StayInCutLimits_AndNeighboursDiffer()
        {
            for (int i = 0; i < 40; i++)
            {
                float m = FlowerCoronaMesh.StripLengthMul(i, 0f);
                Assert.That(m, Is.InRange(1f - FlowerCutMath.LengthJitter, 1f + FlowerCutMath.LengthJitter));
                Assert.That(FlowerCoronaMesh.StripLengthMul(i + 1, 0f), Is.Not.EqualTo(m).Within(1e-5f));
            }
        }

        [Test]
        public void Bounds_AreLargeEnoughForTheBentStrips()
        {
            var mesh = FlowerCoronaMesh.Build(10);
            Assert.That(mesh.bounds.size.x, Is.GreaterThanOrEqualTo(4f));
            Assert.That(mesh.bounds.Contains(Vector3.zero));
            Object.DestroyImmediate(mesh);
        }
    }
}
