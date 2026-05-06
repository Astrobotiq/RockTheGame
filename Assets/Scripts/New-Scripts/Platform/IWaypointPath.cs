using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Platform rotalarının veri sözleşmesini ve indeks mantığını sağlayan arayüz.
    /// </summary>
    public interface IWaypointPath
    {
        Vector2 GetWaypoint(int index);
        int GetNextIndex(int currentIndex, ref bool movingForward);
        bool IsValid();
    }
}