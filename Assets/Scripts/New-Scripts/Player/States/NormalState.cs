using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Normal hareket (Celeste StNormal): yerde koşu ve havadaki fizik tek durumda. Yerde olmak bir alandır
    /// (<see cref="IsOnGround"/>); yere iniş <see cref="OnLand"/>, yerden ayrılış <see cref="LeaveGround"/> ile olur,
    /// durum değişmez (docs/yer-hava-birlestirme-plani.md Aşama 2). Havaya giriş kuralı <see cref="AirEntry"/> profilidir.
    /// </summary>
    public class NormalState : IPlayerState
    {
        private readonly PlayerController _context;
        private readonly PlayerStatsSO _stats;

        public bool IsOnGround { get; private set; }

        // ---- Hava alanları: her havaya girişte BeginAir sıfırlar ----
        private Vector2 _currentVelocity;
        private float _grappleLockoutTimer;
        private float _wallClimbLockoutTimer;
        private float _coyoteTimer;
        private float _horizontalInputLockoutTimer;
        private bool _isJumping;
        private bool _bypassJumpGravity;
        // Yalnızca boost'lu yüzeyden coyote zıplamasında açılır (HandleCoyoteJump).
        private bool _bypassApexHangGravity;
        private float _endEarlyGravityMultiplier;
        // Taşıyan yüzeyden devralınan yatay hız. Hava sürtünmesi/ivmesi yalnızca oyuncunun kendi hızına
        // işler (yüzeyin referans çerçevesi); aksi halde 0.2 sn içinde sönüp duvar kaçardı.
        private float _carriedVelocityX;
        private WallSpeedRetention _wallSpeedRetention;
        // Celeste değişken zıplaması (PlayerStatsSO.CelesteVariableJump): tuş basılıyken bu süre boyunca yukarı hız sabit.
        private bool _holdJump;
        private float _varJumpTimer;
        private float _varJumpSpeed;

        /// <summary>Yerde başlar: girişte iniş yan etkileri (<see cref="OnLand"/>) çalışır.</summary>
        public NormalState(PlayerController context)
        {
            _context = context;
            _stats = context.Stats;
            IsOnGround = true;
        }

        /// <summary>Havaya adı olan bir profille girer (<see cref="AirEntry"/>). Profil taşıma hızı içeriyorsa
        /// (<see cref="SurfaceCarry"/>) Y bileşeni normal hıza eklenir (yerçekimi sönümler), X bileşeni referans çerçevesi olarak taşınır.</summary>
        public NormalState(PlayerController context, Vector2 inheritedVelocity, AirEntry entry)
        {
            _context = context;
            _stats = context.Stats;
            BeginAir(inheritedVelocity, entry);
        }

        public void EnterState()
        {
            if (IsOnGround) OnLand();
            else ApplyAirVelocity();
        }

        public void UpdateState()
        {
            if (IsOnGround) UpdateGround();
            else UpdateAir();
        }

        public void FixedUpdateState()
        {
            if (IsOnGround) FixedUpdateGround();
            else FixedUpdateAir();
        }

        public void ExitState()
        {
        }

        // =====================================================================
        // Yer ↔ hava sınırı
        // =====================================================================

        /// <summary>Yere iniş: hızın y'si sıfırlanır, kancalar bırakılır, kaynaklar yenilenir, iniş sesi/tozu/titreşimi.
        /// Sıra D2/W2/L2 kurallarına bağlı; değiştirme.</summary>
        private void OnLand()
        {
            IsOnGround = true;

            Vector2 currentVelocity = _context.Velocity;
            currentVelocity.y = 0f;
            _context.Velocity = currentVelocity;

            _context.LeftAnchor = null;
            _context.RightAnchor = null;
            _context.ResetDash();
            _context.ResetSlingshot();
            _context.RefillWallStamina();
            _context.ColorController.ResetAllColors();
            _context.ResetWallSlideTime();

            if (Time.timeSinceLevelLoad > 0.1f)
            {
                if (_context.Audio != null) _context.Audio.PlayLand();
                if (_context.VFX != null) _context.VFX.PlayLandDust();
                if (_context.VibrationSettings != null && HapticManager.Instance != null)
                {
                    HapticManager.Instance.Vibrate(_context.VibrationSettings.Land);
                }
            }
        }

        /// <summary>Yerden havaya: profil uygulanır ve hız verilir. Aynı karede başka bir duruma geçilmişse
        /// (örneğin kanca) eskiden olduğu gibi bu durum geri gelir.</summary>
        private void LeaveGround(Vector2 velocity, AirEntry entry)
        {
            BeginAir(velocity, entry);
            if (_context.CurrentState == this) EnterState();
            else _context.TransitionToState(this);
        }

        /// <summary>Hava alanlarını profile göre baştan kurar (eskiden her seferinde yeni AirborneState).</summary>
        private void BeginAir(Vector2 inheritedVelocity, AirEntry entry)
        {
            IsOnGround = false;
            _currentVelocity = inheritedVelocity + new Vector2(0f, entry.CarriedVelocity.y);
            _carriedVelocityX = entry.CarriedVelocity.x;
            _isJumping = entry.IsJumping;
            _grappleLockoutTimer = entry.GrappleLockout;
            _wallClimbLockoutTimer = entry.WallClimbLockout;
            _coyoteTimer = entry.Coyote;
            _horizontalInputLockoutTimer = entry.HorizontalLockout;
            _bypassJumpGravity = entry.BypassJumpGravity;
            _bypassApexHangGravity = false;
            _endEarlyGravityMultiplier = Mathf.Clamp01(entry.EndEarlyGravityMultiplier);
            _wallSpeedRetention = default;
            _holdJump = false;
            _varJumpTimer = 0f;
            _varJumpSpeed = 0f;

            if (entry.VariableJump) TryBeginHoldJump(inheritedVelocity.y, entry.CarriedVelocity.y);
        }

        private void ApplyAirVelocity()
        {
            _context.Velocity = _currentVelocity + new Vector2(_carriedVelocityX, 0f);
        }

        // =====================================================================
        // Yerde
        // =====================================================================

        private void UpdateGround()
        {
            // Sıra eski GroundedState ile aynı: zıplama havaya geçirse de dash ve duvar kontrolü bu karede çalışır.
            HandleGroundArmRouting();
            CheckGroundGrappleInput();
            CheckLeaveGround();
            CheckDashTransition();
            CheckGroundWallClimb();
        }

        private void FixedUpdateGround()
        {
            float targetVelocityX = _context.Input.LeftStick.x * _stats.MoveSpeed;
            _context.Velocity = new Vector2(targetVelocityX, _context.Velocity.y);
        }

        private void HandleGroundArmRouting()
        {
            Vector2 facingDirection = new Vector2(_context.Input.LeftStick.x, 0f).normalized;
            _context.LeftArm.UpdateArmRotation(facingDirection);
            _context.RightArm.UpdateArmRotation(_context.Input.RightStick);
        }

        private void CheckGroundGrappleInput()
        {
            if (_context.Input.IsRightTriggerHeld && !_context.RightAnchor.HasValue)
            {
                if (_context.TryCastGrapple(_context.Input.RightStick, out Vector2 hitPoint, out Transform node))
                {
                    _context.AttachAnchor(ActiveArm.Right, hitPoint, node);
                    _context.TransitionToState(new SwingingState(_context, ActiveArm.Right, wasGrounded: true));
                }
            }
        }

        private void CheckLeaveGround()
        {
            if (_context.JumpBufferTimer > 0f)
            {
                _context.ConsumeJumpBuffer();
                if (_context.Audio != null) _context.Audio.PlayJump();
                if (_context.VFX != null) _context.VFX.PlayJumpDust();
                if (_context.VibrationSettings != null && HapticManager.Instance != null)
                {
                    HapticManager.Instance.Vibrate(_context.VibrationSettings.Jump);
                }

                Vector2 jumpVelocityVector = new Vector2(_context.Velocity.x, _stats.JumpVelocity);

                IMovingSurface movingSurface = _context.PhysicsHandler.CurrentMovingSurface;
                // Yerden zıplarken yalnızca boost'lu yüzeyde (JumpBoostMultiplier > 0) yüzey hızı eklenir.
                // Boost'suz yüzeyin hızı aktarılmaz: stick'in hava hızının üstüne binip çok hızlandırıyordu.
                bool boostedSurface = false;
                if (movingSurface != null && movingSurface.JumpBoostMultiplier > 0f)
                {
                    jumpVelocityVector += movingSurface.SurfaceVelocity * movingSurface.JumpBoostMultiplier;
                    boostedSurface = true;
                }

                LeaveGround(jumpVelocityVector, AirEntry.Jump(boostedSurface));
            }
            else if (!_context.IsGrounded)
            {
                OnLeaveGround();
            }
        }

        /// <summary>Kenardan yürüyerek düşme: coyote açık (zıplanırsa coyote zıplaması).</summary>
        private void OnLeaveGround()
        {
            LeaveGround(_context.Velocity, AirEntry.WalkOff(_stats.CoyoteTimeDuration));
        }

        private void CheckGroundWallClimb()
        {
            if (!_context.CanWallClimb) return;

            if (_context.Input.IsLeftBumperHeld && _context.CanGrabWall(-1))
            {
                _context.TransitionToState(new WallClimbingState(_context, -1));
                return;
            }

            if (_context.Input.IsRightBumperHeld && _context.CanGrabWall(1))
            {
                _context.TransitionToState(new WallClimbingState(_context, 1));
            }
        }

        // =====================================================================
        // Havada
        // =====================================================================

        private void UpdateAir()
        {
            HandleAirArmRouting();
            HandleCoyoteJump();
            if (HandleAirWallJump()) return;
            HandleAirTransitions();
        }

        private void FixedUpdateAir()
        {
            DecrementTimers();

            if (_context.IsGrounded && _currentVelocity.y <= 0.01f)
            {
                OnLand();
                return;
            }

            HandleHorizontalObstructions();
            ApplyCustomGravity();
            ApplyAirMovement();
            HandleVerticalObstructions();

            ApplyAirVelocity();
        }

        /// <summary>Celeste modunda zıplamanın başlangıç hızını düşürür ve tutma süresini başlatır (tam yükseklik aynı kalır).</summary>
        private void TryBeginHoldJump(float jumpSpeed, float carriedY)
        {
            if (!_stats.CelesteVariableJump || jumpSpeed <= 0f) return;

            _holdJump = true;
            _currentVelocity.y = PlayerStatsSO.HoldJumpSpeed(jumpSpeed, Mathf.Abs(_stats.Gravity), _stats.VarJumpTime) + carriedY;
            _varJumpSpeed = _currentVelocity.y;
            _varJumpTimer = _stats.VarJumpTime;
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

        // Fizik adiminda sayilir (PF-6): sureler kare hizindan bagimsiz, her zaman ayni adim sayisi.
        private void DecrementTimers()
        {
            float dt = Time.fixedDeltaTime;
            if (_coyoteTimer > 0f) _coyoteTimer -= dt;
            if (_horizontalInputLockoutTimer > 0f) _horizontalInputLockoutTimer -= dt;
            if (_grappleLockoutTimer > 0f) _grappleLockoutTimer -= dt;
            if (_wallClimbLockoutTimer > 0f) _wallClimbLockoutTimer -= dt;
            if (_varJumpTimer > 0f) _varJumpTimer -= dt;
        }

        private void HandleAirArmRouting()
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
                if (!_bypassApexHangGravity) TryBeginHoldJump(jumpVelocityVector.y, 0f);
            }
        }

        private void HandleAirTransitions()
        {
            if (CheckDashTransition()) return;

            if (_grappleLockoutTimer <= 0f)
            {
                ProcessGrappleInput();
            }

            if (_wallClimbLockoutTimer <= 0f)
            {
                ProcessWallInteraction();
            }
        }

        private bool CheckDashTransition()
        {
            if (!_context.Input.IsDashPressed || !_context.HasDashCharge) return false;

            _context.TransitionToState(new DashState(_context, _context.Input.LeftStick));
            return true;
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
                    else if (!_holdJump && _currentVelocity.y > 0f && !_context.Input.IsJumpHeld)
                    {
                        gravityMultiplier = _stats.JumpEndEarlyGravityMultiplier * _endEarlyGravityMultiplier;
                    }
                    else if (!_bypassApexHangGravity && (!_holdJump || _context.Input.IsJumpHeld)
                             && Mathf.Abs(_currentVelocity.y) < _stats.ApexThreshold)
                    {
                        gravityMultiplier = _stats.ApexHangGravityMultiplier;
                    }
                }
            }

            float gravityStep = _stats.Gravity * gravityMultiplier * Time.fixedDeltaTime;
            _currentVelocity.y += gravityStep;
            _currentVelocity.y = Mathf.Max(_currentVelocity.y, _stats.TerminalVelocity);

            // Celeste: tuş basılıyken yukarı hız tutma süresi boyunca düşmez; bırakınca süre biter.
            if (_varJumpTimer > 0f)
            {
                if (_context.Input.IsJumpHeld) _currentVelocity.y = Mathf.Max(_currentVelocity.y, _varJumpSpeed);
                else _varJumpTimer = 0f;
            }
        }

        private void ApplyAirMovement()
        {
            if (_horizontalInputLockoutTimer > 0f) return;

            float moveInput = _context.Input.LeftStick.x;
            float targetX = moveInput * _stats.MoveSpeed * _stats.AirControlMultiplier;
            float maxX = _stats.MoveSpeed * _stats.AirControlMultiplier;
            bool hasInput = Mathf.Abs(moveInput) >= 0.05f;
            float accel = !hasInput ? _stats.AirDrag : _stats.AirAcceleration;

            // Celeste RunReduce: sinirin ustundeyken ayni yone basmak momentumu daha yavas sondurur (3.4 #3).
            if (hasInput && Mathf.Abs(_currentVelocity.x) > maxX && Mathf.Sign(_currentVelocity.x) == Mathf.Sign(moveInput))
            {
                accel = _stats.AirOverspeedReduce;
            }

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
                _varJumpTimer = 0f;
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
            if (_context.TryCastGrapple(direction, out Vector2 hitPoint, out Transform node))
            {
                _context.AttachAnchor(arm, hitPoint, node);

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
