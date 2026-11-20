using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Tutulan node tutulamaz hale geldiğinde (<see cref="Nodes.IGrabbableNode"/>) bir kolu tetik bırakılmış gibi bırakabilen durum.
    /// Slingshot ve diğer durumlar bunu uygulamaz: gerilme çok kısa sürer, anchor'lar zaten sabitlenmiştir.
    /// </summary>
    public interface INodeReleaseHandler
    {
        /// <param name="arm">Bırakılacak kol.</param>
        /// <param name="impulse">Node'dan dışa doğru verilecek hız (dünya).</param>
        void ReleaseByNode(ActiveArm arm, Vector2 impulse);
    }
}
