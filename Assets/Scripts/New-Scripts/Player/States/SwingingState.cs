using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin kanca ile sarkaç hareketini (pendulum) yonetir. 
    /// İvme ve teğetsel hiz hesaplamalarinda, oyunun evrensel dinamik yercekimi referans alinir.
    /// </summary>
    public class SwingingState : IPlayerState
    {
        private readonly PlayerController _context;
        private readonly PlayerStatsSO _stats;
        private readonly ActiveArm _swingingArm;
        
        private Vector2 _anchorPoint;
        private float _ropeLength;
        private Vector2 _currentVelocity;

        public SwingingState(PlayerController context, ActiveArm swingingArm)
        {
            _context = context;
            _stats = context.Stats;
            _swingingArm = swingingArm;
        }

        public void EnterState()
        {
            if (!TrySetAnchorPoint())
            {
                _context.TransitionToState(new AirborneState(_context, _context.Velocity));
                return;
            }

            _ropeLength = Vector2.Distance(_context.PlayerRigidbody.position, _anchorPoint);
            _currentVelocity = _context.Velocity;
            
            _context.ResetDash();
            _context.ColorController.ResetBodyColor();
        }

        public void UpdateState()
        {
            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            ApplyArcadePendulum();
        }

        public void ExitState() { }

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
            bool activeTriggerHeld = _swingingArm == ActiveArm.Left ? _context.Input.IsLeftTriggerHeld : _context.Input.IsRightTriggerHeld;
            
            if (!activeTriggerHeld)
            {
                ReleaseAnchor();
                _context.TransitionToState(new AirborneState(_context, _currentVelocity));
                return;
            }
            
            CheckOppositeGrappleCast();
        }

        private void CheckOppositeGrappleCast()
        {
            bool oppositeTriggerHeld = _swingingArm == ActiveArm.Left ? _context.Input.IsRightTriggerHeld : _context.Input.IsLeftTriggerHeld;
            
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

        private void ApplyArcadePendulum()
        {
            float playerInputX = _swingingArm == ActiveArm.Left ? _context.Input.LeftStick.x : _context.Input.RightStick.x;
            
            // DİKKAT: Artık sarkaç yerçekimi evrensel formüle bağlı!
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

        private bool CheckSweepCollision(Vector2 playerPos, Vector2 movementDelta)
        {
            RaycastHit2D hit = Physics2D.CircleCast(playerPos, _stats.SwingCollisionRadius, movementDelta.normalized, movementDelta.magnitude, _context.GroundLayerMask);
            
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