using New_Scripts.Platform;
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
        private bool _bypassJumpGravity;
        private bool _bypassApexHangGravity;
        private bool _bypassEndEarlyGravity;
        private float _endEarlyGravityMultiplier;
        // Taşıyan yüzeyden devralınan yatay hız. Hava sürtünmesi/ivmesi yalnızca oyuncunun kendi hızına
        // işler (yüzeyin referans çerçevesi); aksi halde 0.2 sn içinde sönüp duvar kaçardı.
        private float _carriedVelocityX;
        private WallSpeedRetention _wallSpeedRetention;

        /// <param name="carriedVelocity">Taşıyan yüzeyden ayrılırken aktarılan hız (<see cref="SurfaceCarry"/>).
        /// Y bileşeni normal hıza eklenir (yerçekimi sönümler), X bileşeni referans çerçevesi olarak taşınır.</param>
        public AirborneState(PlayerController context, Vector2 inheritedVelocity, bool isJumping = false,
            float grappleLockout = 0f, float wallClimbLockout = 0f, float coyote = 0f, float horizontalLockout = 0f,
            bool bypassJumpGravity = false, bool bypassApexHangGravity = false, bool bypassEndEarlyGravity = false
            , float endEarlyGravityMultiplier = 1f, Vector2 carriedVelocity = default)
        {
            _context = context;
            _stats = context.Stats;
            _currentVelocity = inheritedVelocity + new Vector2(0f, carriedVelocity.y);
            _carriedVelocityX = carriedVelocity.x;
            _isJumping = isJumping;
            _grappleLockoutTimer = grappleLockout;
            _wallClimbLockoutTimer = wallClimbLockout;
            _coyoteTimer = coyote;
            _horizontalInputLockoutTimer = horizontalLockout;
            _bypassJumpGravity = bypassJumpGravity;
            _bypassApexHangGravity = bypassApexHangGravity;
            _bypassEndEarlyGravity = bypassEndEarlyGravity;
            _endEarlyGravityMultiplier = endEarlyGravityMultiplier;

            if (_endEarlyGravityMultiplier < 0f)
                _endEarlyGravityMultiplier = 0f;
            else if (_endEarlyGravityMultiplier > 1f)
                _endEarlyGravityMultiplier = 1f;
        }

        public void EnterState()
        {
            _context.Velocity = _currentVelocity + new Vector2(_carriedVelocityX, 0f);
        }

        public void UpdateState()
        {
            HandleArmRouting();
            HandleCoyoteJump();
            if (HandleAirWallJump()) return;
            HandleTransitions();
        }

        // Celeste gibi (PF-5): coyote once (HandleCoyoteJump buffer'i tuketir), sonra duvar yakinsa duvar ziplamasi;
        // temas, stick yonu ve dusus sart degil. Iki yanda duvar varsa once sag (Celeste WallJumpCheck(1)).
        private bool HandleAirWallJump()
        {
            if (_context.JumpBufferTimer <= 0f) return false;

            float distance = _stats.WallJumpCheckDistance;
            int wallDirection = _context.IsWallWithin(1, distance) ? 1
                : _context.IsWallWithin(-1, distance) ? -1 : 0;
            if (wallDirection == 0) return false;

            WallSlidingState.PerformWallJump(_context, wallDirection);
            return true;
        }

        public void FixedUpdateState()
        {
            DecrementTimers();

            if (_context.IsGrounded && _currentVelocity.y <= 0.01f)
            {
                _context.TransitionToState(new GroundedState(_context));
                return;
            }

            HandleHorizontalObstructions();
            ApplyCustomGravity();
            ApplyAirMovement();
            HandleVerticalObstructions();

            _context.Velocity = _currentVelocity + new Vector2(_carriedVelocityX, 0f);
        }

        public void ExitState()
        {
        }

        // Fizik adiminda sayilir (PF-6): sureler kare hizindan bagimsiz, her zaman ayni adim sayisi.
        private void DecrementTimers()
        {
            float dt = Time.fixedDeltaTime;
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
                if (_context.Audio != null) _context.Audio.PlayJump();
                if (_context.VibrationSettings != null && HapticManager.Instance != null)
                {
                    HapticManager.Instance.Vibrate(_context.VibrationSettings.Jump);
                }
                
                Vector2 jumpVelocityVector = new Vector2(_currentVelocity.x, _stats.JumpVelocity);
                
                IMovingSurface lastSurface = _context.PhysicsHandler.LastMovingSurface;
                if (lastSurface != null && lastSurface.JumpBoostMultiplier > 0f)
                {
                    // Boost taban hızın yerine geçer (platforms.md): devralınan hız tekrar eklenmesin.
                    _carriedVelocityX = 0f;
                    jumpVelocityVector += lastSurface.SurfaceVelocity * lastSurface.JumpBoostMultiplier;
                    _bypassApexHangGravity = true;
                }
                
                _currentVelocity = jumpVelocityVector;
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
                if (_bypassJumpGravity)
                {
                    if (_currentVelocity.y < 0f)
                    {
                        gravityMultiplier = _stats.FallGravityMultiplier;
                    }
                    else if (_currentVelocity.y > 0f)
                    {
                        gravityMultiplier = _stats.JumpEndEarlyGravityMultiplier * _endEarlyGravityMultiplier;
                    }
                }
                else
                {
                    if (_currentVelocity.y < 0f)
                    {
                        gravityMultiplier = _stats.FallGravityMultiplier;
                    }
                    else if (!_bypassEndEarlyGravity && _currentVelocity.y > 0f && !_context.Input.IsJumpHeld)
                    {
                        gravityMultiplier = _stats.JumpEndEarlyGravityMultiplier * _endEarlyGravityMultiplier;
                    }
                    else if (!_bypassApexHangGravity && Mathf.Abs(_currentVelocity.y) < _stats.ApexThreshold)
                    {
                        gravityMultiplier = _stats.ApexHangGravityMultiplier;
                    }
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

        // Duvara carpan yatay hiz sifirlanir; duvar WallSpeedRetentionTime icinde biterse geri verilir (PF-4).
        private void HandleHorizontalObstructions()
        {
            _currentVelocity.x = _wallSpeedRetention.Step(_currentVelocity.x, _context.IsTouchingLeftWall,
                _context.IsTouchingRightWall, _stats.WallSpeedRetentionTime, Time.fixedDeltaTime);
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
                if (_context.Input.IsLeftBumperHeld && _context.CanGrabWall(-1))
                {
                    _context.TransitionToState(new WallClimbingState(_context, -1));
                    return;
                }

                if (_context.Input.IsRightBumperHeld && _context.CanGrabWall(1))
                {
                    _context.TransitionToState(new WallClimbingState(_context, 1));
                    return;
                }
            }

            if (_currentVelocity.y < 0f && _context.CurrentWallSlideTime > 0f)
            {
                if (_context.Input.LeftStick.x < -0.1f && _context.CanGrabWall(-1))
                {
                    _context.TransitionToState(new WallSlidingState(_context, -1));
                }
                else if (_context.Input.LeftStick.x > 0.1f && _context.CanGrabWall(1))
                {
                    _context.TransitionToState(new WallSlidingState(_context, 1));
                }
            }
        }
    }
}