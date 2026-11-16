using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin duvardan asagi dogru surtunerek kaydigi durumu yonetir.
    /// Maksimum 2 saniye surebilir. Oyuncu analogu duvara itmezse veya sure biterse karakter duser.
    /// Kayarken ziplama (Wall Jump), tirmanma (Wall Climb), dash atma ve kanca firlatma (Swing) yeteneklerine izin verir.
    /// </summary>
    public class WallSlidingState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;
        private readonly int wallDirection; // -1: Sol Duvar, 1: Sag Duvar

        public WallSlidingState(PlayerController context, int wallDirection)
        {
            this.context = context;
            this.stats = context.Stats;
            this.wallDirection = wallDirection;
        }

        public void EnterState()
        {
            context.PhysicsHandler.ClingingWallDirection = wallDirection;
            if (context.VibrationSettings != null && HapticManager.Instance != null)
            {
                var profile = wallDirection == -1 
                    ? context.VibrationSettings.WallSlideLeft 
                    : context.VibrationSettings.WallSlideRight;
                HapticManager.Instance.StartContinuousVibration(profile);
            }
        }

        public void UpdateState()
        {
            context.ConsumeWallSlideTime(Time.deltaTime);

            HandleArmRouting();
            CheckInputTransitions();
        }

        public void FixedUpdateState()
        {
            Vector2 velocity = context.Velocity;

            if (context.CurrentWallSlideTime <= 0f)
            {
                context.TransitionToState(new NormalState(context, velocity, AirEntry.WallDrop(CurrentCarrierVelocity())));
                return;
            }

            bool isPushingTowardsWall = (wallDirection == -1 && context.Input.LeftStick.x < -0.1f) || 
                                        (wallDirection == 1 && context.Input.LeftStick.x > 0.1f);
            
            if (!isPushingTowardsWall)
            {
                // Coyote yok (Celeste gibi): hemen ardından basılan zıplamayı havadan duvar zıplaması karşılar (W9).
                // Eskiden coyote açıktı ve öncelikli olduğu için duvarın dibinde dikey zıplama çıkıyordu.
                context.TransitionToState(new NormalState(context, velocity,
                    AirEntry.WallDrop(CurrentCarrierVelocity(), wallClimbLockout: 0.2f)));
                return;
            }

            velocity.y = Mathf.MoveTowards(velocity.y, -stats.WallSlideMaxSpeed, stats.WallSlideFriction * Time.fixedDeltaTime);

            // Duvar tolerans icinde uzaklastiysa stick'in itisiyla duvara yapis.
            velocity.x = 0f;
            if (context.TryMeasureWallGap(wallDirection, out float gap))
            {
                velocity.x = wallDirection * WallClingMath.ClosingSpeed(gap, stats.WallSnapSafetyOffset,
                    stats.WallClimbContactTolerance, Time.fixedDeltaTime);
            }
            context.Velocity = velocity;

            if (context.IsGrounded)
            {
                context.TransitionToState(new NormalState(context));
            }
            else if (!IsStillOnWall())
            {
                context.TransitionToState(new NormalState(context, velocity, AirEntry.WallDrop(CurrentCarrierVelocity())));
            }
        }

        public void ExitState()
        {
            context.PhysicsHandler.ClingingWallDirection = 0;
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.StopContinuousVibration();
            }
        }

        private void HandleArmRouting()
        {
            context.LeftArm.UpdateArmRotation(context.Input.LeftStick);
            context.RightArm.UpdateArmRotation(context.Input.RightStick);
        }

        private void CheckInputTransitions()
        {
            if (context.Input.IsDashPressed && context.HasDashCharge)
            {
                context.TransitionToState(new DashState(context, context.Input.LeftStick));
                return;
            }

            if (CheckGrappleInputs()) return;

            if (context.JumpBufferTimer > 0f)
            {
                PerformWallJump(context, wallDirection);
                return;
            }

            bool climbInput = (wallDirection == -1 && context.Input.IsLeftBumperHeld) || 
                              (wallDirection == 1 && context.Input.IsRightBumperHeld);
                              
            if (climbInput && context.CanWallClimb)
            {
                context.TransitionToState(new WallClimbingState(context, wallDirection));
                return;
            }
        }

        /// <summary>
        /// Duvardan disari ziplama (W5 kayma kurali): buffer'i tuketir, kayma suresini yeniler (W4), havaya (NormalState) gecer.
        /// Kayarken ve havada duvara yakinken (NormalState, WallJumpCheckDistance) ayni ziplama kullanilir.
        /// </summary>
        public static void PerformWallJump(PlayerController context, int wallDirection)
        {
            PlayerStatsSO stats = context.Stats;
            context.ConsumeJumpBuffer();
            context.ResetWallSlideTime();

            if (context.VibrationSettings != null && HapticManager.Instance != null)
            {
                HapticManager.Instance.Vibrate(context.VibrationSettings.Jump);
            }

            Vector2 jumpDirection = new Vector2(-wallDirection * stats.WallSlideJumpForce.x, stats.WallSlideJumpForce.y);

            IMovingSurface movingSurface = wallDirection == -1
                ? context.PhysicsHandler.CurrentLeftMovingSurface
                : context.PhysicsHandler.CurrentRightMovingSurface;

            // Boost'lu yüzeyde eski formül jumpDirection'a eklenir; değilse yüzeyin hızı referans çerçevesi olarak taşınır.
            Vector2 surfaceAddition = SurfaceCarry.JumpAddition(movingSurface, Time.fixedDeltaTime, out bool bypassJumpGravity);
            Vector2 carriedVelocity = Vector2.zero;
            if (bypassJumpGravity) jumpDirection += surfaceAddition;
            else carriedVelocity = surfaceAddition;

            context.TransitionToState(new NormalState(context, jumpDirection,
                AirEntry.WallJump(stats.WallJumpInputLockoutTime, boostedSurface: bypassJumpGravity, carriedVelocity)));
        }

        private bool CheckGrappleInputs()
        {
            bool transitionTriggered = false;

            // Sol Kanca Atisi
            if (context.Input.IsLeftTriggerHeld && !context.LeftAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.LeftStick, out Vector2 hitPoint))
                {
                    context.LeftAnchor = hitPoint;
                    transitionTriggered = true;
                }
            }

            // Sag Kanca Atisi
            if (context.Input.IsRightTriggerHeld && !context.RightAnchor.HasValue)
            {
                if (context.TryCastGrapple(context.Input.RightStick, out Vector2 hitPoint))
                {
                    context.RightAnchor = hitPoint;
                    transitionTriggered = true;
                }
            }

            if (transitionTriggered)
            {
                EvaluateGrappleTransition();
                return true;
            }

            return false;
        }

        private void EvaluateGrappleTransition()
        {
            if (context.LeftAnchor.HasValue && context.RightAnchor.HasValue)
            {
                if (context.CheckNodeCoincidence() && context.CanSlingshot)
                {
                    context.TransitionToState(new SlingshotState(context));
                }
                else
                {
                    context.TransitionToState(new DualSwingingState(context));
                }
            }
            else if (context.LeftAnchor.HasValue)
            {
                context.TransitionToState(new SwingingState(context, ActiveArm.Left));
            }
            else if (context.RightAnchor.HasValue)
            {
                context.TransitionToState(new SwingingState(context, ActiveArm.Right));
            }
        }

        private Vector2 CurrentCarrierVelocity()
        {
            return SurfaceCarry.Velocity(context.PhysicsHandler.CurrentCarrier, Time.fixedDeltaTime);
        }

        private bool IsStillOnWall()
        {
            bool touching = wallDirection == -1 ? context.IsTouchingLeftWall : context.IsTouchingRightWall;
            return touching || context.TryMeasureWallGap(wallDirection, out _);
        }
    }
}