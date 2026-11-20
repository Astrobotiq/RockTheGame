using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class FlowerPetalMeshTests
    {
        [Test]
        public void Grid_HasExpectedCounts()
        {
            var mesh = FlowerPetalMesh.Build(10, 4);
            Assert.That(mesh.vertexCount, Is.EqualTo(11 * 5));
            Assert.That(mesh.triangles.Length, Is.EqualTo(10 * 4 * 6));
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Grid_UvCoversUnitSquare()
        {
            var mesh = FlowerPetalMesh.Build(6, 3);
            Vector2 min = Vector2.one, max = Vector2.zero;
            foreach (var uv in mesh.uv)
            {
                min = Vector2.Min(min, uv);
                max = Vector2.Max(max, uv);
            }
            Assert.That(min, Is.EqualTo(Vector2.zero));
            Assert.That(max, Is.EqualTo(Vector2.one));
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Grid_BoundsAreLargeEnoughForBentPetals()
        {
            var mesh = FlowerPetalMesh.Build();
            Assert.That(mesh.bounds.size.x, Is.GreaterThanOrEqualTo(4f));
            Object.DestroyImmediate(mesh);
        }
    }
}
