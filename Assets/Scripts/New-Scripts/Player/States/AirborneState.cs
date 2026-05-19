using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin havadaki fiziksel davranislarini, durum gecislerini ve ozel yercekimi hesaplamalarini yoneten durum sinifi.
    /// </summary>
    public class AirborneState : IPlayerState
    {
        private readonly PlayerController _context;
        private readonly PlayerStatsSO _stats;

        private Vector2 _currentVelocity;
        private float _grappleLockoutTimer;
        private float _wallClimbLockoutTimer;
        private float _coyoteTimer;
        private float _horizontalInputLockoutTimer;
        private bool _isJumping;

        public AirborneState(PlayerController context, Vector2 inheritedVelocity, bool isJumping = false,
            float grappleLockout = 0f, float wallClimbLockout = 0f, float coyote = 0f, float horizontalLockout = 0f)
        {
            _context = context;
            _stats = context.Stats;
            _currentVelocity = inheritedVelocity;
            _isJumping = isJumping;
            _grappleLockoutTimer = grappleLockout;
            _wallClimbLockoutTimer = wallClimbLockout;
            _coyoteTimer = coyote;
            _horizontalInputLockoutTimer = horizontalLockout;
        }

        public void EnterState()
        {
            _context.Velocity = _currentVelocity;
        }

        public void UpdateState()
        {
            DecrementTimers();
            HandleArmRouting();
            HandleCoyoteJump();
            HandleTransitions();
        }

        public void FixedUpdateState()
        {
            if (_context.IsGrounded && _currentVelocity.y <= 0.01f)
            {
                _context.TransitionToState(new GroundedState(_context));
                return;
            }

            ApplyCustomGravity();
            ApplyAirMovement();
            HandleVerticalObstructions();

            _context.Velocity = _currentVelocity;
        }

        public void ExitState()
        {
        }

        private void DecrementTimers()
        {
            float dt = Time.deltaTime;
            if (_coyoteTimer > 0f) _coyoteTimer -= dt;
            if (_horizontalInputLockoutTimer > 0f) _horizontalInputLockoutTimer -= dt;
            if (_grappleLockoutTimer > 0f) _grappleLockoutTimer -= dt;
            if (_wallClimbLockoutTimer > 0f) _wallClimbLockoutTimer -= dt;
        }

        private void HandleArmRouting()
        {
            _context.LeftArm.UpdateArmRotation(_context.Input.LeftStick);
            _context.RightArm.UpdateArmRotation(_context.Input.RightStick);
        }

        private void HandleCoyoteJump()
        {
            if (_coyoteTimer > 0f && _context.JumpBufferTimer > 0f)
            {
                _context.ConsumeJumpBuffer();
                _coyoteTimer = 0f;
                _isJumping = true;
                _currentVelocity.y = _stats.JumpVelocity;
            }
        }

        private void HandleTransitions()
        {
            if (_context.Input.IsDashPressed && _context.HasDashCharge)
            {
                _context.TransitionToState(new DashState(_context, _context.Input.LeftStick));
                return;
            }

            if (_grappleLockoutTimer <= 0f)
            {
                ProcessGrappleInput();
            }

            if (_wallClimbLockoutTimer <= 0f)
            {
                ProcessWallInteraction();
            }
        }

        private void ApplyCustomGravity()
        {
            float gravityMultiplier = 1f;

            if (_isJumping)
            {
                if (_currentVelocity.y < 0f)
                {
                    gravityMultiplier = _stats.FallGravityMultiplier;
                }
                else if (_currentVelocity.y > 0f && !_context.Input.IsJumpHeld)
                {
                    gravityMultiplier = _stats.JumpEndEarlyGravityMultiplier;
                }
                else if (Mathf.Abs(_currentVelocity.y) < _stats.ApexThreshold)
                {
                    gravityMultiplier = _stats.ApexHangGravityMultiplier;
                }
            }

            float gravityStep = _stats.Gravity * gravityMultiplier * Time.fixedDeltaTime;
            _currentVelocity.y += gravityStep;
            _currentVelocity.y = Mathf.Max(_currentVelocity.y, _stats.TerminalVelocity);
        }

        private void ApplyAirMovement()
        {
            if (_horizontalInputLockoutTimer > 0f) return;

            float moveInput = _context.Input.LeftStick.x;
            float targetX = moveInput * _stats.MoveSpeed * _stats.AirControlMultiplier;
            float accel = Mathf.Abs(moveInput) < 0.05f ? _stats.AirDrag : _stats.AirAcceleration;

            _currentVelocity.x = Mathf.MoveTowards(_currentVelocity.x, targetX, accel * Time.fixedDeltaTime);
        }

        private void HandleVerticalObstructions()
        {
            if (_currentVelocity.y > 0f && _context.IsTouchingCeiling)
            {
                _currentVelocity.y = 0f;
            }
        }

        private void ProcessGrappleInput()
        {
            if (_context.Input.IsLeftTriggerHeld && !_context.LeftAnchor.HasValue)
            {
                AttemptGrappleTransition(_context.Input.LeftStick, ActiveArm.Left);
            }
            else if (_context.Input.IsRightTriggerHeld && !_context.RightAnchor.HasValue)
            {
                AttemptGrappleTransition(_context.Input.RightStick, ActiveArm.Right);
            }
        }

        private void AttemptGrappleTransition(Vector2 direction, ActiveArm arm)
        {
            if (_context.TryCastGrapple(direction, out Vector2 hitPoint))
            {
                if (arm == ActiveArm.Left)
                {
                    _context.LeftAnchor = hitPoint;
                }
                else
                {
                    _context.RightAnchor = hitPoint;
                }

                EvaluateFinalGrappleState();
            }
        }

        private void EvaluateFinalGrappleState()
        {
            if (_context.LeftAnchor.HasValue && _context.RightAnchor.HasValue)
            {
                if (_context.CheckNodeCoincidence() && _context.CanSlingshot)
                {
                    _context.TransitionToState(new SlingshotState(_context));
                }
                else
                {
                    _context.TransitionToState(new DualSwingingState(_context));
                }
            }
            else if (_context.LeftAnchor.HasValue)
            {
                _context.TransitionToState(new SwingingState(_context, ActiveArm.Left));
            }
            else if (_context.RightAnchor.HasValue)
            {
                _context.TransitionToState(new SwingingState(_context, ActiveArm.Right));
            }
        }

        private void ProcessWallInteraction()
        {
            if (_context.CanWallClimb)
            {
                if (_context.Input.IsLeftBumperHeld && _context.IsTouchingLeftWall)
                {
                    _context.TransitionToState(new WallClimbingState(_context, -1));
                    return;
                }

                if (_context.Input.IsRightBumperHeld && _context.IsTouchingRightWall)
                {
                    _context.TransitionToState(new WallClimbingState(_context, 1));
                    return;
                }
            }

            if (_currentVelocity.y < 0f && _context.CurrentWallSlideTime > 0f)
            {
                if (_context.IsTouchingLeftWall && _context.Input.LeftStick.x < -0.1f)
                {
                    _context.TransitionToState(new WallSlidingState(_context, -1));
                }
                else if (_context.IsTouchingRightWall && _context.Input.LeftStick.x > 0.1f)
                {
                    _context.TransitionToState(new WallSlidingState(_context, 1));
                }
            }
        }
    }
}