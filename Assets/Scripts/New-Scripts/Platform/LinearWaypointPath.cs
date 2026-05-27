using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Lineer bir rota üzerinde sırasıyla gidip gelme mantığını sağlayan veri sınıfı.
    /// </summary>
    public class LinearWaypointPath : MonoBehaviour, IWaypointPath
    {
        [SerializeField] private Vector2[] waypoints;

        public Vector2 GetWaypoint(int index)
        {
            return this.waypoints[index];
        }

        public int GetNextIndex(int currentIndex, ref bool movingForward)
        {
            if (movingForward)
            {
                currentIndex++;
                if (currentIndex >= this.waypoints.Length)
                {
                    currentIndex = this.waypoints.Length - 2;
                    movingForward = false;
                }
            }
            else
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = 1;
                    movingForward = true;
                }
            }

            return currentIndex;
        }

        public bool IsValid()
        {
            return this.waypoints != null && this.waypoints.Length >= 2;
        }
    }
}