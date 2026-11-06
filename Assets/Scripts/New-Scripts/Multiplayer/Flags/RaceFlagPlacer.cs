using System.Collections.Generic;
using System.Text;
using New_Scripts.LevelChange;
using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>
    /// RaceFlagPoint'i olmayan odalara işareti varsayılan yere koyar: odadan RoomId + 1'e giden geçişin hemen önü;
    /// böyle bir geçiş yoksa (son oda, yan oda) oda sınırlarının merkezi. Var olan (taşınmış olabilir) işaretlere dokunmaz.
    /// </summary>
    public static class RaceFlagPlacer
    {
        public sealed class Placement
        {
            public Room Room;
            public RaceFlagPoint Point;
            public bool UsedFallback;
        }

        public sealed class PlacementReport
        {
            public readonly List<Placement> Created = new List<Placement>();
            /// <summary>RoomId'si 0 olduğu için atlanan odalar (sıra bilinmiyor).</summary>
            public readonly List<Room> Skipped = new List<Room>();

            public override string ToString()
            {
                var sb = new StringBuilder($"{Created.Count} işaret kondu");
                foreach (var p in Created)
                    if (p.UsedFallback) sb.Append($"; oda {p.Room.RoomId} ({p.Room.name}) merkezde, elle taşı");
                foreach (var r in Skipped) sb.Append($"; {r.name} atlandı (RoomId 0)");
                return sb.ToString();
            }
        }

        public static PlacementReport PlaceMissing(IEnumerable<Room> rooms)
        {
            var report = new PlacementReport();
            foreach (var room in rooms)
            {
                if (room == null || room.GetComponentInChildren<RaceFlagPoint>(true) != null) continue;
                if (room.RoomId == 0)
                {
                    report.Skipped.Add(room);
                    continue;
                }

                bool fallback = !TryFindExitToNext(room, out Vector2 position, out bool horizontal);
                if (fallback) position = room.RoomBounds != null
                    ? (Vector2)RaceFlagPlacement.WorldBounds(room.RoomBounds).center
                    : (Vector2)room.transform.position;
                // Dikey çıkışta (kuyu, delik) aşağısı boşluk olabilir; orada zemine indirilmez.
                if (fallback || horizontal) position = SnapToGround(position);

                var go = new GameObject(RaceFlagPoint.ObjectName);
                go.transform.SetParent(room.transform, false);
                go.transform.position = new Vector3(position.x, position.y, room.transform.position.z);
                report.Created.Add(new Placement { Room = room, Point = go.AddComponent<RaceFlagPoint>(), UsedFallback = fallback });
            }
            return report;
        }

        private static bool TryFindExitToNext(Room room, out Vector2 position, out bool horizontal)
        {
            foreach (var trigger in room.GetComponentsInChildren<RoomTransitionTrigger>(true))
            {
                if (trigger.TargetRoom == null || trigger.TargetRoom.RoomId != room.RoomId + 1) continue;
                var box = trigger.GetComponent<BoxCollider2D>();
                position = RaceFlagPlacement.InFrontOf(RaceFlagPlacement.WorldBounds(box), trigger.Direction);
                horizontal = trigger.Direction == TransitionDirection.Left || trigger.Direction == TransitionDirection.Right;
                return true;
            }
            position = default;
            horizontal = false;
            return false;
        }

        /// <summary>Aşağıda MaxGroundDrop içinde Ground/OneWay zemini varsa işareti onun yüzeyine indirir (trigger'lar sayılmaz).</summary>
        private static Vector2 SnapToGround(Vector2 position)
        {
            Physics2D.SyncTransforms(); // editörde yeni oluşturulan ya da taşınan collider'lar sorguya girsin
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(LayerMask.GetMask("Ground", "OneWay"));
            var hits = new RaycastHit2D[1];
            if (Physics2D.Raycast(position, Vector2.down, filter, hits, RaceFlagPlacement.MaxGroundDrop) > 0)
                position.y = hits[0].point.y;
            return position;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Rock The Game/Multiplayer/Bayrak işaretlerini yerleştir")]
        private static void PlaceMissingInOpenScenes()
        {
            var rooms = Object.FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var report = PlaceMissing(rooms);
            foreach (var p in report.Created)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(p.Point.gameObject, "Bayrak işaretlerini yerleştir");
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(p.Room.gameObject.scene);
            }
            Debug.Log($"[MP] {report}");
        }
#endif
    }
}
