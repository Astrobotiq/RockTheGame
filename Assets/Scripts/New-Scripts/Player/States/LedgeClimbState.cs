using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Karakterin duvardan köşe düzlüğüne çıkış (Ledge Climb) animasyonunu
    /// kinematik olarak yumuşak bir şekilde gerçekleştiren durum sınıfı.
    /// </summary>
    public class LedgeClimbState : IPlayerState
    {
        private readonly PlayerController context;
        private readonly PlayerStatsSO stats;
        private readonly Vector2 startPos;
        private readonly Vector2 targetPos;
        private readonly Vector2 midPos;
        
        private float elapsedTime;
        private readonly float duration;

        public LedgeClimbState(PlayerController context, Vector2 startPos, Vector2 targetPos)
        {
            this.context = context;
            this.stats = context.Stats;
            this.startPos = startPos;
            this.targetPos = targetPos;
            this.duration = stats.LedgeClimbDuration;
            
            // Midpoint: directly above the start position, at the target height
            this.midPos = new Vector2(startPos.x, targetPos.y);
        }

        public void EnterState()
        {
            elapsedTime = 0f;
            context.Velocity = Vector2.zero;
        }

        public void UpdateState()
        {
            elapsedTime += Time.deltaTime;
            
            float u = Mathf.Clamp01(elapsedTime / duration);
            
            // 40% vertical phase, 60% horizontal phase
            const float verticalRatio = 0.4f;
            Vector2 currentPos;
            
            if (u < verticalRatio)
            {
                float tVert = u / verticalRatio;
                float easedT = EaseInOutQuad(tVert);
                currentPos = new Vector2(startPos.x, Mathf.Lerp(startPos.y, midPos.y, easedT));
            }
            else
            {
                float tHoriz = (u - verticalRatio) / (1f - verticalRatio);
                float easedT = EaseInOutQuad(tHoriz);
                currentPos = new Vector2(Mathf.Lerp(midPos.x, targetPos.x, easedT), targetPos.y);
            }
            
            context.PlayerRigidbody.position = currentPos;
            
            if (u >= 1f)
            {
                context.TransitionToState(new GroundedState(context));
            }
        }

        public void FixedUpdateState()
        {
            context.Velocity = Vector2.zero;
        }

        public void ExitState()
        {
            // Ensure we snap exactly to targetPos
            context.PlayerRigidbody.position = targetPos;
            context.Velocity = Vector2.zero;
        }
        
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }
    }
}
