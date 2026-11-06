using NUnit.Framework;
using New_Scripts.Multiplayer.Hud;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>HUD metinleri. docs/superpowers/plans/2026-11-06-mp7-lobby-hud.md → Task 3.</summary>
    public class RaceHudTextTests
    {
        [Test]
        public void LocalFlags_ShowsCountOutOfThree()
        {
            Assert.AreEqual("Bayrak 2/3", RaceHudText.LocalFlags(2));
        }

        [Test]
        public void Opponent_RaceShowsRoomAndFlags_CoopOnlyRoom_UnknownIsQuestionMark()
        {
            Assert.AreEqual("Rakip: Oda 7 · Bayrak 1/3", RaceHudText.Opponent(RaceMode.Race, 7, 1));
            Assert.AreEqual("Rakip: Oda 7", RaceHudText.Opponent(RaceMode.Coop, 7, 1));
            Assert.AreEqual("Rakip: ?", RaceHudText.Opponent(RaceMode.Race, 0, 0));
        }

        [Test]
        public void Countdown_RoundsUp_ThenGo()
        {
            Assert.AreEqual("3", RaceHudText.Countdown(3f));
            Assert.AreEqual("3", RaceHudText.Countdown(2.01f));
            Assert.AreEqual("2", RaceHudText.Countdown(2f));
            Assert.AreEqual("1", RaceHudText.Countdown(0.3f));
            Assert.AreEqual("BAŞLA!", RaceHudText.Countdown(0f));
        }

        [Test]
        public void LobbyStatus_ByPhaseAndRole()
        {
            Assert.AreEqual("Mod seç, host ol ya da katıl", RaceHudText.LobbyStatus(null, false, null));
            Assert.AreEqual("Rakip bekleniyor… (port 7777)", RaceHudText.LobbyStatus(LobbyPhase.Idle, true, null, 7777));
            Assert.AreEqual("Bağlanılıyor…", RaceHudText.LobbyStatus(LobbyPhase.Idle, false, null));
            Assert.AreEqual("Yükleniyor…", RaceHudText.LobbyStatus(LobbyPhase.Loading, false, null));
            Assert.AreEqual("Host'a ulaşılamadı", RaceHudText.LobbyStatus(LobbyPhase.Failed, false, "Host'a ulaşılamadı"));
        }

        [Test]
        public void Status_DescribesWaitingAndDisconnect()
        {
            Assert.AreEqual("Rakip yükleniyor…", RaceHudText.Status(LobbyPhase.WaitingForOpponent, TransportState.Connected, null));
            Assert.AreEqual("Bağlantı koptu", RaceHudText.Status(LobbyPhase.Racing, TransportState.Disconnected, null));
            Assert.AreEqual("Sürüm uyuşmuyor", RaceHudText.Status(LobbyPhase.Failed, TransportState.Disconnected, "Sürüm uyuşmuyor"));
            Assert.AreEqual("", RaceHudText.Status(LobbyPhase.Racing, TransportState.Connected, null));
        }
    }
}
