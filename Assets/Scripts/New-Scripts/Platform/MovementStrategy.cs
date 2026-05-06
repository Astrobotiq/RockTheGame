using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Platformun hareket stratejisini tanımlayan soyut temel sınıf.
    /// </summary>
    public abstract class MovementStrategy : MonoBehaviour
    {
        public abstract Vector2 GetPositionAtTime(float time);
    }
}