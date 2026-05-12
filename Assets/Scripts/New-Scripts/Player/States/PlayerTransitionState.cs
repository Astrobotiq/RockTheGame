using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Odalar arasi gecis aninda oyuncunun fiziksel etkilesimlerden ve girdilerden koptugu ozel durumu temsil eder.
    /// </summary>
    public class PlayerTransitionState : IPlayerState
    {
        private readonly PlayerController controller;

        public PlayerTransitionState(PlayerController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            controller.Velocity = Vector2.zero;
        }

        public void UpdateState()
        {
        }

        public void FixedUpdateState()
        {
        }

        public void ExitState()
        {
        }
    }
}