using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Anahtar parçası (key shard) belirteci olan toplanabilir parça efekti.
    /// </summary>
    public class KeyShardEffect : CollectibleEffect
    {
        public override void Apply(PlayerController player)
        {
            Debug.Log("Anahtar parçası toplandı!");
        }
    }
}
