using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Oyuncunun slingshot atış hakkını yenileyen toplanabilir parça efekti.
    /// </summary>
    public class SlingshotRefillEffect : CollectibleEffect
    {
        public override void Apply(PlayerController player)
        {
            if (player != null)
            {
                player.ResetSlingshot();
                Debug.Log("Slingshot yenilendi!");
            }
        }
    }
}
