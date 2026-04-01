namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin FSM (Durum Makinesi) içindeki her bir durumunun uygulaması gereken temel arayüz.
    /// </summary>
    public interface IPlayerState
    {
        void EnterState();
        void UpdateState();
        void FixedUpdateState();
        void ExitState();
    }
}