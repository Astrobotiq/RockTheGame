using UnityEngine;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;

namespace New_Scripts.Player
{
    public enum CameraFocusType
    {
        Player,
        StaticPosition,
        TargetTransform,
        PlayerAndTransform
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

        [Header("Zoom Settings")]
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
