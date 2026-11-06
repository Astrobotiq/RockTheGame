using TMPro;
using UnityEngine;

namespace New_Scripts.Multiplayer.Hud
{
    /// <summary>Multiplayer HUD ve lobi metinlerinin görünüşü. Resources/Multiplayer/MultiplayerUISettings'ten yüklenir.</summary>
    [CreateAssetMenu(menuName = "Rock The Game/Multiplayer/UI Settings", fileName = "MultiplayerUISettings")]
    public sealed class MultiplayerUISettingsSO : ScriptableObject
    {
        public const string ResourcePath = "Multiplayer/MultiplayerUISettings";

        public TMP_FontAsset Font;
        public float InfoFontSize = 40f;
        public float CountdownFontSize = 200f;
        public Color TextColor = Color.white;
        public Color WarningColor = new Color(1f, 0.45f, 0.4f);

        public static MultiplayerUISettingsSO Load()
        {
            var settings = Resources.Load<MultiplayerUISettingsSO>(ResourcePath);
            return settings != null ? settings : CreateInstance<MultiplayerUISettingsSO>();
        }
    }
}
