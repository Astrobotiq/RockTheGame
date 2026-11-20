using New_Scripts.Player.Nodes.Rotation;
using New_Scripts.Player.Nodes.Spiral;
using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin kanca ile sarkaç hareketini (pendulum) yonetir. 
    /// Yerden geliyorsa once kancaya dogru kucuk bir sicrama (hop) yapar.
    /// 
    /// DEĞİŞİKLİK (FullRotation): FullRotationTracker inject edildi.
    /// Anchor, FullRotationNode taşıyorsa; 360 tamamlanınca node'un efekti tetiklenir.
    /// Mevcut pendulum/hop mantığına dokunulmadı.
    /// </summary>
    public class SwingingState : IPlayerState, INodeReleaseHandler
    {
        private readonly PlayerController _context;
        private readonly PlayerStatsSO _stats;
        private readonly ActiveArm _swingingArm;
        private readonly bool _wasGrounded;

        private Vector2 _anchorPoint;
        private float _ropeLength;
        private Vector2 _currentVelocity;

        // Hareketli node ataleti: sınırlı ivmeyle yumuşatılmış node hızı (docs/hareketli-node-tasarimi.md)
        private Vector2 _pivotVelocity;

        // Hop (Sıçrama) aşaması değişkenleri
        private bool _isHopping;
        private float _hopTimer;

        // --- 360 Dönüş Takibi ---
        private FullRotationTracker _rotationTracker;
        private FullRotationNode _fullRotationNode;

        // --- Spiral node (docs/spiral-node-tasarimi.md) ---
        private SpiralNode _spiral;

        // Takılma kontrolü: önceki adımın konumu, teğeti ve komut verilen teğet hızı
        private bool _hasSpiralSample;
        private Vector2 _spiralSamplePos;
        private Vector2 _spiralSampleTangent;
        private float _spiralSampleCommand;
        private int _spiralBlockedCount;
        private bool _spiralMotorStarted;
        private bool _spiralFeedbackStarted;

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
                _context.TransitionToState(new NormalState(_context, _context.Velocity, AirEntry.Fall()));
                return;
            }

            _context.ResetDash();
            _context.ColorController.ResetBodyColor();

            if (_context.Audio != null)
            {
                _context.Audio.PlayGrappleConnect();
            }


            if (_wasGrounded)
                StartHopPhase();
            else
                InitializePendulum();

            // Spiral varsa dönüş sayacı bağlanmaz (aynı node'da ikisi birlikte kullanılmaz).
            if (!TryBindSpiralNode())
                TryBindFullRotationNode();
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
                TickRotationTracker(); // Hop bitmeden önce saymaya başlama

                // Ters dönen spiral: yalnızca motor çalışırken (bu adımda kanca kopmadıysa) sayar.
                if (_spiral != null && _context.CurrentState == this)
                    _spiral.TickReverse(_context.PlayerRigidbody.position, _anchorPoint);
            }
        }

        public void ExitState()
        {
            if (_spiral != null)
            {
                Transform node = ArmAnchorNode();
                bool stillHeld = node != null && node.GetComponentInParent<SpiralNode>() == _spiral;
                _spiral.Reversed -= OnSpiralReversed;
                _spiral.Disconnect(stillHeld);
                StopSpiralFeedback();
            }

            if (_fullRotationNode != null)
            {
                _fullRotationNode.OnConnectionLost();
            }
        }

        // --- SPİRAL NODE ---

        private Transform ArmAnchorNode() =>
            _swingingArm == ActiveArm.Left ? _context.LeftAnchorNode : _context.RightAnchorNode;

        private bool TryBindSpiralNode()
        {
            Transform node = ArmAnchorNode();
            _spiral = node != null ? node.GetComponentInParent<SpiralNode>() : null;
            if (_spiral == null) return false;

            _spiral.Connect(_context);
            _spiral.Reversed += OnSpiralReversed;
            if (!_isHopping) _context.SpiralFacingSign = _spiral.SpinSign;

            _spiralFeedbackStarted = true;
            if (_context.Audio != null) _context.Audio.PlaySpiralLoop();
            if (_context.VibrationSettings != null && HapticManager.Instance != null)
                HapticManager.Instance.StartContinuousVibration(_context.VibrationSettings.SpiralOrbit);
            return true;
        }

        private void StopSpiralFeedback()
        {
            _context.SpiralFacingSign = 0;
            if (!_spiralFeedbackStarted) return;
            _spiralFeedbackStarted = false;

            if (_context.Audio != null) _context.Audio.StopSpiralLoop();
            if (HapticManager.Instance != null) HapticManager.Instance.StopContinuousVibration();
        }

        private void OnSpiralReversed()
        {
            if (_context.Audio != null) _context.Audio.PlaySpiralReverse();
            if (_context.VibrationSettings != null && HapticManager.Instance != null)
                HapticManager.Instance.Vibrate(_context.VibrationSettings.SpiralReverse);
        }

        /// <summary>Motorun ilk adımında: oyuncu spinin tersine geliyorsa mücadele sesi ve titreşimi.</summary>
        private void OnSpiralMotorStart(float speedAlongTangent, int spinSign)
        {
            _spiralMotorStarted = true;
            if (speedAlongTangent * spinSign >= -1f) return;

            if (_context.Audio != null) _context.Audio.PlaySpiralOpposeGrab();
            if (_context.VibrationSettings != null && HapticManager.Instance != null)
                HapticManager.Instance.Vibrate(_context.VibrationSettings.SpiralOpposeGrab);
        }

        /// <summary>
        /// Spiral oyuncuyu bir yüzeye bastırıyorsa (tutunma çarpışması teğet hareketi sessizce durdurur) art arda
        /// SpiralBlockedSteps adım komutun SpiralBlockedRatio'su bile gerçekleşmez; o zaman kanca kopar (SP4).
        /// </summary>
        private bool IsSpiralBlocked()
        {
            if (!_hasSpiralSample) return false;

            Vector2 moved = _context.PlayerRigidbody.position - _spiralSamplePos - _context.AnchorDelta(_swingingArm);
            float actual = Vector2.Dot(moved, _spiralSampleTangent) / Time.fixedDeltaTime;

            if (SpiralDriveMath.IsBlocked(_spiralSampleCommand, actual, _stats.SpiralBlockedRatio))
                _spiralBlockedCount++;
            else
                _spiralBlockedCount = 0;

            return _spiralBlockedCount >= _stats.SpiralBlockedSteps;
        }

        /// <summary>
        /// Spiral motoru: node'a göre teğet hızı hedef hıza sınırlı ivmeyle yaklaştırır (yerçekimi zaten eklendi).
        /// Stick hedefi spin yönünde ±SpiralInputInfluence oranında değiştirir, işaretini asla çevirmez.
        /// </summary>
        private float DriveSpiral(float speedAlongTangent, Vector2 tangent)
        {
            Vector2 stick = _swingingArm == ActiveArm.Left ? _context.Input.LeftStick : _context.Input.RightStick;
            int spinSign = _spiral.SpinSign;
            _context.SpiralFacingSign = spinSign;
            if (!_spiralMotorStarted) OnSpiralMotorStart(speedAlongTangent, spinSign);
            float input = Vector2.Dot(stick, tangent * spinSign);
            float target = SpiralDriveMath.EffectiveTarget(spinSign, _spiral.TargetSpeed, input,
                _stats.SpiralInputInfluence, _context.ActiveSpeedMultiplier);

            float speed = SpiralDriveMath.StepTangentialSpeed(speedAlongTangent, target, _stats.SpiralDriveAccel,
                _stats.SpiralOpposeAccel, _stats.SpiralOverspeedDecay, Time.fixedDeltaTime);

            if (_ropeLength > 0.001f)
                _spiral.ReportAngularSpeed(speed / _ropeLength * Mathf.Rad2Deg);
            return speed;
        }

        /// <summary>
        /// Gönüllü bırakışta spin teğeti yönünde SpiralExitBoost ekler (SP8, docs/spiral-cikis-hizi.md). Yalnızca motor
        /// çalışmışken, node etkinken, spin yönünde ve yeterince hızlıyken; aksi halde normal bırakış (SP5).
        /// Kopma, çarpışma, çift kanca ve respawn bu yoldan geçmez.
        /// </summary>
        private bool TryApplySpiralExitBoost()
        {
            if (_spiral == null || _isHopping || !_spiralMotorStarted || !_spiral.isActiveAndEnabled) return false;

            Vector2 directionToAnchor = (_anchorPoint - _context.PlayerRigidbody.position).normalized;
            Vector2 tangent = new Vector2(directionToAnchor.y, -directionToAnchor.x);
            float boost = SpiralDriveMath.ExitBoost(Vector2.Dot(_currentVelocity, tangent), _spiral.SpinSign,
                _spiral.TargetSpeed * _context.ActiveSpeedMultiplier, _stats.SpiralExitMinSpeedRatio, _stats.SpiralExitBoost);
            if (boost == 0f) return false;

            _currentVelocity += tangent * boost;
            _spiral.PulseGlow();
            return true;
        }

        private void PlaySpiralExitFeedback()
        {
            if (_context.Audio != null) _context.Audio.PlaySpiralRelease();
            if (_context.VibrationSettings != null && HapticManager.Instance != null)
                HapticManager.Instance.Vibrate(_context.VibrationSettings.SpiralRelease);
        }

        // --- 360 DÖNÜŞ METOTLARI ---

        private void TryBindFullRotationNode()
        {
            // Anchor noktasındaki Collider2D üzerinde FullRotationNode arar.
            // Yoksa tracker hiç oluşturulmaz → sıfır overhead.
            Collider2D[] hits = Physics2D.OverlapPointAll(_anchorPoint);
            foreach (var col in hits)
            {
                if (col.TryGetComponent(out FullRotationNode node))
                {
                    _fullRotationNode = node;
                    _rotationTracker = new FullRotationTracker(_fullRotationNode.TargetDegrees);
                    
                    Vector2 radiusVector = _context.PlayerRigidbody.position - _anchorPoint;
                    
                    float crossZ = (radiusVector.x * _currentVelocity.y) - (radiusVector.y * _currentVelocity.x);
                    
                    bool isClockwise = crossZ < 0f;
                    
                    _fullRotationNode.InitializeNodeConnection(_context.PlayerRigidbody.position, isClockwise);
                    return;
                }
            }

            _fullRotationNode = null;
            _rotationTracker = null;
        }

        private void TickRotationTracker()
        {
            if (_rotationTracker == null || _fullRotationNode == null) return;

            if (!_fullRotationNode.CanTrigger) return;

            bool completed = _rotationTracker.Tick(
                _context.PlayerRigidbody.position,
                _anchorPoint
            );

            _fullRotationNode.UpdateProgressVisual(_rotationTracker.Progress);

            if (completed)
            {
                _fullRotationNode.TriggerRotationEffect();
                _rotationTracker.Reset();
            }
        }

        // --- HOP (SIÇRAMA) AŞAMASI METOTLARI ---

        private void StartHopPhase()
        {
            _isHopping = true;
            _hopTimer = _stats.SwingHopDuration;

            Vector2 playerPos = _context.PlayerRigidbody.position;
            Vector2 directionToAnchor = (_anchorPoint - playerPos).normalized;

            float dirX = Mathf.Sign(directionToAnchor.x);
            _currentVelocity = new Vector2(dirX * _stats.SwingHopForwardSpeed, _stats.SwingHopUpwardSpeed);

            _context.Velocity = _currentVelocity;
        }

        private void ApplyHopPhysics()
        {
            RefreshAnchorPoint();
            _hopTimer -= Time.fixedDeltaTime;

            _currentVelocity.y += _stats.Gravity * Time.fixedDeltaTime;
            _context.Velocity = _currentVelocity;

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
            // Gelen hız dünyaya göre: tutunma anında korunur, node'a göre hız = dünya − node. Hareket eden node'u
            // yakalayınca ip oyuncuyu çeker.
            _pivotVelocity = _context.AnchorVelocity(_swingingArm);
            _currentVelocity = _context.Velocity - _pivotVelocity;
            // Hop'tan ilk pendulum adımına geçişte takılma ölçülmez.
            _hasSpiralSample = false;
            _spiralBlockedCount = 0;
        }

        private void ApplyArcadePendulum()
        {
            if (_spiral != null && IsSpiralBlocked())
            {
                BreakGrapple();
                return;
            }

            float playerInputX = _swingingArm == ActiveArm.Left
                ? _context.Input.LeftStick.x
                : _context.Input.RightStick.x;

            _currentVelocity.y += _stats.Gravity * Time.fixedDeltaTime;

            // Hareketli node: node'un ivmesi ters yönde salınıma geçer (radyal kısmı aşağıdaki izdüşüm atar).
            ApplyNodeInertia();

            // Hareketli node: oyuncu node'un kaymasıyla taşınır, sarkaç node'a göre çalışır.
            RefreshAnchorPoint();
            Vector2 playerPos = _context.PlayerRigidbody.position;
            Vector2 carriedPos = playerPos + _context.AnchorDelta(_swingingArm);
            Vector2 directionToAnchor = (_anchorPoint - carriedPos).normalized;
            Vector2 tangent = new Vector2(directionToAnchor.y, -directionToAnchor.x);

            float speedAlongTangent = Vector2.Dot(_currentVelocity, tangent);

            if (_spiral != null)
            {
                // Spiral pompalamanın ve MaxSwingSpeed sınırının yerine geçer.
                speedAlongTangent = DriveSpiral(speedAlongTangent, tangent);
                _hasSpiralSample = true;
                _spiralSamplePos = playerPos;
                _spiralSampleTangent = tangent;
                _spiralSampleCommand = speedAlongTangent;
            }
            else
            {
                float inputForce = playerInputX * _stats.SwingForceMultiplier * Time.fixedDeltaTime;

                float currentMaxSpeed = _stats.MaxSwingSpeed * _context.ActiveSpeedMultiplier;

                if (Mathf.Abs(speedAlongTangent + inputForce) < currentMaxSpeed)
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

        /// <summary>Node hızının (sınırlı ivmeyle yumuşatılmış) bu adımdaki değişimi × atalet, node'a göre hızdan çıkarılır.
        /// Sabit node'da hız hep 0, etkisi yok.</summary>
        private void ApplyNodeInertia()
        {
            Vector2 previous = _pivotVelocity;
            _pivotVelocity = Vector2.MoveTowards(_pivotVelocity, _context.AnchorVelocity(_swingingArm),
                _stats.MovingNodeMaxPivotAccel * Time.fixedDeltaTime);
            _currentVelocity -= (_pivotVelocity - previous) * _stats.MovingNodeInertia;
        }

        /// <summary>Anchor hareketli node'u izliyorsa güncel konumu alır.</summary>
        private void RefreshAnchorPoint()
        {
            Vector2? anchor = _swingingArm == ActiveArm.Left ? _context.LeftAnchor : _context.RightAnchor;
            if (anchor.HasValue) _anchorPoint = anchor.Value;
        }

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
                Vector2 nodeVelocity = _context.AnchorReleaseVelocity(_swingingArm) * _stats.MovingNodeLaunchMultiplier;
                bool spiralExit = TryApplySpiralExitBoost();
                ReleaseAnchor();

                AirEntry entry = spiralExit
                    ? AirEntry.SpiralRelease(nodeVelocity, _stats.MovingNodeCarryDecayTime, _stats.SpiralExitHoldTime)
                    : AirEntry.SwingRelease(nodeVelocity, _stats.MovingNodeCarryDecayTime);
                _context.TransitionToState(new NormalState(_context, _currentVelocity, entry));
                // ExitState döngü sesini ve sürekli titreşimi kapattıktan sonra: tek seferlik çıkış geri bildirimi.
                if (spiralExit) PlaySpiralExitFeedback();
                return;
            }

            CheckOppositeGrappleCast();
        }

        /// <summary>
        /// Tutulan node tutulamaz hale geldi (kapanan çiçek): tetik bırakılmış gibi tek kanca bırakışı, üstüne node'dan itme.
        /// Spiral çıkış boost'u yok: node zaten kapandı.
        /// </summary>
        public void ReleaseByNode(ActiveArm arm, Vector2 impulse)
        {
            if (arm != _swingingArm) return;

            Vector2 nodeVelocity = _context.AnchorReleaseVelocity(_swingingArm) * _stats.MovingNodeLaunchMultiplier;
            ReleaseAnchor();
            AirEntry entry = AirEntry.SwingRelease(nodeVelocity, _stats.MovingNodeCarryDecayTime);
            _context.TransitionToState(new NormalState(_context, _currentVelocity + impulse, entry));
        }

        private void CheckOppositeGrappleCast()
        {
            bool oppositeTriggerHeld = _swingingArm == ActiveArm.Left
                ? _context.Input.IsRightTriggerHeld
                : _context.Input.IsLeftTriggerHeld;

            if (!oppositeTriggerHeld) return;

            Vector2 aimStick = _swingingArm == ActiveArm.Left ? _context.Input.RightStick : _context.Input.LeftStick;

            if (_context.TryCastGrapple(aimStick, out Vector2 hitPoint, out Transform node))
            {
                _context.AttachAnchor(_swingingArm == ActiveArm.Left ? ActiveArm.Right : ActiveArm.Left, hitPoint, node);

                EvaluateDualGrappleTransition();
            }
        }

        private void EvaluateDualGrappleTransition()
        {
            // Sonraki durum dünyaya göre hız bekler (sallanırken Velocity node'a göre).
            _context.Velocity = _currentVelocity + _context.AnchorVelocity(_swingingArm);
            if (_context.CanSlingshot && _context.CheckNodeCoincidence())
                _context.TransitionToState(new SlingshotState(_context));
            else
                _context.TransitionToState(new DualSwingingState(_context));
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
            _context.TransitionToState(new NormalState(_context, _currentVelocity, AirEntry.Fall()));
        }

        private bool CheckSweepCollision(Vector2 playerPos, Vector2 movementDelta)
        {
            // Oyuncunun kendi kutusuyla aranir: daire kosedeki temasi kaciriyordu, egimlerde karakter takili kaliyordu.
            RaycastHit2D hit = Physics2D.BoxCast(playerPos, _context.PlayerCollider.bounds.size, 0f, movementDelta.normalized,
                movementDelta.magnitude, _context.GroundLayerMask);

            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                if (angle > _stats.MaxWallAngle)
                    BreakGrapple();
                else
                {
                    _context.LeftAnchor = null;
                    _context.RightAnchor = null;
                    _context.TransitionToState(new NormalState(_context));
                }

                return true;
            }

            return false;
        }
    }
}