using System.Collections.Generic;
using System.Text.RegularExpressions;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Hayalet nesne kimlikleri: kardeş sıralı yol → FNV-1a 32 bit, ve hareketli sürücülerin kimlik tablosu.
    /// docs/multiplayer-ceza-kapi-ve-platform-plani.md → MPG1.
    /// </summary>
    public class MovingSurfaceRegistryTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created) if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        private GameObject Make(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            _created.Add(go);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        // ---- SceneObjectId ----

        [Test]
        public void FromPath_MatchesPublishedFnv1aVectors()
        {
            Assert.AreEqual(0x811C9DC5u, SceneObjectId.FromPath(""));
            Assert.AreEqual(0xE40C292Cu, SceneObjectId.FromPath("a"));
            Assert.AreEqual(0xBF9CF968u, SceneObjectId.FromPath("foobar"));
        }

        [Test]
        public void BuildPath_ListsNameAndSiblingIndexFromRootDown()
        {
            var root = Make("Room");
            Make("Other", root.transform);
            var content = Make("Content", root.transform);
            var platform = Make("Platform", content.transform);

            Assert.AreEqual($"Room:{root.transform.GetSiblingIndex()}/Content:1/Platform:0", SceneObjectId.BuildPath(platform.transform));
        }

        [Test]
        public void SamePath_SameId_AcrossIndependentHierarchies()
        {
            var a = Make("Room");
            var aPlat = Make("Platform", a.transform);
            var idA = SceneObjectId.For(aPlat.transform);
            Object.DestroyImmediate(a);

            var b = Make("Room");
            var bPlat = Make("Platform", b.transform);
            // Kök kardeş sırası da yolda: ikisi de sahnenin son kökü olduğu için aynı.
            Assert.AreEqual(idA, SceneObjectId.For(bPlat.transform));
        }

        [Test]
        public void SameNamedSiblings_GetDifferentIds()
        {
            var root = Make("Room");
            var first = Make("Platform", root.transform);
            var second = Make("Platform", root.transform);

            Assert.AreNotEqual(SceneObjectId.For(first.transform), SceneObjectId.For(second.transform));
        }

        // ---- MovingSurfaceRegistry ----

        [Test]
        public void Register_LookupWorksBothWays()
        {
            var registry = new MovingSurfaceRegistry();
            var t = Make("Platform").transform;

            Assert.IsTrue(registry.Register(t));

            Assert.AreEqual(1, registry.Count);
            Assert.IsTrue(registry.TryGetId(t, out uint id));
            Assert.AreEqual(SceneObjectId.For(t), id);
            Assert.IsTrue(registry.TryGet(id, out var found));
            Assert.AreSame(t, found);
        }

        [Test]
        public void Register_SameTransformTwice_IsIgnored()
        {
            var registry = new MovingSurfaceRegistry();
            var t = Make("Platform").transform;

            Assert.IsTrue(registry.Register(t));
            Assert.IsTrue(registry.Register(t));

            Assert.AreEqual(1, registry.Count);
            Assert.AreEqual(0, registry.AmbiguousCount);
        }

        [Test]
        public void Collision_WarnsAndDropsBothIds()
        {
            var registry = new MovingSurfaceRegistry();
            var first = Make("A").transform;
            var second = Make("B").transform;
            Assert.IsTrue(registry.Register(first, 7u));

            LogAssert.Expect(LogType.Warning, new Regex("çakışıyor"));
            Assert.IsFalse(registry.Register(second, 7u));

            Assert.AreEqual(0, registry.Count);
            Assert.AreEqual(1, registry.AmbiguousCount);
            Assert.IsFalse(registry.TryGet(7u, out _));
            Assert.IsFalse(registry.TryGetId(first, out _));
            Assert.IsFalse(registry.TryGetId(second, out _));
        }

        [Test]
        public void Collision_ThirdObjectWithSameId_IsAlsoRejected()
        {
            var registry = new MovingSurfaceRegistry();
            registry.Register(Make("A").transform, 7u);
            LogAssert.Expect(LogType.Warning, new Regex("çakışıyor"));
            registry.Register(Make("B").transform, 7u);

            LogAssert.Expect(LogType.Warning, new Regex("çakışıyor"));
            Assert.IsFalse(registry.Register(Make("C").transform, 7u));

            Assert.AreEqual(0, registry.Count);
            Assert.AreEqual(1, registry.AmbiguousCount);
        }

        [Test]
        public void TryGet_DestroyedTransform_ReturnsFalse()
        {
            var registry = new MovingSurfaceRegistry();
            var go = Make("Platform");
            registry.Register(go.transform);
            registry.TryGetId(go.transform, out uint id);

            Object.DestroyImmediate(go);

            Assert.IsFalse(registry.TryGet(id, out var t));
            Assert.IsNull(t);
        }

        [Test]
        public void TryGet_UnknownId_ReturnsFalse()
        {
            Assert.IsFalse(new MovingSurfaceRegistry().TryGet(12345u, out _));
        }

        [Test]
        public void BuildFromScene_RegistersBothControllerTypes_IncludingInactive()
        {
            var plain = Make("PlainPlatform");
            plain.AddComponent<PlatformController>();
            var triggered = Make("TriggeredPlatform");
            triggered.AddComponent<BoxCollider2D>();
            triggered.AddComponent<TriggeredPlatformController>();
            triggered.SetActive(false);

            var registry = MovingSurfaceRegistry.BuildFromScene();

            Assert.IsTrue(registry.TryGetId(plain.transform, out _), "PlatformController kayıtlı");
            Assert.IsTrue(registry.TryGetId(triggered.transform, out _), "pasif TriggeredPlatformController da kayıtlı");
        }
    }
}
