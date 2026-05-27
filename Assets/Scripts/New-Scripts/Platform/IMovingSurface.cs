using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Üzerindeki nesnelere hız aktarabilen hareketli yüzeylerin uygulaması gereken arayüz.
    /// </summary>
    public interface IMovingSurface
    {
        Vector2 DeltaPosition { get; }
        Vector2 SurfaceVelocity { get; }
    }
}