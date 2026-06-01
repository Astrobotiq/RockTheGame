using UnityEngine;
using System.Collections.Generic;

namespace New_Scripts.KinematicActionSystem.Core
{
    /// <summary>
    /// Harici paket bağımlılığı olmadan pürüzsüz spline rotaları oluşturmak için Catmull-Rom interpolasyon bileşeni.
    /// </summary>
    public class KinematicSplinePath : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();
        [SerializeField] private bool loop = false;

        public List<Transform> Waypoints => waypoints;

        /// <summary>
        /// Rota üzerindeki 0 ile 1 arasındaki normalize süreye denk gelen dünya pozisyonunu hesaplar.
        /// </summary>
        public Vector3 GetPoint(float t)
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                return transform.position;
            }

            if (waypoints.Count == 1)
            {
                return waypoints[0] != null ? waypoints[0].position : transform.position;
            }

            t = Mathf.Clamp01(t);

            // Nokta sayısına göre segment tespiti
            int numSections = loop ? waypoints.Count : waypoints.Count - 1;
            int currSection = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
            float u = (t * numSections) - currSection;

            // Catmull-Rom noktalarını belirleme
            Vector3 p0, p1, p2, p3;

            if (loop)
            {
                p0 = GetWaypointPosition((currSection - 1 + waypoints.Count) % waypoints.Count);
                p1 = GetWaypointPosition(currSection);
                p2 = GetWaypointPosition((currSection + 1) % waypoints.Count);
                p3 = GetWaypointPosition((currSection + 2) % waypoints.Count);
            }
            else
            {
                p0 = GetWaypointPosition(Mathf.Max(currSection - 1, 0));
                p1 = GetWaypointPosition(currSection);
                p2 = GetWaypointPosition(Mathf.Min(currSection + 1, waypoints.Count - 1));
                p3 = GetWaypointPosition(Mathf.Min(currSection + 2, waypoints.Count - 1));
            }

            return GetCatmullRomPosition(u, p0, p1, p2, p3);
        }

        private Vector3 GetWaypointPosition(int index)
        {
            if (index < 0 || index >= waypoints.Count || waypoints[index] == null)
            {
                return transform.position;
            }
            return waypoints[index].position;
        }

        private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count < 2) return;

            Gizmos.color = Color.cyan;
            Vector3 lastPos = GetPoint(0f);
            int resolutions = waypoints.Count * 10;

            for (int i = 1; i <= resolutions; i++)
            {
                float t = (float)i / resolutions;
                Vector3 currentPos = GetPoint(t);
                Gizmos.DrawLine(lastPos, currentPos);
                lastPos = currentPos;
            }

            // Waypointlerin yerini çiz
            Gizmos.color = Color.blue;
            foreach (var wp in waypoints)
            {
                if (wp != null)
                {
                    Gizmos.DrawSphere(wp.position, 0.15f);
                }
            }
        }
    }
}
