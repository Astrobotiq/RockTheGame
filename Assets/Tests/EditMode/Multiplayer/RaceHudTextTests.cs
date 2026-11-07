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
        public void Reconnecting_AndResumeLabel()
        {
            Assert.AreEqual("Rakip bağlantısı koptu, bekleniyor… (27)", RaceHudText.Reconnecting(27));
            Assert.AreEqual("Devam et: Açık (Oda 7, Bayrak 2/3)", RaceHudText.ResumeLabel(true, 7, 2));
            Assert.AreEqual("Devam et: Kapalı", RaceHudText.ResumeLabel(false, 7, 2));
        }

        [Test]
        public void MultiplayerEntry_OnlyWithArgument()
        {
            Assert.IsTrue(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.HasArgument(new[] { "RockTheGame.exe", "-multiplayer" }));
            Assert.IsTrue(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.HasArgument(new[] { "x", "-MULTIPLAYER" }));
            Assert.IsFalse(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.HasArgument(new[] { "RockTheGame.exe" }));
            Assert.IsFalse(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.HasArgument(null));
            Assert.IsTrue(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.HasArgument(new[] { "x", "-mp-host" }));
            Assert.AreEqual("100.64.0.7", New_Scripts.Multiplayer.Lobby.MultiplayerEntry.JoinAddress(new[] { "x", "-mp-join", "100.64.0.7" }));
            Assert.IsNull(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.JoinAddress(new[] { "x", "-mp-join" }));
            Assert.IsTrue(New_Scripts.Multiplayer.Lobby.MultiplayerEntry.HasArgument(new[] { "x", "+connect_lobby", "109775241234" }), "Steam davetiyle açılış lobiye gider");
        }

        [Test]
        public void FormatTime_MinutesSecondsTenths()
        {
            Assert.AreEqual("00:00.0", RaceHudText.FormatTime(0f));
            Assert.AreEqual("05:12.3", RaceHudText.FormatTime(312.34f));
            Assert.AreEqual("61:01.0", RaceHudText.FormatTime(3661f));
        }

        [Test]
        public void Result_ByFinishStateAndMode()
        {
            Assert.AreEqual("", RaceHudText.Result(RaceMode.Race, false, 0f, false, 0f, false));
            Assert.AreEqual("Bitirdin! 05:12.3\nRakip bekleniyor…", RaceHudText.Result(RaceMode.Race, true, 312.34f, false, 0f, false));
            Assert.AreEqual("Kazandın! 05:12.3\nRakip: 05:40.0", RaceHudText.Result(RaceMode.Race, true, 312.34f, true, 340f, false));
            Assert.AreEqual("Kaybettin. 05:40.0\nRakip: 05:12.3", RaceHudText.Result(RaceMode.Race, true, 340f, true, 312.34f, false));
            Assert.AreEqual("Berabere! 05:00.0\nRakip: 05:00.0", RaceHudText.Result(RaceMode.Race, true, 300f, true, 300f, false));
            Assert.AreEqual("Birlikte bitirdiniz! 05:12.3\nRakip: 05:40.0", RaceHudText.Result(RaceMode.Coop, true, 312.34f, true, 340f, false));
            StringAssert.EndsWith("A: Lobiye dön", RaceHudText.Result(RaceMode.Race, true, 312.34f, false, 0f, true));
        }

        [Test]
        public void OpponentFinishedStatus()
        {
            Assert.AreEqual("Rakip bitirdi: 05:40.0", RaceHudText.OpponentFinished(340f));
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
