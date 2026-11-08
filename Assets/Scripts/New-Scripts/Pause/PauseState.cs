using System;

namespace New_Scripts.Pause
{
    /// <summary>
    /// Pause durumunun saf mantığı (Unity'ye dokunmaz, EditMode'da test edilir).
    /// Hit-stop ile ilgisi yok: o <c>HitStopEvents</c> üzerinden çalışır, bu oyunu oyuncu isteğiyle durdurur.
    /// </summary>
    public sealed class PauseState
    {
        public bool IsPaused { get; private set; }

        /// <summary>Durum değişince bir kez tetiklenir; argüman yeni <see cref="IsPaused"/> değeridir.</summary>
        public event Action<bool> Changed;

        /// <summary>İzin varsa ve zaten duraklı değilse duraklatır. Değişiklik olduysa true döner.</summary>
        public bool TryPause(bool allowed)
        {
            if (IsPaused || !allowed) return false;
            IsPaused = true;
            Changed?.Invoke(true);
            return true;
        }

        /// <summary>Duraklıysa devam ettirir. İzin gerekmez: oyuncu her zaman çıkabilmeli.</summary>
        public bool Resume()
        {
            if (!IsPaused) return false;
            IsPaused = false;
            Changed?.Invoke(false);
            return true;
        }

        /// <summary>Duraklıysa devam ettirir, değilse (izin varsa) duraklatır.</summary>
        public bool Toggle(bool allowed) => IsPaused ? Resume() : TryPause(allowed);
    }
}
