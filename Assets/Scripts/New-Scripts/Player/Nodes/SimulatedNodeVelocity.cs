using UnityEngine;

namespace New_Scripts.Player.Nodes
{
    /// <summary>
    /// Test/tasarım aracı: yerinde duran node'u, kancadan bırakışta (tek, çift, slingshot) bu hızla hareket ediyormuş
    /// gibi gösterir. Node hareket etmez; taşıma ve atalet yoktur, yalnızca bırakışta aktarılan node hızına eklenir.
    /// Hareketli node bırakışının iniş yerine etkisini karşılaştırmak için (docs/systems/grapple.md "Hareketli node").
    /// </summary>
    public class SimulatedNodeVelocity : MonoBehaviour
    {
        [SerializeField] private Vector2 velocity = new Vector2(10f, 0f);

        public Vector2 Velocity => velocity;
    }
}
