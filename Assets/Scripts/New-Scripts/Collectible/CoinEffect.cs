using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Oyuncunun envanterindeki altın/çilek miktarını arttıran toplanabilir parça efekti.
    /// </summary>
    public class CoinEffect : CollectibleEffect
    {
        [SerializeField] private CollectibleInventorySO inventory;

        public override void Apply(PlayerController player)
        {
            if (inventory != null)
            {
                inventory.AddCoin();
            }
            else
            {
                Debug.LogWarning("CoinEffect: CollectibleInventorySO referansı eksik!");
            }
        }
    }
}
