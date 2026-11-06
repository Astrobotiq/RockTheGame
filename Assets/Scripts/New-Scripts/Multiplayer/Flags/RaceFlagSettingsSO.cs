using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>Yarış bayrağının görünüşü ve dokunma alanı. Resources/Multiplayer/RaceFlagSettings'ten yüklenir.</summary>
    [CreateAssetMenu(menuName = "Rock The Game/Multiplayer/Race Flag Settings", fileName = "RaceFlagSettings")]
    public sealed class RaceFlagSettingsSO : ScriptableObject
    {
        public const string ResourcePath = "Multiplayer/RaceFlagSettings";

        [Tooltip("Bool 'isEntered' ile direkten bayrak kaldıran animator (Assets/Animator/Flag.controller).")]
        public RuntimeAnimatorController Animator;
        [Tooltip("Animator yokken ya da ilk karede görünen sprite.")]
        public Sprite Sprite;
        public Vector3 Scale = Vector3.one;
        [Tooltip("Görselin işarete göre kayması (sprite pivotu ortadaysa yukarı kaldırmak için).")]
        public Vector2 VisualOffset = Vector2.zero;
        public Vector2 TriggerSize = new Vector2(1.5f, 3f);
        public string SortingLayerName = "Default";
        public int SortingOrder;

        public static RaceFlagSettingsSO Load()
        {
            var settings = Resources.Load<RaceFlagSettingsSO>(ResourcePath);
            return settings != null ? settings : CreateInstance<RaceFlagSettingsSO>();
        }
    }
}
