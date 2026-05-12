using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Hareketli platformların (Sürücülerin) taşıyabileceği yolcuları tanımlayan arayüz.
    /// </summary>
    public interface IPassenger
    {
        void MoveWithPlatform(Vector2 deltaPosition);
    }
}