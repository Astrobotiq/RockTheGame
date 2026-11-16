using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>Geçici (yer-hava-birlestirme-plani.md Aşama 2, adım 2): havada başlayan <see cref="NormalState"/>.
    /// Çağrı yerleri taşınınca silinecek.</summary>
    public class AirborneState : NormalState
    {
        public AirborneState(PlayerController context, Vector2 inheritedVelocity, AirEntry entry)
            : base(context, inheritedVelocity, entry)
        {
        }
    }
}
