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

        [Test]
        public void WaitingForRival_OnlyForHostWithoutOpponent()
        {
            Assert.IsTrue(LobbyScreenMath.WaitingForRival(true, true, false));
            Assert.IsFalse(LobbyScreenMath.WaitingForRival(true, true, true));
            Assert.IsFalse(LobbyScreenMath.WaitingForRival(true, false, false), "istemci beklemez, katılır");
            Assert.IsFalse(LobbyScreenMath.WaitingForRival(false, true, false), "bağlanmadan bekleme yok");
        }

        [Test]
        public void RivalArrived_FiresOnlyWhenHostWaitingAndOpponentComes()
        {
            Assert.IsTrue(LobbyScreenMath.RivalArrived(true, true, true, true));
            Assert.IsFalse(LobbyScreenMath.RivalArrived(false, true, true, true), "zaten gelmişti");
            Assert.IsFalse(LobbyScreenMath.RivalArrived(true, false, true, false), "vazgeçmek varış değil");
            Assert.IsFalse(LobbyScreenMath.RivalArrived(true, true, true, false), "hâlâ bekliyor");
            Assert.IsFalse(LobbyScreenMath.RivalArrived(true, true, false, true), "istemci için varış animasyonu yok");
        }

        [Test]
        public void RightSweep_GrowsFromDiagonalToRightEdge()
        {
            const float w = 480f, h = 270f, pad = 200f, topX = 296f, bottomX = 196f;
            Assert.IsEmpty(LobbyScreenMath.RightSweep(0f, w, h, pad, topX, bottomX));

            var full = LobbyScreenMath.RightSweep(1f, w, h, pad, topX, bottomX);
            Assert.AreEqual(4, full.Length);
            Assert.AreEqual(w + pad, MaxX(full), 1e-3f);

            float previous = 0f;
            for (float t = 0.1f; t <= 1.001f; t += 0.1f)
            {
                var pts = LobbyScreenMath.RightSweep(t, w, h, pad, topX, bottomX);
                float max = MaxX(pts);
                Assert.Greater(max, previous, $"t={t}: sağ kenar ilerlemeli");
                previous = max;
                foreach (var p in pts)
                {
                    float boundary = topX + (bottomX - topX) * p.y / h;
                    Assert.GreaterOrEqual(p.x, boundary - 1e-3f, $"t={t}: nokta çapraz sınırın solunda");
                }
            }
        }

        [Test]
        public void RightSweep_EarlyFrameIsTriangleOnTheDiagonal()
        {
            var pts = LobbyScreenMath.RightSweep(0.1f, 480f, 270f, 200f, 296f, 196f);
            Assert.AreEqual(3, pts.Length);
            Assert.AreEqual(pts[0].x, pts[1].x, 1e-3f, "sağ kenar dikey");
        }

        private static float MaxX(UnityEngine.Vector2[] points)
        {
            float max = float.MinValue;
            foreach (var p in points) max = UnityEngine.Mathf.Max(max, p.x);
            return max;
        }
    }
}
