using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Hayaletin gözlerinin kopyalanacağı kaynak: oyuncunun göz kökü, gözleri, animasyonlu EyePosition noktası ve
    /// PlayerEyesController ayarları. Varsayılan konumlar verilmezse kaynağın şu anki yerel konumu kullanılır.
    /// </summary>
    public sealed class GhostEyeSource
    {
        public sealed class Eye
        {
            public Transform Container;
            public SpriteRenderer Background;
            public SpriteRenderer Pupil;
            public SpriteRenderer Blink;
            public Vector3? DefaultContainerLocalPosition;
            public Vector3? DefaultPupilLocalPosition;
        }

        public Transform Root;
        public Vector3? DefaultRootLocalPosition;
        public Transform AnimatedAnchor;
        public Vector3? DefaultAnchorLocalPosition;
        public readonly List<Eye> Eyes = new List<Eye>();
        public float VelocitySensitivity = 0.01f;
        public float MaxPupilOffset = 0.1f;
        public float PupilSmoothSpeed = 10f;
        public bool FlipParentTransform = true;
    }

    /// <summary>
    /// Hayaletin gözleri: PlayerEyesController'ın yön çevirme, EyePosition'a göre dikey kayma ve hıza göre göz bebeği
    /// kaydırma davranışının aynısını rakibin pozuyla yapar. Göz kırpma yok (gözler açık kalır).
    /// </summary>
    internal sealed class GhostEyes
    {
        private sealed class EyeCopy
        {
            public Transform Container;
            public Vector3 DefaultContainerPosition;
            public Transform Pupil;
            public Vector3 DefaultPupilPosition;
        }

        private readonly List<EyeCopy> _eyes = new List<EyeCopy>();
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private Transform _root;
        private Vector3 _defaultRootPosition;
        private Transform _anchor;
        private float _defaultAnchorY;
        private float _sensitivity;
        private float _maxPupilOffset;
        private float _smoothSpeed;
        private bool _flipParent;
        private Vector2 _pupilOffset;

        public static GhostEyes Create(GhostEyeSource source, Transform ghostRoot, Transform ghostBody, Transform bodySource,
            Color tint, int sortingOrderOffset)
        {
            var eyes = new GhostEyes
            {
                _sensitivity = source.VelocitySensitivity,
                _maxPupilOffset = source.MaxPupilOffset,
                _smoothSpeed = source.PupilSmoothSpeed,
                _flipParent = source.FlipParentTransform,
                _defaultRootPosition = source.DefaultRootLocalPosition ?? source.Root.localPosition
            };

            eyes._root = new GameObject(source.Root.name).transform;
            eyes._root.SetParent(ghostRoot, false);
            eyes._root.localPosition = eyes._defaultRootPosition;

            foreach (var eye in source.Eyes)
            {
                if (eye?.Container == null) continue;
                var container = new GameObject(eye.Container.name).transform;
                container.SetParent(eyes._root, false);
                var defaultContainer = eye.DefaultContainerLocalPosition ?? eye.Container.localPosition;
                container.localPosition = defaultContainer;
                container.localRotation = eye.Container.localRotation;
                container.localScale = eye.Container.localScale;

                eyes.CopyRenderer(eye.Background, container, tint, sortingOrderOffset);
                var pupil = eyes.CopyRenderer(eye.Pupil, container, tint, sortingOrderOffset);
                if (pupil != null && eye.DefaultPupilLocalPosition.HasValue)
                    pupil.transform.localPosition = eye.DefaultPupilLocalPosition.Value;
                eyes.CopyRenderer(eye.Blink, container, tint, sortingOrderOffset);

                eyes._eyes.Add(new EyeCopy
                {
                    Container = container,
                    DefaultContainerPosition = defaultContainer,
                    Pupil = pupil != null ? pupil.transform : null,
                    DefaultPupilPosition = pupil != null ? pupil.transform.localPosition : Vector3.zero
                });
            }

            // Gövde animasyonu EyePosition'ı oynatır (gözler zıplamada aşağı yukarı gider); hayalet gövdesinde de aynı adla
            // bulunsun ki aynı animator onu sürsün.
            if (source.AnimatedAnchor != null && bodySource != null && source.AnimatedAnchor.parent == bodySource)
            {
                eyes._anchor = new GameObject(source.AnimatedAnchor.name).transform;
                eyes._anchor.SetParent(ghostBody, false);
                var defaultAnchor = source.DefaultAnchorLocalPosition ?? source.AnimatedAnchor.localPosition;
                eyes._anchor.localPosition = defaultAnchor;
                eyes._defaultAnchorY = defaultAnchor.y;
            }

            return eyes;
        }

        public void SetVisible(bool visible)
        {
            foreach (var r in _renderers) r.enabled = visible;
        }

        public void Tick(GhostPose pose, float deltaTime)
        {
            bool flipped = !pose.FacingRight;
            float offsetY = _anchor != null ? _anchor.localPosition.y - _defaultAnchorY : 0f;

            if (_flipParent)
            {
                var position = _defaultRootPosition;
                position.y += offsetY;
                if (flipped) position.x = -_defaultRootPosition.x;
                _root.localPosition = position;
                _root.localScale = new Vector3(flipped ? -1f : 1f, 1f, 1f);
            }
            else
            {
                foreach (var eye in _eyes)
                {
                    var position = eye.DefaultContainerPosition;
                    position.y += offsetY;
                    if (flipped) position.x = -eye.DefaultContainerPosition.x;
                    eye.Container.localPosition = position;
                    var scale = eye.Container.localScale;
                    eye.Container.localScale = new Vector3(flipped ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x), scale.y, scale.z);
                }
            }

            // Rakibin yatay hızı: Speed mutlak değer, yönü bakış yönünden gelir.
            var velocity = new Vector2(pose.FacingRight ? pose.Speed : -pose.Speed, pose.VerticalVelocity);
            var target = Vector2.ClampMagnitude(velocity * _sensitivity, _maxPupilOffset);
            if (flipped) target.x *= -1f;
            _pupilOffset = Vector2.Lerp(_pupilOffset, target, deltaTime * _smoothSpeed);

            foreach (var eye in _eyes)
                if (eye.Pupil != null) eye.Pupil.localPosition = eye.DefaultPupilPosition + (Vector3)_pupilOffset;
        }

        private SpriteRenderer CopyRenderer(SpriteRenderer source, Transform parent, Color tint, int sortingOrderOffset)
        {
            if (source == null) return null;
            var copy = new GameObject(source.name).AddComponent<SpriteRenderer>();
            copy.transform.SetParent(parent, false);
            copy.transform.localPosition = source.transform.localPosition;
            copy.transform.localRotation = source.transform.localRotation;
            copy.transform.localScale = source.transform.localScale;
            copy.sprite = source.sprite;
            copy.sharedMaterial = source.sharedMaterial;
            copy.sortingLayerID = source.sortingLayerID;
            copy.sortingOrder = source.sortingOrder - sortingOrderOffset;
            copy.color = new Color(source.color.r * tint.r, source.color.g * tint.g, source.color.b * tint.b, tint.a);
            copy.enabled = false;
            _renderers.Add(copy);
            return copy;
        }
    }
}
