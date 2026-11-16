namespace New_Scripts.Player.States
{
    /// <summary>Geçici (yer-hava-birlestirme-plani.md Aşama 2, adım 2): yerde başlayan <see cref="NormalState"/>.
    /// Çağrı yerleri taşınınca silinecek.</summary>
    public class GroundedState : NormalState
    {
        public GroundedState(PlayerController context) : base(context)
        {
        }
    }
}
