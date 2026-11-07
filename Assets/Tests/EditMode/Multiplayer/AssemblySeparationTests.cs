using System.Linq;
using NUnit.Framework;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Tek kişilik kod (RockTheGame.Runtime) multiplayer'a bağlanamaz; multiplayer kendi assembly'sinde yaşar.
    /// docs/systems/multiplayer.md → "Tek kişilik oyunun korunması" madde 1.
    /// </summary>
    public class AssemblySeparationTests
    {
        [Test]
        public void Runtime_DoesNotReferenceMultiplayer()
        {
            var refs = typeof(New_Scripts.Player.PlayerController).Assembly.GetReferencedAssemblies();
            Assert.IsFalse(refs.Any(r => r.Name == "RockTheGame.Multiplayer"));
        }

        [Test]
        public void Steamworks_OnlyInSteamAssembly()
        {
            // Steam yalnız RockTheGame.Multiplayer.Steam'de; tek kişilik kod ve multiplayer çekirdeği ona bağlanmaz.
            var runtime = typeof(New_Scripts.Player.PlayerController).Assembly.GetReferencedAssemblies();
            var multiplayer = typeof(New_Scripts.Multiplayer.RaceProtocol).Assembly.GetReferencedAssemblies();
            Assert.IsFalse(runtime.Any(r => r.Name.StartsWith("Facepunch")));
            Assert.IsFalse(multiplayer.Any(r => r.Name.StartsWith("Facepunch") || r.Name == "RockTheGame.Multiplayer.Steam"));
        }

        [Test]
        public void Multiplayer_LivesInItsOwnAssembly()
        {
            Assert.AreEqual("RockTheGame.Multiplayer",
                typeof(New_Scripts.Multiplayer.RaceProtocol).Assembly.GetName().Name);
        }
    }
}
