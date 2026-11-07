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
        /// <summary>Hayaletin bütün sprite'ları bu kadar geri alınır: kendi içindeki sıra korunur, hepsi gerçek oyuncunun arkasında kalır.</summary>
        public const int SortingOrderOffset = 10;
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
        private GhostEyes _eyes;
        private GhostArms _arms;
        private GhostDashFx _dashFx;
        private bool _hasPose;
        private bool _wasShown;

        /// <summary>Bu kadardan büyük sıçrama ışınlanmadır (ölüm/respawn, ceza): iz temizlenir.</summary>
        private const float TrailClearDistance = 3f;

        public int ActiveAfterimageCount => _dashFx?.ActiveCount ?? 0;

        public static GhostView CreateFrom(SpriteRenderer bodySource, Animator animatorSource, LineRenderer ropeSource,
            GhostEyeSource eyesSource = null, GhostArmSource armsSource = null, GhostDashSource dashSource = null)
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
            view._body.sortingOrder = bodySource.sortingOrder - SortingOrderOffset; // gerçek oyuncunun arkasında
            view._body.color = Tint;

            view._animator = bodyGo.AddComponent<Animator>();
            if (animatorSource != null) view._animator.runtimeAnimatorController = animatorSource.runtimeAnimatorController;

            if (eyesSource?.Root != null)
                view._eyes = GhostEyes.Create(eyesSource, root.transform, bodyGo.transform, bodySource.transform, Tint, SortingOrderOffset);

            if (armsSource != null)
                view._arms = GhostArms.Create(armsSource, root.transform, Tint, SortingOrderOffset);
            if (dashSource != null)
                view._dashFx = GhostDashFx.Create(dashSource, root.transform, view._body, Tint, SortingOrderOffset);

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
            if (_hasPose && Vector2.Distance(_pose.Position, pose.Position) > TrailClearDistance) _dashFx?.ClearTrail();
            _hasPose = true;
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
            bool shown = ShouldShow;
            if (shown && !_wasShown) _dashFx?.ClearTrail(); // yeniden belirince eski konumdan iz çekilmesin
            _wasShown = shown;
            _body.enabled = shown;
            _eyes?.SetVisible(shown);
            _arms?.Update(_pose.LeftAnchor, _pose.RightAnchor, shown);
            UpdateRopes();
        }

        /// <summary>Gözleri pozla günceller; LateUpdate'te, animator EyePosition'ı oynattıktan sonra çağrılır.</summary>
        public void TickEyes(float deltaTime) => _eyes?.Tick(_pose, deltaTime);

        /// <summary>Dash kopyalarını ve izi ilerletir (LateUpdate'te; testler doğrudan çağırır).</summary>
        public void TickEffects(float deltaTime) => _dashFx?.Tick(_pose, ShouldShow, _body, deltaTime);

        private void LateUpdate()
        {
            TickEyes(Time.deltaTime);
            TickEffects(Time.deltaTime);
        }

        private void OnDestroy() => _dashFx?.Dispose();

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
            UpdateRope(_leftRope, _pose.LeftAnchor, left: true);
            UpdateRope(_rightRope, _pose.RightAnchor, left: false);
        }

        private void UpdateRope(LineRenderer rope, Vector2? anchor, bool left)
        {
            rope.enabled = ShouldShow && anchor.HasValue;
            if (!anchor.HasValue) return;
            Vector2 start = _arms != null ? _arms.RopeStart(left, anchor, _pose.Position) : _pose.Position;
            rope.SetPosition(0, new Vector3(start.x, start.y, 0f));
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
