using System;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Oyuncunun altın/çilek envanterini yöneten ve verileri taşıyan ScriptableObject.
    /// Checkpoint öncesi toplananlar geçici hafızada (sessionCoins) tutulur.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleInventory", menuName = "Collectible/Inventory SO")]
    public class CollectibleInventorySO : ScriptableObject
    {
        [SerializeField] private int totalCoins;
        
        private int sessionCoins; // Checkpoint alınana kadar geçici toplanan miktar

        public int TotalCoins => totalCoins + sessionCoins;
        
        public event Action<int> OnCoinsChanged;

        public void AddCoin()
        {
            sessionCoins++;
            OnCoinsChanged?.Invoke(TotalCoins);
        }

        public void Commit()
        {
            totalCoins += sessionCoins;
            sessionCoins = 0;
        }

        public void Revert()
        {
            sessionCoins = 0;
            OnCoinsChanged?.Invoke(TotalCoins);
        }

        public void ResetInventory()
        {
            totalCoins = 0;
            sessionCoins = 0;
            OnCoinsChanged?.Invoke(0);
        }

        private void OnEnable()
        {
            sessionCoins = 0;
        }
    }
}
