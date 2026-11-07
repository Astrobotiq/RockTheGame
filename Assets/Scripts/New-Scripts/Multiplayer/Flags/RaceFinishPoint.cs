using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>
    /// Yarışın bitiş çizgisinin yeri (ana yolun son odasında). Yalnız işaret: görüntüsü, collider'ı, Update'i yok; tek kişilik
    /// oyunda hiçbir şey yapmaz. Multiplayer'da buraya RaceFinishLine kurulur. Geliştirici sahnede sürükleyip taşıyabilir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RaceFinishPoint : MonoBehaviour
    {
        public const string ObjectName = "RaceFinishPoint";

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Dama bayrağı: direk + 2x2 kare.
            Vector3 p = transform.position;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(p, p + Vector3.up * 2.5f);
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            {
                Gizmos.color = (x + y) % 2 == 0 ? Color.white : Color.black;
                Gizmos.DrawCube(p + new Vector3(0.25f + x * 0.5f, 2.25f - y * 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.01f));
            }
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(p, 0.2f);
            UnityEditor.Handles.Label(p + Vector3.up * 2.9f, "Bitiş");
        }
#endif
    }
}
