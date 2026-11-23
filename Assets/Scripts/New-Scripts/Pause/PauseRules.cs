namespace New_Scripts.Pause
{
    /// <summary>Oyunun o an duraklatılıp duraklatılamayacağını söyleyen saf kural.</summary>
    public static class PauseRules
    {
        /// <param name="playerActive">Oyuncu objesi sahnede aktif (ölünce <c>SetActive(false)</c> olur).</param>
        /// <param name="playerBusy"><c>PlayerController.IsPaused</c>: respawn ya da hit-stop sırasında true.</param>
        /// <param name="inTransition">Oda geçişi sürüyor (<c>PlayerTransitionState</c>).</param>
        /// <param name="blocked">Bir sistem pause'u kapattı (ör. çok oyunculu yarış).</param>
        public static bool CanPause(bool playerActive, bool playerBusy, bool inTransition, bool blocked)
            => playerActive && !playerBusy && !inTransition && !blocked;

        /// <summary>
        /// <see cref="CanPause"/>'ın <c>blocked</c> değeri: bir sistem pause'u kapattıysa kapalı, ama çok oyunculu oturum
        /// "şu an duraklatılabilir" diyorsa (yarış sürüyor) açık.
        /// </summary>
        public static bool IsBlocked(bool blockerPresent, bool multiplayerAllows) => blockerPresent && !multiplayerAllows;
    }
}
