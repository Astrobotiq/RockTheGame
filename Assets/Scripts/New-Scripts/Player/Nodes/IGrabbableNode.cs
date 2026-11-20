using UnityEngine;

namespace New_Scripts.Player.Nodes
{
    /// <summary>
    /// Zamana bağlı tutulabilen node (kapanan çiçek gibi). Node'un kökünde ya da üstündeki bir objede durur;
    /// <see cref="PlayerController"/> tutulan node'a <c>GetComponentInParent</c> ile bakar. Node tutulamaz hale gelince
    /// (kapanışta) o kol bırakılır ve oyuncu node'dan <see cref="ReleaseImpulse"/> kadar itilir.
    /// Tutulabilirlik algılaması node'un collider'ına bağlıdır: tutulamazken collider kapalı olmalıdır (NodeDetector görmesin).
    /// </summary>
    public interface IGrabbableNode
    {
        bool IsGrabbable { get; }

        /// <summary>Node tutulamaz hale gelip kanca koptuğunda oyuncuya node'dan dışa doğru verilen hız (b/sn).</summary>
        float ReleaseImpulse { get; }
    }
}
