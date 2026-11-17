using UnityEngine;

namespace New_Scripts.Player.States
{
    /// <summary>
    /// Havaya giriş profili: <see cref="NormalState"/>'in havadaki yerçekimi kuralı, kilitleri, coyote'u ve taşıma hızı.
    /// docs/systems/movement-core.md'deki "havaya giriş → yerçekimi kuralı" tablosu bu fabrikaların listesidir;
    /// yeni bir havaya geçiş ya mevcut bir profili seçer ya da buraya yeni bir profil ekler (yer-hava-birlestirme-plani.md).
    ///
    /// | Profil            | Yerçekimi kuralı                                                 | Kural |
    /// |-------------------|------------------------------------------------------------------|-------|
    /// | Jump, WalkOff     | tam kural (tuşa bağlı, apex hang)                                | H1    |
    /// | WallJump, ClimbJump | tam kural; boost'lu yüzeyde H3                                 | W5    |
    /// | SwingRelease, SlingshotLaunch, LaunchPad | yükselirken 3×, düşerken 1.5×, tuştan bağımsız | H2, L5 |
    /// | Jump(boosted)     | yükselirken 1.5×, tuştan bağımsız                                | H3    |
    /// | Fall, WallDrop    | sabit 1×                                                         | H4    |
    ///
    /// <c>default(AirEntry)</c> kullanma: <see cref="EndEarlyGravityMultiplier"/> 0 olur. Her zaman bir fabrika seç.
    /// </summary>
    public readonly struct AirEntry
    {
        public readonly bool IsJumping;
        public readonly bool BypassJumpGravity;
        public readonly float EndEarlyGravityMultiplier;
        public readonly float GrappleLockout;
        public readonly float WallClimbLockout;
        public readonly float Coyote;
        public readonly float HorizontalLockout;
        public readonly Vector2 CarriedVelocity;
        /// <summary>Taşınan yatay hızın sıfıra indiği süre (sn); 0 = sönmez (duvardan ayrılma kuralı).
        /// Yalnızca hareketli node bırakışları verir (docs/hareketli-node-tasarimi.md).</summary>
        public readonly float CarriedDecayTime;
        /// <summary>Oyuncunun kendi zıplaması: CelesteVariableJump açıkken tuş basılı tutma süresi bu profillerde işler.</summary>
        public readonly bool VariableJump;

        /// <summary>Boost'lu yüzeyden zıplamada yükseliş çarpanı 3× yerine 1.5× (H3).</summary>
        private const float BoostedEndEarlyMultiplier = 0.5f;

        private AirEntry(bool isJumping, bool bypassJumpGravity = false, float endEarlyGravityMultiplier = 1f,
            float grappleLockout = 0f, float wallClimbLockout = 0f, float coyote = 0f, float horizontalLockout = 0f,
            Vector2 carriedVelocity = default, bool variableJump = false, float carriedDecayTime = 0f)
        {
            IsJumping = isJumping;
            BypassJumpGravity = bypassJumpGravity;
            EndEarlyGravityMultiplier = endEarlyGravityMultiplier;
            GrappleLockout = grappleLockout;
            WallClimbLockout = wallClimbLockout;
            Coyote = coyote;
            HorizontalLockout = horizontalLockout;
            CarriedVelocity = carriedVelocity;
            VariableJump = variableJump;
            CarriedDecayTime = carriedDecayTime;
        }

        private static AirEntry JumpRule(bool boostedSurface, float wallClimbLockout = 0f, float horizontalLockout = 0f,
            Vector2 carriedVelocity = default) =>
            new AirEntry(true, bypassJumpGravity: boostedSurface,
                endEarlyGravityMultiplier: boostedSurface ? BoostedEndEarlyMultiplier : 1f,
                wallClimbLockout: wallClimbLockout, horizontalLockout: horizontalLockout, carriedVelocity: carriedVelocity,
                variableJump: !boostedSurface);

        /// <summary>Yerden zıplama. Boost'lu yüzeyde (JumpBoostMultiplier &gt; 0) yükseliş tuştan bağımsız ve yumuşak (H3).</summary>
        public static AirEntry Jump(bool boostedSurface) => JumpRule(boostedSurface);

        /// <summary>Kenardan yürüyerek düşme: tam kural, coyote açık (zıplanırsa coyote zıplaması olur).</summary>
        public static AirEntry WalkOff(float coyote) => new AirEntry(true, coyote: coyote);

        /// <summary>Kayarken ya da havadan duvar zıplaması: duvardan dışarı, kısa yatay girdi kilidi.</summary>
        public static AirEntry WallJump(float horizontalLockout, bool boostedSurface, Vector2 carriedVelocity) =>
            JumpRule(boostedSurface, horizontalLockout: horizontalLockout, carriedVelocity: carriedVelocity);

        /// <summary>Tırmanırken dikey zıplama: hemen yeniden tutunmasın diye kısa duvar kilidi.</summary>
        public static AirEntry ClimbJump(bool boostedSurface, Vector2 carriedVelocity) =>
            JumpRule(boostedSurface, wallClimbLockout: 0.2f, carriedVelocity: carriedVelocity);

        /// <summary>Tek kancayı bırakma (H2): yükselirken 3×, düşerken 1.5×, tuştan bağımsız.
        /// Hareketli node'dan bırakınca node'un hızı <paramref name="carriedVelocity"/> ile taşınır.</summary>
        public static AirEntry SwingRelease(Vector2 carriedVelocity = default, float carriedDecayTime = 0f) =>
            new AirEntry(true, bypassJumpGravity: true, carriedVelocity: carriedVelocity, carriedDecayTime: carriedDecayTime);

        /// <summary>Slingshot fırlatışının sonu ya da erken bırakma (L5): SwingRelease kuralı + kanca kilidi.</summary>
        public static AirEntry SlingshotLaunch(float grappleLockout, Vector2 carriedVelocity = default, float carriedDecayTime = 0f) =>
            new AirEntry(true, bypassJumpGravity: true, grappleLockout: grappleLockout, carriedVelocity: carriedVelocity,
                carriedDecayTime: carriedDecayTime);

        /// <summary>Launch pad (H2): SwingRelease kuralı.</summary>
        public static AirEntry LaunchPad() => new AirEntry(true, bypassJumpGravity: true);

        /// <summary>Sabit 1× (H4): dash sonu, çift kanca bırakma, kanca kopması, oda geçişi, respawn.</summary>
        public static AirEntry Fall(Vector2 carriedVelocity = default, float carriedDecayTime = 0f) =>
            new AirEntry(false, carriedVelocity: carriedVelocity, carriedDecayTime: carriedDecayTime);

        /// <summary>Duvardan bırakma/düşme: sabit 1×, taşıyan duvarın hızı, isteğe bağlı duvar kilidi. Coyote yok:
        /// hemen ardından basılan zıplamayı havadan duvar zıplaması karşılar (W9).</summary>
        public static AirEntry WallDrop(Vector2 carriedVelocity, float wallClimbLockout = 0f) =>
            new AirEntry(false, wallClimbLockout: wallClimbLockout, carriedVelocity: carriedVelocity);
    }
}
