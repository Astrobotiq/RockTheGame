using NUnit.Framework;
using New_Scripts.Multiplayer.Ghost;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Hayalet görseli oyuncunun sprite/animator/ip kaynaklarından kurulur; collider ve oyuncu script'i taşımaz,
    /// sahne kökünde durur. docs/superpowers/plans/2026-11-06-mp4-ghost.md → Task 2.
    /// </summary>
    public class GhostViewTests
    {
        private GameObject _source;
        private SpriteRenderer _body;
        private Animator _animator;
        private LineRenderer _rope;
        private GhostView _ghost;

        [SetUp]
        public void SetUp()
        {
            _source = new GameObject("FakePlayer");
            var bodyGo = new GameObject("Animation");
            bodyGo.transform.SetParent(_source.transform, false);
            bodyGo.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            _body = bodyGo.AddComponent<SpriteRenderer>();
            _body.sortingLayerName = "Player";
            _body.sortingOrder = 5;
            _animator = bodyGo.AddComponent<Animator>();
            var ropeGo = new GameObject("LeftRope");
            ropeGo.transform.SetParent(_source.transform, false);
            _rope = ropeGo.AddComponent<LineRenderer>();
            _rope.startWidth = 0.1f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_ghost != null) Object.DestroyImmediate(_ghost.gameObject);
            Object.DestroyImmediate(_source);
        }

        [Test]
        public void CreateFrom_CopiesSpriteAnimatorAndSorting_NoColliderNoPlayerScripts()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope);

            var sprite = _ghost.GetComponentInChildren<SpriteRenderer>();
            Assert.IsNotNull(sprite);
            Assert.AreEqual(_body.sortingLayerID, sprite.sortingLayerID);
            Assert.AreEqual(4, sprite.sortingOrder);
            Assert.AreEqual(0.55f, sprite.color.a, 1e-3f);
            Assert.IsNotNull(_ghost.GetComponentInChildren<Animator>());
            Assert.AreEqual(2, _ghost.GetComponentsInChildren<LineRenderer>(true).Length);
            Assert.IsEmpty(_ghost.GetComponentsInChildren<Collider2D>(true));
            Assert.IsNull(_ghost.GetComponentInChildren<New_Scripts.Player.PlayerController>(true));
            Assert.AreEqual(GhostView.ObjectName, _ghost.gameObject.name);
        }

        [Test]
        public void CreateFrom_IsRootObject()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope);
            Assert.IsNull(_ghost.transform.parent);
        }

        [Test]
        public void Apply_SetsPositionFlipAndRopes()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope);
            _ghost.SetVisible(true);

            _ghost.Apply(new GhostPose { Position = new Vector2(3f, 4f), FacingRight = false, LeftAnchor = new Vector2(5f, 9f) });

            Assert.AreEqual(new Vector3(3f, 4f, 0f), _ghost.transform.position);
            var sprite = _ghost.GetComponentInChildren<SpriteRenderer>();
            Assert.IsTrue(sprite.flipX);
            var ropes = _ghost.GetComponentsInChildren<LineRenderer>(true);
            Assert.IsTrue(ropes[0].enabled, "sol ip açık olmalı");
            Assert.AreEqual(new Vector3(5f, 9f, 0f), ropes[0].GetPosition(1));
            Assert.IsFalse(ropes[1].enabled, "sağ kanca yok, ip kapalı olmalı");
        }

        [Test]
        public void SetVisible_False_HidesRenderers()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope);
            _ghost.SetVisible(true);
            _ghost.Apply(new GhostPose { Position = Vector2.zero, LeftAnchor = Vector2.one });

            _ghost.SetVisible(false);

            Assert.IsFalse(_ghost.GetComponentInChildren<SpriteRenderer>().enabled);
            foreach (var rope in _ghost.GetComponentsInChildren<LineRenderer>(true)) Assert.IsFalse(rope.enabled);
        }
    }
}
