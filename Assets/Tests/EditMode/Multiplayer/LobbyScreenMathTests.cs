using NUnit.Framework;
using New_Scripts.Multiplayer.Lobby;
using New_Scripts.Multiplayer.Messages;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>Lobi ekranının (düzen B) saf kuralları (docs/systems/multiplayer.md "Lobi ekranı").</summary>
    public class LobbyScreenMathTests
    {
        [Test]
        public void Move_PicksSideFromStick_AndKeepsItInDeadzone()
        {
            Assert.AreEqual(LobbySide.Host, LobbyScreenMath.Move(LobbySide.Join, -0.9f));
            Assert.AreEqual(LobbySide.Join, LobbyScreenMath.Move(LobbySide.Host, 0.9f));
            Assert.AreEqual(LobbySide.Join, LobbyScreenMath.Move(LobbySide.Join, 0.2f));
            Assert.AreEqual(LobbySide.Host, LobbyScreenMath.Move(LobbySide.Host, -0.2f));
            Assert.AreEqual(LobbySide.Join, LobbyScreenMath.Move(LobbySide.Host, LobbyScreenMath.StickDeadzone));
        }

        [Test]
        public void ModeName_IsLowercaseEnglish()
        {
            Assert.AreEqual("race", LobbyScreenMath.ModeName(RaceMode.Race));
            Assert.AreEqual("co-op", LobbyScreenMath.ModeName(RaceMode.Coop));
        }

        [Test]
        public void Captions_FollowConnectionState()
        {
            Assert.AreEqual("START A ROOM", LobbyScreenMath.HostCaption(false, false, false));
            Assert.AreEqual("INVITE SENT", LobbyScreenMath.HostCaption(true, true, true));
            Assert.AreEqual("ROOM OPEN", LobbyScreenMath.HostCaption(true, true, false));
            Assert.AreEqual("", LobbyScreenMath.HostCaption(true, false, false));

            Assert.AreEqual("ENTER A ROOM", LobbyScreenMath.JoinCaption(false, false, false, false));
            Assert.AreEqual("ACCEPT AN INVITE", LobbyScreenMath.JoinCaption(false, false, false, true));
            Assert.AreEqual("WAITING FOR P2", LobbyScreenMath.JoinCaption(true, true, false, false));
            Assert.AreEqual("RIVAL", LobbyScreenMath.JoinCaption(true, true, true, false));
            Assert.AreEqual("JOINING", LobbyScreenMath.JoinCaption(true, false, false, false));
        }

        [Test]
        public void CanConfirm_BlockedWhileConnecting_AndJoinNeedsIpMode()
        {
            Assert.IsTrue(LobbyScreenMath.CanConfirm(false, LobbySide.Host, true));
            Assert.IsTrue(LobbyScreenMath.CanConfirm(false, LobbySide.Join, false));
            Assert.IsFalse(LobbyScreenMath.CanConfirm(false, LobbySide.Join, true));
            Assert.IsFalse(LobbyScreenMath.CanConfirm(true, LobbySide.Host, false));
            Assert.IsFalse(LobbyScreenMath.CanConfirm(true, LobbySide.Join, false));
        }

        [Test]
        public void Hints_ShowConnectionAndResume()
        {
            Assert.AreEqual("Y  CONNECTION: IP", LobbyScreenMath.ConnectionHint(false, "Steam"));
            Assert.AreEqual("Y  CONNECTION: STEAM", LobbyScreenMath.ConnectionHint(true, "Steam"));
            Assert.AreEqual("X  CONTINUE: OFF", LobbyScreenMath.ResumeHint(false, 8, 2));
            Assert.AreEqual("X  CONTINUE: ROOM 8  FLAG 2/3", LobbyScreenMath.ResumeHint(true, 8, 2));
        }
    }
}
