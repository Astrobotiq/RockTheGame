using System.Collections.Generic;
using NUnit.Framework;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Flags;
using UnityEditor;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Bayrak işaretinin varsayılan yeri ve yerleştirme komutu: bir sonraki odaya giden geçişin hemen önü,
    /// yoksa oda merkezi; yalnız eksik odalar doldurulur. docs/superpowers/plans/2026-11-06-mp5-flags.md → Task 2.
    /// </summary>
    public class RaceFlagPlacementTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created) if (go != null) Object.DestroyImmediate(go);
            _created.Clear();
        }

        private Room MakeRoom(int id, Vector2 boundsCenter, Vector2 boundsSize)
        {
            var go = new GameObject($"Room-{id}");
            _created.Add(go);
            var bounds = go.AddComponent<BoxCollider2D>();
            bounds.isTrigger = true;
            bounds.offset = boundsCenter;
            bounds.size = boundsSize;
            var room = go.AddComponent<Room>();
            var so = new SerializedObject(room);
            so.FindProperty("roomId").intValue = id;
            so.FindProperty("roomBounds").objectReferenceValue = bounds;
            so.ApplyModifiedPropertiesWithoutUndo();
            return room;
        }

        private static void AddTrigger(Room from, Room to, TransitionDirection direction, Vector2 center, Vector2 size)
        {
            var go = new GameObject($"Transition-{to.RoomId}");
            go.transform.SetParent(from.transform, false);
            go.transform.position = center;
            var trigger = go.AddComponent<RoomTransitionTrigger>();
            go.GetComponent<BoxCollider2D>().size = size;
            var so = new SerializedObject(trigger);
            so.FindProperty("targetRoom").objectReferenceValue = to;
            so.FindProperty("direction").enumValueIndex = (int)direction;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void InFrontOf_EachDirection_BacksOffFromEntrySide()
        {
            var trigger = new Bounds(new Vector3(10f, 20f, 0f), new Vector3(2f, 4f, 0f)); // x 9..11, y 18..22
            float m = RaceFlagPlacement.Margin;

            Assert.AreEqual(new Vector2(9f - m, 20f), RaceFlagPlacement.InFrontOf(trigger, TransitionDirection.Right));
            Assert.AreEqual(new Vector2(11f + m, 20f), RaceFlagPlacement.InFrontOf(trigger, TransitionDirection.Left));
            Assert.AreEqual(new Vector2(10f, 18f - m), RaceFlagPlacement.InFrontOf(trigger, TransitionDirection.Up));
            Assert.AreEqual(new Vector2(10f, 22f + m), RaceFlagPlacement.InFrontOf(trigger, TransitionDirection.Down));
        }

        [Test]
        public void PlaceMissing_UsesTriggerToNextRoom_NotTheWayBack()
        {
            var previous = MakeRoom(1, Vector2.zero, Vector2.one);
            var room = MakeRoom(2, new Vector2(50f, 0f), new Vector2(40f, 20f));
            var next = MakeRoom(3, new Vector2(100f, 0f), Vector2.one);
            AddTrigger(room, previous, TransitionDirection.Left, new Vector2(31f, 0f), new Vector2(1f, 4f));
            AddTrigger(room, next, TransitionDirection.Right, new Vector2(69f, 2f), new Vector2(1f, 4f));

            var report = RaceFlagPlacer.PlaceMissing(new[] { room });

            Assert.AreEqual(1, report.Created.Count);
            Assert.IsFalse(report.Created[0].UsedFallback);
            var point = room.GetComponentInChildren<RaceFlagPoint>();
            Assert.AreEqual(new Vector3(68.5f - RaceFlagPlacement.Margin, 2f, 0f), point.transform.position);
        }

        private void MakeGround(Vector2 center, Vector2 size)
        {
            var go = new GameObject("Ground") { layer = LayerMask.NameToLayer("Ground") };
            _created.Add(go);
            go.transform.position = center;
            go.AddComponent<BoxCollider2D>().size = size;
        }

        [Test]
        public void PlaceMissing_HorizontalExit_SnapsDownToGroundWithinReach()
        {
            var room = MakeRoom(2, new Vector2(50f, 0f), new Vector2(40f, 20f));
            var next = MakeRoom(3, new Vector2(100f, 0f), Vector2.one);
            AddTrigger(room, next, TransitionDirection.Right, new Vector2(69f, 2f), new Vector2(1f, 4f));
            MakeGround(new Vector2(60f, -1f), new Vector2(30f, 2f)); // üst yüzey y = 0

            RaceFlagPlacer.PlaceMissing(new[] { room });

            var p = room.GetComponentInChildren<RaceFlagPoint>().transform.position;
            Assert.AreEqual(68.5f - RaceFlagPlacement.Margin, p.x, 1e-4f);
            Assert.AreEqual(0f, p.y, 1e-3f);
        }

        [Test]
        public void PlaceMissing_GroundTooFar_StaysAtTriggerHeight()
        {
            var room = MakeRoom(2, new Vector2(50f, 0f), new Vector2(40f, 40f));
            var next = MakeRoom(3, new Vector2(100f, 0f), Vector2.one);
            AddTrigger(room, next, TransitionDirection.Right, new Vector2(69f, 2f), new Vector2(1f, 4f));
            MakeGround(new Vector2(60f, -1f - RaceFlagPlacement.MaxGroundDrop - 5f), new Vector2(30f, 2f));

            RaceFlagPlacer.PlaceMissing(new[] { room });

            Assert.AreEqual(2f, room.GetComponentInChildren<RaceFlagPoint>().transform.position.y, 1e-4f);
        }

        [Test]
        public void PlaceMissing_NoNextTrigger_FallsBackToBoundsCenter()
        {
            var room = MakeRoom(21, new Vector2(30f, 40f), new Vector2(10f, 10f));

            var report = RaceFlagPlacer.PlaceMissing(new[] { room });

            Assert.AreEqual(1, report.Created.Count);
            Assert.IsTrue(report.Created[0].UsedFallback);
            Assert.AreEqual(new Vector3(30f, 40f, 0f), room.GetComponentInChildren<RaceFlagPoint>().transform.position);
        }

        [Test]
        public void PlaceMissing_ExistingPoint_IsLeftWhereItIs()
        {
            var room = MakeRoom(4, Vector2.zero, new Vector2(10f, 10f));
            RaceFlagPlacer.PlaceMissing(new[] { room });
            var point = room.GetComponentInChildren<RaceFlagPoint>();
            point.transform.position = new Vector3(-3f, 7f, 0f); // geliştirici taşıdı

            var report = RaceFlagPlacer.PlaceMissing(new[] { room });

            Assert.AreEqual(0, report.Created.Count);
            Assert.AreEqual(1, room.GetComponentsInChildren<RaceFlagPoint>(true).Length);
            Assert.AreEqual(new Vector3(-3f, 7f, 0f), point.transform.position);
        }

        [Test]
        public void PlaceFinish_GoesToLastMainPathRoom_NotSideRoom_AndOnlyOnce()
        {
            var r1 = MakeRoom(1, Vector2.zero, new Vector2(10f, 10f));
            var r2 = MakeRoom(2, new Vector2(20f, 0f), new Vector2(10f, 10f));
            var side = MakeRoom(3, new Vector2(0f, 20f), new Vector2(10f, 10f)); // oda 1'den açılan yan oda (2'den gelinmiyor)
            AddTrigger(r1, r2, TransitionDirection.Right, new Vector2(5f, 0f), new Vector2(1f, 4f));
            AddTrigger(r1, side, TransitionDirection.Up, new Vector2(0f, 5f), new Vector2(4f, 1f));
            var rooms = new[] { r1, r2, side };

            var placed = RaceFlagPlacer.PlaceFinishIfMissing(rooms);

            Assert.AreEqual(r2, placed, "ana yolun son odası: bir öncekinden girilen en büyük kimlik");
            Assert.IsNotNull(r2.GetComponentInChildren<RaceFinishPoint>());
            Assert.IsNull(side.GetComponentInChildren<RaceFinishPoint>());
            Assert.AreEqual(new Vector3(20f, 0f, 0f), r2.GetComponentInChildren<RaceFinishPoint>().transform.position);

            Assert.IsNull(RaceFlagPlacer.PlaceFinishIfMissing(rooms), "zaten varsa ikincisi konmaz");
            Assert.AreEqual(1, Object.FindObjectsByType<RaceFinishPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
        }

        [Test]
        public void PlaceMissing_RoomIdZero_IsSkipped()
        {
            var room = MakeRoom(0, Vector2.zero, Vector2.one);

            var report = RaceFlagPlacer.PlaceMissing(new[] { room });

            Assert.AreEqual(0, report.Created.Count);
            CollectionAssert.Contains(report.Skipped, room);
            Assert.IsNull(room.GetComponentInChildren<RaceFlagPoint>());
        }

        [Test]
        public void Point_IsDirectChildOfRoom_ReportsRoomId_HasNoColliderOrRenderer()
        {
            var room = MakeRoom(7, Vector2.zero, Vector2.one);
            RaceFlagPlacer.PlaceMissing(new[] { room });

            var point = room.GetComponentInChildren<RaceFlagPoint>();
            Assert.AreEqual(room.transform, point.transform.parent);
            Assert.AreEqual(RaceFlagPoint.ObjectName, point.gameObject.name);
            Assert.AreEqual(7, point.RoomId);
            Assert.IsNull(point.GetComponent<Collider2D>());
            Assert.IsNull(point.GetComponent<Renderer>());
        }
    }
}
