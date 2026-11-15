using UnityEngine;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;

namespace New_Scripts.Player
{
    public enum CameraFocusType
    {
        Player,
        StaticPosition,
        TargetTransform,
        PlayerAndTransform,
        /// <summary>Kamera sabit bir dünya dikdörtgenini gösterir; ortho boyu en-boy oranından otomatik (CameraFraming.Fit).</summary>
        FrameArea
    }

    [System.Serializable]
    public class CameraOverrideSettings
    {
        [Header("Focus Settings")]
        public CameraFocusType focusType = CameraFocusType.Player;

        [ConditionalField(nameof(focusType), false, CameraFocusType.StaticPosition)]
        public Vector2 staticFocusPosition;

        [ConditionalField(nameof(focusType), false, CameraFocusType.TargetTransform, CameraFocusType.PlayerAndTransform)]
        public Transform targetFocusTransform;

        [ConditionalField(nameof(focusType), false, CameraFocusType.PlayerAndTransform)]
        [Range(0f, 1f)] public float focusWeight = 0.5f;

        [Tooltip("Bölgenin transform'una göre yerel dikdörtgen; bölge taşınınca çerçeve de taşınır.")]
        [ConditionalField(nameof(focusType), false, CameraFocusType.FrameArea)]
        public Rect frame = new Rect(-10f, -6f, 20f, 12f);

        [Tooltip("Çerçevenin kenarına eklenen pay (dünya birimi).")]
        [ConditionalField(nameof(focusType), false, CameraFocusType.FrameArea)]
        [Min(0f)] public float framePadding = 1f;

        [Tooltip("Çerçeveye giriş ve çıkışta kameranın yerine varma süresi (sn). Çıkışta bırakılan bölgenin değeri kullanılır.")]
        [ConditionalField(nameof(focusType), false, CameraFocusType.FrameArea)]
        [Min(0.05f)] public float blendTime = 0.35f;

        [Header("Zoom Settings")]
        [Tooltip("FrameArea'da açıksa otomatik boy yerine elle girilen cameraSize kullanılır.")]
        public bool overrideZoom;

        [ConditionalField(nameof(overrideZoom))]
        public float cameraSize = 8f;

        [Header("Follow Settings")]
        public bool overrideFollowSpeed;

        [ConditionalField(nameof(overrideFollowSpeed))]
        public float followLerpSpeed = 8f;

        public bool overrideLookAhead;

        [ConditionalField(nameof(overrideLookAhead))]
        public float lookAheadMultiplier = 0.15f;
    }
}
