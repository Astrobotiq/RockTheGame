using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Uzayan kolların görsel ayarları. His kullanıcının kararıdır, gamepad ile denenip ayarlanır.
    /// </summary>
    [CreateAssetMenu(fileName = "ArmVisualSettings", menuName = "Player/Arm Visual Settings")]
    public class ArmVisualSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float IdleReach { get; private set; } = 1f;
        /// <summary>Nişan yokken kolun gövdeden yana açılma payı; kollar gövdenin içinde bacak gibi durmasın.</summary>
        [field: SerializeField] public float IdleSpread { get; private set; } = 0.7f;
        /// <summary>Yerdeyken elin zeminden yüksekliği (el yerde sürünür).</summary>
        [field: SerializeField] public float GroundHandHeight { get; private set; } = 0.2f;
        /// <summary>Yerde koşarken elin hızın tersine geride kalma katsayısı (dünya birimi / hız).</summary>
        [field: SerializeField] public float GroundTrail { get; private set; } = 0.18f;
        /// <summary>Nişan yokken elin omuzdan en fazla ne kadar aşağı sarkacağı.</summary>
        [field: SerializeField] public float IdleDrop { get; private set; } = 0.3f;
        [Header("Duvar")]
        /// <summary>Duvarda elin gövde merkezinden duvara doğru yatay uzaklığı.</summary>
        [field: SerializeField] public float WallHandX { get; private set; } = 0.6f;
        /// <summary>Tırmanırken elin gövdeye göre en alçak ve en yüksek noktası.</summary>
        [field: SerializeField] public float WallHandLow { get; private set; } = -0.4f;
        [field: SerializeField] public float WallHandHigh { get; private set; } = 1.1f;
        /// <summary>Duvarda kayarken (tırmanmıyorken) elin gövdeye göre yüksekliği.</summary>
        [field: SerializeField] public float WallSlideHandY { get; private set; } = 0.5f;
        [Header("Genel")]
        [field: SerializeField] public float IdleBow { get; private set; } = 0.25f;
        [field: SerializeField] public float HandStiffness { get; private set; } = 220f;
        [field: SerializeField] public float HandDamping { get; private set; } = 12f;
        [field: SerializeField] public float BowStiffness { get; private set; } = 90f;
        [field: SerializeField] public float BowDamping { get; private set; } = 6f;
        [field: SerializeField] public float DroopTrail { get; private set; } = 0.04f;
        [field: SerializeField] public float ExtendDuration { get; private set; } = 0.1f;
        [field: SerializeField] public float ShoulderWidth { get; private set; } = 0.2f;
        [field: SerializeField] public float HandWidth { get; private set; } = 0.34f;
        [field: SerializeField] public int Resolution { get; private set; } = 20;
        [field: SerializeField] public Color ArmColor { get; private set; } = new Color32(0xFF, 0x67, 0x36, 0xFF);
        [field: SerializeField] public Color OutlineColor { get; private set; } = new Color32(0x2B, 0x14, 0x10, 0xFF);
        /// <summary>Dış çizginin kolun toplam genişliğine eklediği pay (dünya birimi, 2 piksel ≈ 0.083).</summary>
        [field: SerializeField] public float OutlineExtra { get; private set; } = 0.083f;

        [Header("El")]
        [field: SerializeField] public Sprite OpenHand { get; private set; }
        [field: SerializeField] public Sprite ClosedHand { get; private set; }
        /// <summary>Duvara tutunan el: parmaklar yandan duvara kıvrılır (tırmanırken tutma anı).</summary>
        [field: SerializeField] public Sprite WallGripHand { get; private set; }
        /// <summary>Tutunma anında elin büyüyüp yerine oturması (yay itişi).</summary>
        [field: SerializeField] public float GrabPunch { get; private set; } = 7f;
        [field: SerializeField] public float PunchStiffness { get; private set; } = 300f;
        [field: SerializeField] public float PunchDamping { get; private set; } = 14f;
    }
}
