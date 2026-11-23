using System.Collections.Generic;
using New_Scripts.Death;
using New_Scripts.Multiplayer.Ghost;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// MPG2 gönderen tarafı: kayıttan poz okuma (ata araması), SnapshotBuilder'ın yüzeyleri taşıması, PlayerHealth.LastKiller ömrü.
    /// </summary>
    public class SurfacePoseTests
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

        [Test]
        public void TryGetPose_RegisteredTransform_ReturnsItsIdAndWorldPose()
        {
            var registry = new MovingSurfaceRegistry();
            var platform = Make("Platform");
            platform.transform.position = new Vector3(4f, -2f, 0f);
            platform.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
            registry.Register(platform.transform);

            Assert.IsTrue(registry.TryGetPose(platform.transform, out var pose));

            Assert.AreEqual(SceneObjectId.For(platform.transform), pose.Id);
            Assert.AreEqual(new Vector2(4f, -2f), pose.Position);
            Assert.AreEqual(30f, pose.Rotation, 1e-3f);
        }

        [Test]
        public void TryGetPose_ChildOfRegistered_UsesTheRegisteredAncestor()
        {
            var registry = new MovingSurfaceRegistry();
            var platform = Make("Platform");
            platform.transform.position = new Vector3(10f, 0f, 0f);
            var node = Make("Node", platform.transform);
            node.transform.localPosition = new Vector3(0f, 5f, 0f);
            registry.Register(platform.transform);

            Assert.IsTrue(registry.TryGetPose(node.transform, out var pose));

            Assert.AreEqual(SceneObjectId.For(platform.transform), pose.Id, "kimlik sürücüyü taşıyan atanın");
            Assert.AreEqual(new Vector2(10f, 0f), pose.Position, "poz atanın, node'un değil");
        }

        [Test]
        public void TryGetPose_StaticObject_ReturnsFalse()
        {
            var registry = new MovingSurfaceRegistry();
            registry.Register(Make("Moving").transform);

            Assert.IsFalse(registry.TryGetPose(Make("Static").transform, out _));
            Assert.IsFalse(registry.TryGetPose(null, out _));
        }

        [Test]
        public void SnapshotBuilder_PassesSurfacesThrough()
        {
            var carrier = new New_Scripts.Multiplayer.Messages.SurfacePose(1u, new Vector2(1f, 2f), 3f);
            var left = new New_Scripts.Multiplayer.Messages.SurfacePose(2u, new Vector2(4f, 5f), 6f);
            var input = new PlayerPoseInput { RoomId = 1, Position = Vector2.zero, Carrier = carrier, LeftNode = left };

            var snapshot = new SnapshotBuilder().Build(input, 1.0);

            Assert.AreEqual(1u, snapshot.Carrier.Value.Id);
            Assert.AreEqual(2u, snapshot.LeftNode.Value.Id);
            Assert.IsNull(snapshot.RightNode);
        }

        [Test]
        public void PlayerHealth_LastKiller_VisibleDuringOnDeath_ThenCleared()
        {
            var player = Make("Player");
            var health = player.AddComponent<PlayerHealth>();
            var hazard = Make("Hazard").transform;
            Transform seen = null;
            health.OnDeath += () => seen = health.LastKiller;

            health.NoteKiller(hazard);
            health.Kill();

            Assert.AreSame(hazard, seen, "OnDeath dinleyicisi öldüreni görür");
            Assert.IsNull(health.LastKiller, "ölümden sonra temizlenir; sonraki menü ölümü eski tehlikeyi taşımasın");
        }

        [Test]
        public void PlayerHealth_KillWithoutNote_HasNoKiller()
        {
            var player = Make("Player");
            var health = player.AddComponent<PlayerHealth>();
            Transform seen = new GameObject("sentinel").transform;
            _created.Add(seen.gameObject);
            health.OnDeath += () => seen = health.LastKiller;

            health.Kill();

            Assert.IsNull(seen);
        }
    }
}
