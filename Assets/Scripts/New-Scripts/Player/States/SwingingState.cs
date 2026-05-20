using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin kanca ile sarkaç hareketini (pendulum) yonetir. 
    /// Yerden geliyorsa once kancaya dogru kucuk bir sicrama (hop) yapar.
    /// </summary>
    public class SwingingState : IPlayerState
    {
        private readonly PlayerController _context;
        private readonly PlayerStatsSO _stats;
        private readonly ActiveArm _swingingArm;
        private readonly bool _wasGrounded;

        private Vector2 _anchorPoint;
        private float _ropeLength;
        private Vector2 _currentVelocity;

        // Hop (Sıçrama) aşaması değişkenleri
        private bool _isHopping;
        private float _hopTimer;

        public SwingingState(PlayerController context, ActiveArm swingingArm, bool wasGrounded = false)
        {
            _context = context;
            _stats = context.Stats;
            _swingingArm = swingingArm;
            _wasGrounded = wasGrounded;
        }

        public void EnterState()
        {
            if (!TrySetAnchorPoint())
            {
                _context.TransitionToState(new AirborneState(_context, _context.Velocity));
                return;
            }

            _context.ResetDash();
            _context.ColorController.ResetBodyColor();

            if (_wasGrounded)
            {
                StartHopPhase();
            }
            else
            {
                InitializePendulum();
            }
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            if (_isHopping)
            {
                ApplyHopPhysics();
            }
            else
            {
                ApplyArcadePendulum();
            }
        }

        public void ExitState()
        {
        }

        // --- HOP (SIÇRAMA) AŞAMASI METOTLARI ---

        private void StartHopPhase()
        {
            _isHopping = true;
            _hopTimer = _stats.SwingHopDuration;

            Vector2 playerPos = _context.PlayerRigidbody.position;
            Vector2 directionToAnchor = (_anchorPoint - playerPos).normalized;

            // Anchor'ın olduğu yöne (sağ/sol) doğru yatay, ve yukarı doğru dikey bir kuvvet.
            float dirX = Mathf.Sign(directionToAnchor.x);
            _currentVelocity = new Vector2(dirX * _stats.SwingHopForwardSpeed, _stats.SwingHopUpwardSpeed);

            _context.Velocity = _currentVelocity;
        }

        private void ApplyHopPhysics()
        {
            _hopTimer -= Time.fixedDeltaTime;

            // Sıçrama sırasında standart yerçekimi uygula (AirborneState'deki gibi)
            _currentVelocity.y += _stats.Gravity * Time.fixedDeltaTime;
            _context.Velocity = _currentVelocity;

            // Süre dolduğunda sarkaç aşamasına geç
            if (_hopTimer <= 0f)
            {
                _isHopping = false;
                InitializePendulum();
            }
        }

        // --- PENDULUM (SARKAÇ) AŞAMASI METOTLARI ---

        private void InitializePendulum()
        {
            _ropeLength = Vector2.Distance(_context.PlayerRigidbody.position, _anchorPoint);
            _currentVelocity = _context.Velocity;
        }

        private void ApplyArcadePendulum()
        {
            float playerInputX = _swingingArm == ActiveArm.Left
                ? _context.Input.LeftStick.x
                : _context.Input.RightStick.x;

            // _stats.Gravity eksi bir değer olduğu için toplama işlemi yapıyoruz.
            _currentVelocity.y += _stats.Gravity * Time.fixedDeltaTime;

            Vector2 playerPos = _context.PlayerRigidbody.position;
            Vector2 directionToAnchor = (_anchorPoint - playerPos).normalized;
            Vector2 tangent = new Vector2(directionToAnchor.y, -directionToAnchor.x);

            float speedAlongTangent = Vector2.Dot(_currentVelocity, tangent);
            float inputForce = playerInputX * _stats.SwingForceMultiplier * Time.fixedDeltaTime;

            if (Mathf.Abs(speedAlongTangent + inputForce) < _stats.MaxSwingSpeed)
            {
                speedAlongTangent += inputForce;
            }

            _currentVelocity = tangent * speedAlongTangent;

            Vector2 nextPosition = _anchorPoint - directionToAnchor * _ropeLength;
            Vector2 movementDelta = nextPosition - playerPos;

            if (CheckSweepCollision(playerPos, movementDelta)) return;

            _context.PlayerRigidbody.position = nextPosition;
            _context.Velocity = _currentVelocity;
        }

        // --- YARDIMCI METOTLAR (Değişmedi) ---

        private bool TrySetAnchorPoint()
        {
            if (_swingingArm == ActiveArm.Left && _context.LeftAnchor.HasValue)
            {
                _anchorPoint = _context.LeftAnchor.Value;
                return true;
            }

            if (_swingingArm == ActiveArm.Right && _context.RightAnchor.HasValue)
            {
                _anchorPoint = _context.RightAnchor.Value;
                return true;
            }

            return false;
        }

        private void HandleArmRouting()
        {
            _context.LeftArm.UpdateArmRotation(_context.Input.LeftStick);
            _context.RightArm.UpdateArmRotation(_context.Input.RightStick);

            Vector2 directionToAnchor = (_anchorPoint - _context.PlayerRigidbody.position).normalized;

            if (_swingingArm == ActiveArm.Left)
                _context.LeftArm.UpdateArmRotation(directionToAnchor);
            else
                _context.RightArm.UpdateArmRotation(directionToAnchor);
        }

        private void CheckInputTransitions()
        {
            bool activeTriggerHeld = _swingingArm == ActiveArm.Left
                ? _context.Input.IsLeftTriggerHeld
                : _context.Input.IsRightTriggerHeld;

            if (!activeTriggerHeld)
            {
                ReleaseAnchor();
                
                if (_context.UseJumpGravity)
                {
                    _context.TransitionToState(new AirborneState(_context, _currentVelocity, isFromSwing: true));
                    return;
                }
                
                _context.TransitionToState(new AirborneState(_context, _currentVelocity, isJumping: true));
                return;
            }

            CheckOppositeGrappleCast();
        }

        private void CheckOppositeGrappleCast()
        {
            bool oppositeTriggerHeld = _swingingArm == ActiveArm.Left
                ? _context.Input.IsRightTriggerHeld
                : _context.Input.IsLeftTriggerHeld;

            if (!oppositeTriggerHeld) return;

            Vector2 aimStick = _swingingArm == ActiveArm.Left ? _context.Input.RightStick : _context.Input.LeftStick;

            if (_context.TryCastGrapple(aimStick, out Vector2 hitPoint))
            {
                if (_swingingArm == ActiveArm.Left) _context.RightAnchor = hitPoint;
                else _context.LeftAnchor = hitPoint;

                EvaluateDualGrappleTransition();
            }
        }

        private void EvaluateDualGrappleTransition()
        {
            if (_context.CanSlingshot && _context.CheckNodeCoincidence())
            {
                _context.TransitionToState(new SlingshotState(_context));
            }
            else
            {
                _context.TransitionToState(new DualSwingingState(_context));
            }
        }

        private void ReleaseAnchor()
        {
            if (_swingingArm == ActiveArm.Left) _context.LeftAnchor = null;
            if (_swingingArm == ActiveArm.Right) _context.RightAnchor = null;
        }

        private void BreakGrapple()
        {
            _context.LeftAnchor = null;
            _context.RightAnchor = null;
            _context.TransitionToState(new AirborneState(_context, _currentVelocity));
        }

        private bool CheckSweepCollision(Vector2 playerPos, Vector2 movementDelta)
        {
            RaycastHit2D hit = Physics2D.CircleCast(playerPos, _stats.SwingCollisionRadius, movementDelta.normalized,
                movementDelta.magnitude, _context.GroundLayerMask);

            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > _stats.MaxWallAngle)
                {
                    BreakGrapple();
                }
                else
                {
                    _context.LeftAnchor = null;
                    _context.RightAnchor = null;
                    _context.TransitionToState(new GroundedState(_context));
                }

                return true;
            }

            return false;
        }
    }
}