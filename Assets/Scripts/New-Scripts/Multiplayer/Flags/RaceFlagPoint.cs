using New_Scripts.LevelChange;
using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>
    /// Odadaki yarış bayrağının yeri. Yalnız işaret: görüntüsü, collider'ı, Update'i yok; tek kişilik oyunda hiçbir şey
    /// yapmaz. Yarış modunda başlatıcı bu noktaya bir RaceFlag kurar. Room'un doğrudan çocuğudur (DynamicContent'in değil),
    /// geliştirici sahnede sürükleyip taşıyabilir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceFlagPoint : MonoBehaviour
    {
        public const string ObjectName = "RaceFlagPoint";

        public Room Room => GetComponentInParent<Room>(true);
        public int RoomId => Room != null ? Room.RoomId : 0;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Direk ve üçgen bayrak; sahnede tıklanıp sürüklenebilsin diye dolu bir tutamak da çizilir.
            Vector3 p = transform.position;
            Gizmos.color = new Color(1f, 0.85f, 0.2f);
            Gizmos.DrawLine(p, p + Vector3.up * 2f);
            Gizmos.DrawLine(p + Vector3.up * 2f, p + new Vector3(1f, 1.6f, 0f));
            Gizmos.DrawLine(p + new Vector3(1f, 1.6f, 0f), p + Vector3.up * 1.2f);
            Gizmos.DrawSphere(p, 0.2f);
            UnityEditor.Handles.Label(p + Vector3.up * 2.4f, $"Bayrak {RoomId}");
        }
#endif
    }
}
