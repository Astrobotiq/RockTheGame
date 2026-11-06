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
            Assert.AreEqual(5 - GhostView.SortingOrderOffset, sprite.sortingOrder);
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
        public void Apply_HiddenPose_HidesBodyAndRopesEvenWhenVisible()
        {
            // Rakip öldü: hayalet aynı odada olsa da görünmez; respawn'da yeniden belirir.
            _ghost = GhostView.CreateFrom(_body, _animator, _rope);
            _ghost.SetVisible(true);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, LeftAnchor = Vector2.one, IsHidden = true });
            Assert.IsFalse(_ghost.GetComponentInChildren<SpriteRenderer>().enabled);
            foreach (var rope in _ghost.GetComponentsInChildren<LineRenderer>(true)) Assert.IsFalse(rope.enabled);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, IsHidden = false });
            Assert.IsTrue(_ghost.GetComponentInChildren<SpriteRenderer>().enabled);
        }

        private GhostEyeSource FakeEyes(out Transform eyesRoot)
        {
            eyesRoot = new GameObject("Eyes").transform;
            eyesRoot.SetParent(_source.transform, false);
            var source = new GhostEyeSource
            {
                Root = eyesRoot,
                VelocitySensitivity = 0.01f,
                MaxPupilOffset = 0.1f,
                PupilSmoothSpeed = 10f,
                FlipParentTransform = true
            };
            foreach (var (name, x) in new[] { ("LeftEyesContainer", 0f), ("RightContainer", 0.39f) })
            {
                var container = new GameObject(name).transform;
                container.SetParent(eyesRoot, false);
                container.localPosition = new Vector3(x, 0.52f, 0f);
                SpriteRenderer Make(string n, int order)
                {
                    var r = new GameObject(n).AddComponent<SpriteRenderer>();
                    r.transform.SetParent(container, false);
                    r.sortingLayerName = "Player";
                    r.sortingOrder = order;
                    return r;
                }
                source.Eyes.Add(new GhostEyeSource.Eye { Container = container, Background = Make("EyeBg", 4), Pupil = Make("Eye", 5), Blink = Make("EyeFg", 6) });
            }
            return source;
        }

        [Test]
        public void CreateFrom_WithEyes_CopiesEyesAboveGhostBodyBelowRealPlayer()
        {
            _body.sortingOrder = 0;
            _ghost = GhostView.CreateFrom(_body, _animator, _rope, FakeEyes(out _));

            var renderers = _ghost.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.AreEqual(7, renderers.Length, "gövde + 2 göz x 3 katman");
            int bodyOrder = -GhostView.SortingOrderOffset;
            foreach (var r in renderers)
            {
                if (r.name == "Body") continue;
                Assert.Greater(r.sortingOrder, bodyOrder, $"{r.name} hayalet gövdesinin önünde olmalı");
                Assert.Less(r.sortingOrder, 0, $"{r.name} gerçek oyuncunun arkasında olmalı");
                Assert.AreEqual(0.55f, r.color.a, 1e-3f);
            }
        }

        [Test]
        public void Apply_FacingLeft_MirrorsEyes()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope, FakeEyes(out _));
            _ghost.SetVisible(true);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, FacingRight = false });
            _ghost.TickEyes(0.016f);

            var eyesRoot = _ghost.transform.Find("Eyes");
            Assert.IsNotNull(eyesRoot);
            Assert.AreEqual(-1f, eyesRoot.localScale.x);
        }

        [Test]
        public void Pupils_LookAlongMovement()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope, FakeEyes(out _));
            _ghost.SetVisible(true);
            var pupil = _ghost.transform.Find("Eyes/RightContainer/Eye");
            float rest = pupil.localPosition.x;

            _ghost.Apply(new GhostPose { Position = Vector2.zero, FacingRight = true, Speed = 20f });
            for (int i = 0; i < 30; i++) _ghost.TickEyes(0.05f);

            Assert.Greater(pupil.localPosition.x, rest + 0.05f, "koşarken göz bebeği ileri bakmalı");
        }

        [Test]
        public void HiddenPose_HidesEyesToo()
        {
            _ghost = GhostView.CreateFrom(_body, _animator, _rope, FakeEyes(out _));
            _ghost.SetVisible(true);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, IsHidden = true });

            foreach (var r in _ghost.GetComponentsInChildren<SpriteRenderer>(true)) Assert.IsFalse(r.enabled, r.name);
        }

        // ---------------- Kollar ve dash efekti (6 Kasım, kullanıcı isteği) ----------------

        private GhostArmSource FakeArms()
        {
            var source = new GhostArmSource();
            foreach (var (name, x) in new[] { ("LeftArmPivot", -0.54f), ("RightArmPivot", 0.22f) })
            {
                var pivot = new GameObject(name).transform;
                pivot.SetParent(_source.transform, false);
                pivot.localPosition = new Vector3(x, 0f, 0f);
                pivot.localScale = Vector3.one * 0.6f;
                var square = new GameObject("Square").AddComponent<SpriteRenderer>();
                square.transform.SetParent(pivot, false);
                square.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                square.transform.localScale = new Vector3(1f, 0.3f, 1f);
                square.sortingLayerName = "Player";
                square.sortingOrder = 1;
                if (x < 0f) { source.LeftPivot = pivot; source.LeftSprite = square; }
                else { source.RightPivot = pivot; source.RightSprite = square; }
            }
            return source;
        }

        private GhostView GhostWithFx(out GhostDashSource dash)
        {
            dash = new GhostDashSource { SpawnInterval = 0.05f, Duration = 0.5f, Color = new Color(0.5f, 0.5f, 1f, 0.5f), Trail = null };
            var ghost = GhostView.CreateFrom(_body, _animator, _rope, null, FakeArms(), dash);
            ghost.SetVisible(true);
            return ghost;
        }

        [Test]
        public void Arm_ShownOnlyWhileHooked_PointsAtAnchor_RopeStartsAtHand()
        {
            _ghost = GhostWithFx(out _);
            var leftArm = _ghost.transform.Find("LeftArmPivot");
            Assert.IsNotNull(leftArm);
            var armSprite = leftArm.GetComponentInChildren<SpriteRenderer>(true);

            _ghost.Apply(new GhostPose { Position = Vector2.zero });
            Assert.IsFalse(armSprite.enabled, "kanca yokken kol gizli");

            _ghost.Apply(new GhostPose { Position = Vector2.zero, LeftAnchor = new Vector2(-0.54f, 5f) });
            Assert.IsTrue(armSprite.enabled, "kanca atılınca kol görünür");
            Assert.AreEqual(90f, leftArm.eulerAngles.z, 0.5f, "kol tutunduğu yere bakar");

            var rope = _ghost.transform.Find("LeftRope").GetComponent<LineRenderer>();
            Vector3 start = rope.GetPosition(0);
            Assert.AreEqual(-0.54f, start.x, 1e-3f);
            Assert.AreEqual(0.6f, start.y, 1e-3f, "ip kolun ucundan (0.6 birim) çıkar");
        }

        [Test]
        public void Arm_HiddenWhenGhostHidden()
        {
            _ghost = GhostWithFx(out _);
            var armSprite = _ghost.transform.Find("RightArmPivot").GetComponentInChildren<SpriteRenderer>(true);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, RightAnchor = new Vector2(3f, 3f), IsHidden = true });

            Assert.IsFalse(armSprite.enabled);
        }

        [Test]
        public void Dash_LeavesFadingAfterimages_OnlyWhileVisible()
        {
            _ghost = GhostWithFx(out _);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, IsDashing = true });
            for (int i = 0; i < 5; i++) _ghost.TickEffects(0.05f);
            int spawned = _ghost.ActiveAfterimageCount;
            Assert.GreaterOrEqual(spawned, 3, "dash sırasında 0.05 sn'de bir kopya");

            _ghost.Apply(new GhostPose { Position = Vector2.zero, IsDashing = false });
            for (int i = 0; i < 12; i++) _ghost.TickEffects(0.05f);
            Assert.AreEqual(0, _ghost.ActiveAfterimageCount, "0.5 sn'de solar");

            _ghost.SetVisible(false);
            _ghost.Apply(new GhostPose { Position = Vector2.zero, IsDashing = true });
            for (int i = 0; i < 5; i++) _ghost.TickEffects(0.05f);
            Assert.AreEqual(0, _ghost.ActiveAfterimageCount, "görünmezken kopya yok");
        }

        [Test]
        public void Trail_EmitsWhileSwingingOrDashing()
        {
            var trailSource = new GameObject("Trail").AddComponent<TrailRenderer>();
            trailSource.transform.SetParent(_source.transform, false);
            trailSource.time = 0.25f;
            var dash = new GhostDashSource { Trail = trailSource, Color = Color.white };
            _ghost = GhostView.CreateFrom(_body, _animator, _rope, null, null, dash);
            _ghost.SetVisible(true);
            var trail = _ghost.GetComponentInChildren<TrailRenderer>(true);
            Assert.IsNotNull(trail);

            _ghost.Apply(new GhostPose { Position = Vector2.zero, IsSwinging = true });
            _ghost.TickEffects(0.016f);
            Assert.IsTrue(trail.emitting);

            _ghost.Apply(new GhostPose { Position = Vector2.zero });
            _ghost.TickEffects(0.016f);
            Assert.IsFalse(trail.emitting);
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
