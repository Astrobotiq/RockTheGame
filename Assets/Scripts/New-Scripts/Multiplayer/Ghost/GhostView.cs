using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Rakibin hayaleti: yarı saydam gövde + iki düz kanca ipi. Görsel çalışırken oyuncunun kendi sprite, animator
    /// ve ip kaynaklarından kurulur (karakter değişince hayalet de güncel kalır). Collider ve oyuncu script'i yok.
    /// Sahne kökünde durur; hiçbir Room'un altında olmadığı için oda uyutulunca kapanmaz.
    /// </summary>
    public sealed class GhostView : MonoBehaviour
    {
        public const string ObjectName = "[MP Ghost]";
        private static readonly Color Tint = new Color(0.55f, 0.85f, 1f, 0.55f);

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsSwingingHash = Animator.StringToHash("IsSwinging");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int IsWallClimbingHash = Animator.StringToHash("IsWallClimbing");
        private static readonly int IsWallSlidingHash = Animator.StringToHash("IsWallSliding");
        private static readonly int DashTriggerHash = Animator.StringToHash("DashTrigger");

        private SpriteRenderer _body;
        private Animator _animator;
        private LineRenderer _leftRope;
        private LineRenderer _rightRope;
        private bool _visible;
        private byte? _lastDashCount;
        private GhostPose _pose;

        public static GhostView CreateFrom(SpriteRenderer bodySource, Animator animatorSource, LineRenderer ropeSource)
        {
            var root = new GameObject(ObjectName);
            var view = root.AddComponent<GhostView>();

            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(root.transform, false);
            bodyGo.transform.localPosition = bodySource.transform.localPosition;
            bodyGo.transform.localRotation = bodySource.transform.localRotation;
            bodyGo.transform.localScale = bodySource.transform.localScale;

            view._body = bodyGo.AddComponent<SpriteRenderer>();
            view._body.sprite = bodySource.sprite;
            view._body.sharedMaterial = bodySource.sharedMaterial;
            view._body.sortingLayerID = bodySource.sortingLayerID;
            view._body.sortingOrder = bodySource.sortingOrder - 1; // gerçek oyuncunun arkasında
            view._body.color = Tint;

            view._animator = bodyGo.AddComponent<Animator>();
            if (animatorSource != null) view._animator.runtimeAnimatorController = animatorSource.runtimeAnimatorController;

            view._leftRope = CreateRope(root.transform, "LeftRope", ropeSource, view._body);
            view._rightRope = CreateRope(root.transform, "RightRope", ropeSource, view._body);

            view.SetVisible(false);
            return view;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
            UpdateRenderers();
        }

        public void Apply(GhostPose pose)
        {
            _pose = pose;
            transform.position = new Vector3(pose.Position.x, pose.Position.y, transform.position.z);
            _body.flipX = !pose.FacingRight;
            ApplyAnimator(pose);
            UpdateRenderers();
        }

        // Rakip ölüyken (IsHidden) hayalet aynı odada olsa da görünmez.
        private bool ShouldShow => _visible && !_pose.IsHidden;

        private void UpdateRenderers()
        {
            _body.enabled = ShouldShow;
            UpdateRopes();
        }

        private void ApplyAnimator(GhostPose pose)
        {
            if (_animator.runtimeAnimatorController == null || !_animator.isActiveAndEnabled) return;

            _animator.SetFloat(SpeedHash, pose.Speed);
            _animator.SetBool(IsGroundedHash, pose.IsGrounded);
            _animator.SetFloat(VerticalVelocityHash, pose.VerticalVelocity);
            _animator.SetBool(IsSwingingHash, pose.IsSwinging);
            _animator.SetBool(IsDashingHash, pose.IsDashing);
            _animator.SetBool(IsWallClimbingHash, pose.IsWallClimbing);
            _animator.SetBool(IsWallSlidingHash, pose.IsWallSliding);

            // Dash sayacı değiştiyse dash başladı (kayıp paket olsa da sayaç farkı görülür).
            if (_lastDashCount.HasValue && pose.DashCount != _lastDashCount.Value) _animator.SetTrigger(DashTriggerHash);
            _lastDashCount = pose.DashCount;
        }

        private void UpdateRopes()
        {
            UpdateRope(_leftRope, _pose.LeftAnchor);
            UpdateRope(_rightRope, _pose.RightAnchor);
        }

        private void UpdateRope(LineRenderer rope, Vector2? anchor)
        {
            rope.enabled = ShouldShow && anchor.HasValue;
            if (!anchor.HasValue) return;
            rope.SetPosition(0, new Vector3(_pose.Position.x, _pose.Position.y, 0f));
            rope.SetPosition(1, new Vector3(anchor.Value.x, anchor.Value.y, 0f));
        }

        private static LineRenderer CreateRope(Transform parent, string name, LineRenderer source, SpriteRenderer body)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rope = go.AddComponent<LineRenderer>();
            rope.useWorldSpace = true;
            rope.positionCount = 2;
            if (source != null)
            {
                rope.sharedMaterial = source.sharedMaterial;
                rope.widthMultiplier = source.widthMultiplier;
                rope.widthCurve = source.widthCurve;
            }
            rope.startColor = Tint;
            rope.endColor = Tint;
            rope.sortingLayerID = body.sortingLayerID;
            rope.sortingOrder = body.sortingOrder - 1;
            rope.enabled = false;
            return rope;
        }
    }
}
