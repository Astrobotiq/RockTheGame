using New_Scripts.Player;
using New_Scripts.Player.Visual;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Toplanabilir nesne alındığında karakterin göz bebeklerini belirtilen şekle çeviren efekt.
    /// </summary>
    public class EyeShapeCollectibleEffect : CollectibleEffect
    {
        [SerializeField] private EyeInsideShape shapeToSet = EyeInsideShape.Key;

        public override void Apply(PlayerController player)
        {
            var eyesController = player.GetComponentInChildren<PlayerEyesController>();
            if (eyesController != null)
            {
                eyesController.SetEyeInsideShape(shapeToSet);
            }
            else
            {
                Debug.LogWarning("EyeShapeCollectibleEffect: PlayerEyesController oyuncu üzerinde bulunamadı!");
            }
        }
    }
}
