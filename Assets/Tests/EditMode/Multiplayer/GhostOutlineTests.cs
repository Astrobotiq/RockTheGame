using System.Collections.Generic;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Multiplayer.Messages;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Hayalet konturu (MPG3): zamanlama (giriş, bekleme, solma, tehlike ömrü), yerel nesneyle örtüşme kuralı,
    /// collider şeklinden yol çıkarma ve interpolatörün yüzey pozunu ara değerlemesi.
    /// </summary>
    public class GhostOutlineTests
    {
        // Enter 0.1, Linger 0.4, Fade 0.2, Hazard 0.6 (son 0.2 solma): ayarların varsayılanları.
        private static readonly GhostOutlineTiming Timing = new GhostOutlineTiming(0.1f, 0.4f, 0.2f, 0.6f, 0.2f);

        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created) if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        private static SurfacePose Pose(uint id, float x = 0f, float y = 0f, float rot = 0f) => new SurfacePose(id, new Vector2(x, y), rot);

        private static float AlphaOf(GhostOutlineTracker tracker, uint id)
        {
            var entries = new List<GhostOutlineEntry>();
            tracker.Collect(entries);
            foreach (var e in entries) if (e.Pose.Id == id) return e.Alpha;
            return -1f;
        }

        // ---- GhostOutlineTracker: platform / node ----

        [Test]
        public void Surface_FadesInOverEnterSeconds()
        {
            var tracker = new GhostOutlineTracker(Timing);
            var present = new List<SurfacePose> { Pose(1) };

            tracker.Tick(0.05f, present);
            Assert.AreEqual(0.5f, AlphaOf(tracker, 1), 1e-4f);

            tracker.Tick(0.05f, present);
            Assert.AreEqual(1f, AlphaOf(tracker, 1), 1e-4f);

            tracker.Tick(1f, present);
            Assert.AreEqual(1f, AlphaOf(tracker, 1), "tam görünürken 1'in üstüne çıkmaz");
        }

        [Test]
        public void Surface_AfterLeaving_HoldsThenFadesThenGoes()
        {
            var tracker = new GhostOutlineTracker(Timing);
            var present = new List<SurfacePose> { Pose(1) };
            tracker.Tick(0.5f, present);

            var none = new List<SurfacePose>();
            tracker.Tick(0.4f, none);
            Assert.AreEqual(1f, AlphaOf(tracker, 1), 1e-4f, "0.4 sn boyunca tam görünür kalır");

            tracker.Tick(0.1f, none);
            Assert.AreEqual(0.5f, AlphaOf(tracker, 1), 1e-4f, "bekleme bitti, solma yarıda");

            tracker.Tick(0.15f, none);
            Assert.AreEqual(-1f, AlphaOf(tracker, 1), "tamamen solunca listeden çıkar");
            Assert.AreEqual(0, tracker.Count);
        }

        [Test]
        public void Surface_KeepsLastPoseAfterLeaving()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.Tick(0.5f, new List<SurfacePose> { Pose(1, 3f, 4f, 10f) });
            tracker.Tick(0.1f, new List<SurfacePose>());

            var entries = new List<GhostOutlineEntry>();
            tracker.Collect(entries);

            Assert.AreEqual(new Vector2(3f, 4f), entries[0].Pose.Position);
        }

        [Test]
        public void Surface_FollowsNewPoseWhilePresent()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.Tick(0.05f, new List<SurfacePose> { Pose(1, 0f) });
            tracker.Tick(0.05f, new List<SurfacePose> { Pose(1, 5f) });

            var entries = new List<GhostOutlineEntry>();
            tracker.Collect(entries);

            Assert.AreEqual(5f, entries[0].Pose.Position.x);
        }

        [Test]
        public void Surface_ReturningDuringLinger_StaysVisible()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.Tick(0.5f, new List<SurfacePose> { Pose(1) });
            tracker.Tick(0.3f, new List<SurfacePose>());
            tracker.Tick(0.05f, new List<SurfacePose> { Pose(1) });
            tracker.Tick(0.3f, new List<SurfacePose>());

            Assert.AreEqual(1f, AlphaOf(tracker, 1), 1e-4f, "geri basınca bekleme sayacı yeniden başlar");
        }

        [Test]
        public void Surface_LeftDuringFadeIn_HoldsPartialAlpha()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.Tick(0.05f, new List<SurfacePose> { Pose(1) });
            tracker.Tick(0.2f, new List<SurfacePose>());

            Assert.AreEqual(0.5f, AlphaOf(tracker, 1), 1e-4f, "yarıda ayrılınca o saydamlıkta bekler, sıçramaz");
        }

        [Test]
        public void SameIdTwice_IsOneOutline()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.Tick(0.1f, new List<SurfacePose> { Pose(1), Pose(1), Pose(2) });

            Assert.AreEqual(2, tracker.Count, "taşıyıcı ile tutunulan node aynı yüzeyse tek kontur");
        }

        // ---- GhostOutlineTracker: tehlike ----

        [Test]
        public void Hazard_FullThenFadesThenGoes()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.AddHazard(Pose(9, 7f, 8f));

            Assert.AreEqual(1f, AlphaOf(tracker, 9), "ölüm anında tam görünür");

            tracker.Tick(0.4f, null);
            Assert.AreEqual(1f, AlphaOf(tracker, 9), 1e-4f, "ömrün ilk 0.4 sn'si tam");

            tracker.Tick(0.1f, null);
            Assert.AreEqual(0.5f, AlphaOf(tracker, 9), 1e-4f, "son 0.2 sn solma");

            tracker.Tick(0.1f, null);
            Assert.AreEqual(-1f, AlphaOf(tracker, 9), "0.6 sn dolunca kalkar");
        }

        [Test]
        public void Hazard_IsFrozenAtDeathPose()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.AddHazard(Pose(9, 7f, 8f, 45f));
            tracker.Tick(0.3f, new List<SurfacePose> { Pose(9, 100f, 100f) });

            var entries = new List<GhostOutlineEntry>();
            tracker.Collect(entries);

            var hazard = entries.Find(e => e.Pose.Position == new Vector2(7f, 8f));
            Assert.AreEqual(45f, hazard.Pose.Rotation, "tehlike ölüm anındaki pozda donuk kalır");
        }

        [Test]
        public void Hazard_CapsAtEightConcurrent()
        {
            var tracker = new GhostOutlineTracker(Timing);
            for (uint i = 0; i < 20; i++) tracker.AddHazard(Pose(i));

            Assert.AreEqual(8, tracker.Count);
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            var tracker = new GhostOutlineTracker(Timing);
            tracker.Tick(0.1f, new List<SurfacePose> { Pose(1) });
            tracker.AddHazard(Pose(2));

            tracker.Reset();

            Assert.AreEqual(0, tracker.Count);
        }

        // ---- GhostOutlineMath ----

        [Test]
        public void ShouldDraw_SkipsWhenRemoteMatchesLocal()
        {
            Assert.IsFalse(GhostOutlineMath.ShouldDraw(new Vector2(1f, 1f), new Vector2(1.1f, 1f), 0.25f));
            Assert.IsTrue(GhostOutlineMath.ShouldDraw(new Vector2(1f, 1f), new Vector2(1.5f, 1f), 0.25f));
            Assert.IsTrue(GhostOutlineMath.ShouldDraw(new Vector2(0f, 0f), new Vector2(0.25f, 0f), 0.25f), "eşiğin tam üstü çizilir");
        }

        [Test]
        public void PixelsToWorld_MatchesOrthographicHeight()
        {
            // Ortografik boy 5 → ekran yüksekliği 10 birim; 1080 piksel için 5 piksel ≈ 0.0463 birim.
            Assert.AreEqual(5f * 10f / 1080f, GhostOutlineMath.PixelsToWorld(5f, 5f, 1080f), 1e-5f);
            Assert.AreEqual(0f, GhostOutlineMath.PixelsToWorld(5f, 5f, 0f));
        }

        // ---- GhostOutlineShape ----

        private Transform MakeSurface(float x, float y, float scale = 1f)
        {
            var go = new GameObject("Platform");
            _created.Add(go);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            return go.transform;
        }

        [Test]
        public void Extract_Box_GivesFourCornersInSurfaceLocalSpace()
        {
            var surface = MakeSurface(10f, 5f);
            var box = surface.gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(4f, 2f);

            var paths = GhostOutlineShape.Extract(surface, new MovingSurfaceRegistry());

            Assert.AreEqual(1, paths.Count);
            Assert.IsTrue(paths[0].Loop);
            Assert.AreEqual(4, paths[0].Points.Length);
            // Köşeler dünya konumundan bağımsız, yüzeyin yerel uzayında.
            CollectionAssert.Contains(paths[0].Points, new Vector2(-2f, -1f));
            CollectionAssert.Contains(paths[0].Points, new Vector2(2f, 1f));
        }

        [Test]
        public void Extract_ScaledSurface_StoresPointsDividedByScale()
        {
            var surface = MakeSurface(0f, 0f, 2f);
            var box = surface.gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 1f);

            var paths = GhostOutlineShape.Extract(surface, new MovingSurfaceRegistry());

            // Yerel nokta ölçekten önceki değer: matris lossyScale ile çarpınca dünya boyuna döner.
            Assert.That(paths[0].Points, Does.Contain(new Vector2(0.5f, 0.5f)));
        }

        [Test]
        public void Extract_PrefersSolidOverTriggerColliders()
        {
            var surface = MakeSurface(0f, 0f);
            var solid = surface.gameObject.AddComponent<BoxCollider2D>();
            solid.size = new Vector2(2f, 2f);
            var trigger = surface.gameObject.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;

            var paths = GhostOutlineShape.Extract(surface, new MovingSurfaceRegistry());

            Assert.AreEqual(1, paths.Count, "dolu collider varken tetik (algılama alanı) çizilmez");
            Assert.AreEqual(4, paths[0].Points.Length);
        }

        [Test]
        public void Extract_OnlyTrigger_UsesTrigger()
        {
            var surface = MakeSurface(0f, 0f);
            var trigger = surface.gameObject.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(3f, 1f);

            Assert.AreEqual(1, GhostOutlineShape.Extract(surface, new MovingSurfaceRegistry()).Count, "tehlike alanı tetik");
        }

        [Test]
        public void Extract_SkipsChildrenOwnedByAnotherRegisteredDriver()
        {
            var surface = MakeSurface(0f, 0f);
            surface.gameObject.AddComponent<BoxCollider2D>();
            var childGo = new GameObject("OtherDriver");
            _created.Add(childGo);
            childGo.transform.SetParent(surface, false);
            childGo.AddComponent<BoxCollider2D>();
            var registry = new MovingSurfaceRegistry();
            registry.Register(childGo.transform);

            Assert.AreEqual(1, GhostOutlineShape.Extract(surface, registry).Count, "iç içe sürücünün collider'ı kendi konturuna ait");
        }

        [Test]
        public void Extract_Circle_IsApproximatedByPolygon()
        {
            var surface = MakeSurface(0f, 0f);
            surface.gameObject.AddComponent<CircleCollider2D>().radius = 1.5f;

            var paths = GhostOutlineShape.Extract(surface, new MovingSurfaceRegistry());

            Assert.Greater(paths[0].Points.Length, 8);
            foreach (var p in paths[0].Points) Assert.AreEqual(1.5f, p.magnitude, 1e-3f);
        }

        // ---- GhostInterpolator: yüzey pozu ----

        private static SnapshotMessage Snap(uint seq, float x, SurfacePose? carrier)
            => new SnapshotMessage { Sequence = seq, RoomId = 1, Position = new Vector2(x, 0f), Carrier = carrier };

        [Test]
        public void Interpolator_SameCarrier_BlendsItsPose()
        {
            var interpolator = new GhostInterpolator();
            interpolator.Add(Snap(20, 0f, Pose(5, 0f, 0f, 0f)), 1.0);
            interpolator.Add(Snap(21, 1f, Pose(5, 10f, 0f, 90f)), 1.05);

            // Gönderen zamanı 1.0..1.05; 0.1 sn geriden çizim: 1.025 anı için ortası değil, t'yi doğrudan örnekle.
            Assert.IsTrue(interpolator.TrySample(1.125, out GhostPose pose));

            Assert.IsTrue(pose.Carrier.HasValue);
            Assert.AreEqual(5u, pose.Carrier.Value.Id);
            Assert.AreEqual(5f, pose.Carrier.Value.Position.x, 1e-3f);
            Assert.AreEqual(45f, pose.Carrier.Value.Rotation, 1e-2f);
        }

        [Test]
        public void Interpolator_CarrierChanges_UsesNewerSurface()
        {
            var interpolator = new GhostInterpolator();
            interpolator.Add(Snap(20, 0f, Pose(5, 0f)), 1.0);
            interpolator.Add(Snap(21, 1f, Pose(6, 10f)), 1.05);

            Assert.IsTrue(interpolator.TrySample(1.125, out GhostPose pose));

            Assert.AreEqual(6u, pose.Carrier.Value.Id, "yüzey değişince iki platform arasında kayan kontur olmaz");
            Assert.AreEqual(10f, pose.Carrier.Value.Position.x, 1e-3f);
        }

        [Test]
        public void Interpolator_CarrierDropped_GivesNone()
        {
            var interpolator = new GhostInterpolator();
            interpolator.Add(Snap(20, 0f, Pose(5)), 1.0);
            interpolator.Add(Snap(21, 1f, null), 1.05);

            Assert.IsTrue(interpolator.TrySample(1.125, out GhostPose pose));

            Assert.IsFalse(pose.Carrier.HasValue);
        }
    }
}
