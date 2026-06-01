using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Toplanabilir parçaların toplandığında ne yapacağını belirleyen soyut temel sınıf (Efekt bileşeni).
    /// </summary>
    public abstract class CollectibleEffect : MonoBehaviour
    {
        public abstract void Apply(PlayerController player);
    }
}
