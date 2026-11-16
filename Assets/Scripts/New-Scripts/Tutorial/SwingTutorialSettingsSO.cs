using UnityEngine;

namespace New_Scripts.Tutorial
{
    /// <summary>
    /// Swing tutorial'ının (oda 5, ilk node) zaman ve süre ayarları. Tüm süreler unscaled saniyedir.
    /// Değerler his kararıdır: Play Mode'da asset'i değiştirip gamepad ile dene (docs/swing-tutorial-plani.md).
    /// </summary>
    [CreateAssetMenu(fileName = "SwingTutorialSettings", menuName = "RockTheGame/Tutorial/Swing Tutorial Settings")]
    public class SwingTutorialSettingsSO : ScriptableObject
    {
        [Header("Zaman ölçeği")]
        [Tooltip("Node menzile girince yükselişte ulaşılan yavaş zaman ölçeği.")]
        [SerializeField, Range(0f, 1f)] private float slowScale = 0.1f;
        [Tooltip("Tepe noktasında bekleme ölçeği. 0 = zaman durur (node menzilden çıkamaz). Artırılırsa 'menzilden çıktıysa 0'a in' güvencesi gerekir.")]
        [SerializeField, Range(0f, 1f)] private float holdScale = 0f;

        [Header("Rampalar (unscaled sn)")]
        [SerializeField, Min(0f)] private float rampInDuration = 0.15f;
        [SerializeField, Min(0f)] private float rampOutDuration = 0.3f;

        [Header("Nişan (unscaled sn)")]
        [Tooltip("Sağ stick node'u bu kadar süre seçili kalırsa ateşle adımına geçilir.")]
        [SerializeField, Min(0f)] private float aimConfirmTime = 0.2f;
        [Tooltip("Ateşle adımında nişan bu kadar süre kaybolursa kolu çevir adımına dönülür.")]
        [SerializeField, Min(0f)] private float aimLostGrace = 0.3f;

        public float SlowScale => slowScale;
        public float HoldScale => holdScale;
        public float RampInDuration => rampInDuration;
        public float RampOutDuration => rampOutDuration;
        public float AimConfirmTime => aimConfirmTime;
        public float AimLostGrace => aimLostGrace;
    }
}
