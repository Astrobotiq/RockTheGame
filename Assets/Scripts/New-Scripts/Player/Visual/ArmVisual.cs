using New_Scripts.Player.States;
using New_Scripts.UI.Theme;
using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Omuzdan ele giden kolu çizer: boşta yaylı ve gevşek, tutunca node'a uzanır. Yalnızca görsel.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ArmVisual : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private ActiveArm arm;
        [SerializeField] private ArmController armController;
        [SerializeField] private LineRenderer line;
        [Tooltip("Kolun arkasındaki koyu dış çizgi.")]
        [SerializeField] private LineRenderer outlineLine;
        [SerializeField] private Transform hand;
        [SerializeField] private ArmVisualSettingsSO settings;

        private UiSpring _handX, _handY, _midX, _midY, _punch;
        private SpriteRenderer _handRenderer;
        private Collider2D _playerCollider;
        private Vector2 _handPos;
        private Vector2 _extendStart;
        private float _elapsed;
        private bool _wasActive;
        private bool _wasOnWall;
        private float _wallPhase;
        private float _prevBodyY;
        private Transform _prevNode;
        private Vector2? _prevAnchor;
        private Vector3[] _points;

        private void Awake()
        {
            _handX = new UiSpring(settings.HandStiffness, settings.HandDamping);
            _handY = new UiSpring(settings.HandStiffness, settings.HandDamping);
            _midX = new UiSpring(settings.BowStiffness, settings.BowDamping);
            _midY = new UiSpring(settings.BowStiffness, settings.BowDamping);

            line.useWorldSpace = true;
            line.positionCount = settings.Resolution;
            line.widthMultiplier = 1f;
            line.widthCurve = new AnimationCurve(
                new Keyframe(0f, settings.ShoulderWidth), new Keyframe(1f, settings.HandWidth));
            line.startColor = line.endColor = settings.ArmColor;
            _points = new Vector3[settings.Resolution];

            outlineLine.useWorldSpace = true;
            outlineLine.positionCount = settings.Resolution;
            outlineLine.widthMultiplier = 1f;
            outlineLine.widthCurve = new AnimationCurve(
                new Keyframe(0f, settings.ShoulderWidth + settings.OutlineExtra),
                new Keyframe(1f, settings.HandWidth + settings.OutlineExtra));
            outlineLine.startColor = outlineLine.endColor = settings.OutlineColor;

            _punch = new UiSpring(settings.PunchStiffness, settings.PunchDamping);
            _handRenderer = hand.GetComponent<SpriteRenderer>();
            _playerCollider = player.GetComponent<Collider2D>();

            Vector2 shoulder = armController.transform.position;
            SnapTo(shoulder);
        }

        private void SnapTo(Vector2 p)
        {
            _handPos = p;
            _handX.Value = _handX.Target = p.x; _handX.Velocity = 0f;
            _handY.Value = _handY.Target = p.y; _handY.Velocity = 0f;
            _midX.Value = _midX.Target = p.x; _midX.Velocity = 0f;
            _midY.Value = _midY.Target = p.y; _midY.Velocity = 0f;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            Vector2 shoulder = armController.transform.position;
            Vector2? anchor = arm == ActiveArm.Left ? player.LeftAnchor : player.RightAnchor;
            Transform node = arm == ActiveArm.Left ? player.LeftAnchorNode : player.RightAnchorNode;
            float progress = 1f;
            bool dragging = false;
            bool onWall = false;
            bool wallGrip = false;

            if (anchor.HasValue)
            {
                if (ArmShapeMath.IsNewGrab(_wasActive, _prevNode, node, _prevAnchor, anchor.Value))
                {
                    _extendStart = _handPos;
                    _elapsed = 0f;
                    _punch.Kick(settings.GrabPunch);
                }
                _elapsed += dt;
                progress = ArmShapeMath.ExtendProgress(_elapsed, settings.ExtendDuration);
                _handPos = Vector2.Lerp(_extendStart, anchor.Value, progress);
                // Bırakınca el buradan yayla geri döner.
                _handX.Value = _handPos.x; _handX.Velocity = 0f;
                _handY.Value = _handPos.y; _handY.Velocity = 0f;
            }
            else
            {
                if (ArmShapeMath.ShouldSnap(_handPos, shoulder, settings.IdleReach))
                    SnapTo(shoulder);

                Vector2 target = ArmShapeMath.IdleHandTarget(shoulder, armController.AimDirection,
                    player.Velocity, settings.IdleReach, settings.DroopTrail);
                float side = arm == ActiveArm.Left ? -1f : 1f;
                int wallDir = player.PhysicsHandler.ClingingWallDirection;
                float bodyY = player.transform.position.y;
                dragging = armController.AimDirection.sqrMagnitude < 0.01f && player.IsGrounded;
                if (wallDir != 0)
                {
                    // Duvarda eller duvara yapışır: tırmanırken dönüşümlü tutunur, kayarken düz basar.
                    bool climbing = player.CurrentState is WallClimbingState;
                    if (!_wasOnWall) _wallPhase = arm == ActiveArm.Right ? 0.5f : 0f;
                    if (climbing)
                        _wallPhase = ArmShapeMath.WallPhaseStep(_wallPhase, bodyY - _prevBodyY,
                            settings.WallHandLow, settings.WallHandHigh);
                    float rel = climbing
                        ? ArmShapeMath.WallHandOffsetY(_wallPhase, settings.WallHandLow, settings.WallHandHigh)
                        : settings.WallSlideHandY;
                    wallGrip = climbing && ArmShapeMath.IsWallGrip(_wallPhase);
                    onWall = true;
                    target = new Vector2(player.transform.position.x + wallDir * settings.WallHandX, bodyY + rel);
                }
                else if (dragging)
                {
                    // Yerdeyken kol zeminde sürünür, hızın gerisinde kalır.
                    target = ArmShapeMath.GroundDragTarget(shoulder, side, _playerCollider.bounds.min.y,
                        settings.GroundHandHeight, settings.IdleSpread, player.Velocity.x,
                        settings.GroundTrail, settings.IdleReach);
                }
                else if (armController.AimDirection.sqrMagnitude < 0.01f)
                {
                    // Eller zemine gömülmesin: yana açık, omuzdan en fazla IdleDrop kadar aşağıda.
                    target.x += side * settings.IdleSpread;
                    target.y = Mathf.Max(target.y, shoulder.y - settings.IdleDrop);
                }
                _handX.Target = target.x; _handY.Target = target.y;
                _handX.Step(dt); _handY.Step(dt);
                _handPos = new Vector2(_handX.Value, _handY.Value);
            }

            _wasOnWall = onWall;
            _prevBodyY = player.transform.position.y;
            _wasActive = anchor.HasValue;
            _prevNode = node;
            _prevAnchor = anchor;

            Vector2 dir = _handPos - shoulder;
            float len = dir.magnitude;
            Vector2 perp = len < 1e-4f ? Vector2.up : new Vector2(-dir.y, dir.x) / len;
            Vector2 midTarget = (shoulder + _handPos) * 0.5f
                                + perp * ArmShapeMath.BowForLength(len, settings.IdleReach, settings.IdleBow);

            _midX.Target = midTarget.x; _midY.Target = midTarget.y;
            _midX.Step(dt); _midY.Step(dt);
            if (anchor.HasValue)
            {
                // Gerilince düzleşsin.
                _midX.Value = Mathf.Lerp(_midX.Value, midTarget.x, progress);
                _midY.Value = Mathf.Lerp(_midY.Value, midTarget.y, progress);
            }

            var mid = new Vector2(_midX.Value, _midY.Value);
            int n = _points.Length;
            for (int i = 0; i < n; i++)
            {
                Vector2 p = ArmShapeMath.Bezier(shoulder, mid, _handPos, i / (float)(n - 1));
                _points[i] = new Vector3(p.x, p.y, 0f);
            }
            line.SetPositions(_points);
            outlineLine.SetPositions(_points);

            _punch.Step(dt);
            hand.position = new Vector3(_handPos.x, _handPos.y, hand.position.z);
            hand.localScale = Vector3.one * (1f + _punch.Value);
            // Yerde sürünen el düz yatar ve gövdeden dışarı bakar.
            Vector2 handDir = dragging ? new Vector2(Mathf.Sign(_handPos.x - shoulder.x), 0f) : dir;
            // Duvardaki el parmakları yukarı bakar; flipY ile avuç duvara döner.
            if (onWall) handDir = new Vector2(player.PhysicsHandler.ClingingWallDirection * 0.001f, 1f);
            if (handDir.sqrMagnitude > 1e-8f) hand.right = handDir;
            _handRenderer.sprite = anchor.HasValue ? settings.ClosedHand
                : wallGrip ? settings.WallGripHand : settings.OpenHand;
            // Sola bakarken el baş aşağı görünmesin.
            _handRenderer.flipY = handDir.x < 0f;
        }
    }
}
