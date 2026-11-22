using UnityEngine;

namespace New_Scripts.Player.Nodes
{
    /// <summary>
    /// Kancayla tutunulduğunu ve bırakıldığını bilmek isteyen node (çiçeğin yaprak tepkisi, polen). Node'un kökünde ya da
    /// üstündeki bir objede durur; <see cref="PlayerController"/> tutulan node'a <c>GetComponentInParent</c> ile bakar.
    /// <see cref="IGrabbableNode"/>'dan bağımsızdır: tutulabilirlik kuralını değiştirmez, yalnızca haber verir.
    ///
    /// Tutunma tek noktadan (<see cref="PlayerController.AttachAnchor"/>) bildirilir. Bırakma çok yoldan olur (el bırakma, çiçeğin
    /// kapanması, ölüm, başka node'a tutunma), bu yüzden <see cref="PlayerController"/> kolun node'unun değiştiğini her karenin
    /// sonunda yoklayıp <see cref="OnReleased"/> çağırır; uygulayan taraf bırakmayı kendisi aramaz.
    /// </summary>
    public interface IGrabReactive
    {
        /// <summary><paramref name="arm"/> (Left/Right) bu node'a tutundu; <paramref name="worldPoint"/> tutunma noktasıdır (dünya).</summary>
        void OnGrabbed(ActiveArm arm, Vector2 worldPoint);

        /// <summary><paramref name="arm"/> bu node'u bıraktı (her neden). Kol tutmuyorsa çağrılmaz.</summary>
        void OnReleased(ActiveArm arm);
    }
}
