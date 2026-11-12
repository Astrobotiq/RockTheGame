using System;
using System.IO;
using NUnit.Framework;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Save;
using New_Scripts.Multiplayer.Session;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Yarış kaydı: oturumdan yazılır, dosyadan okunur, oturuma geri yüklenir; 3 sn sessizlik ve 30 sn bekleme penceresi.
    /// docs/superpowers/plans/2026-11-06-mp8-save-resume.md → Task 1, 2, 4.
    /// </summary>
    public class RaceSaveTests
    {
        private static readonly Guid RaceId = new Guid("bbbbbbbb-0000-4000-8000-000000000008");
        private string _path;

        [SetUp]
        public void SetUp() => _path = Path.Combine(Path.GetTempPath(), $"rtg-race-{Guid.NewGuid():N}.json");

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }

        private static RaceSession SessionAtRoom7()
        {
            var session = new RaceSession(RaceMode.Race);
            session.OnLocalRoomEntered(5);
            session.OnLocalFlagReached(5);
            session.OnLocalRoomEntered(6);
            session.OnLocalFlagReached(6);
            session.OnLocalRoomEntered(7);
            session.Receive(new ProgressSyncMessage { RoomId = 4, FlagCount = 1 });
            return session;
        }

        [Test]
        public void RestoreLocal_SetsRoomCountAndCapturedFlags()
        {
            var session = new RaceSession(RaceMode.Race);

            session.RestoreLocal(7, 2, new[] { 5, 6 });

            Assert.AreEqual(7, session.LocalRoomId);
            Assert.AreEqual(2, session.LocalFlagCount);
            CollectionAssert.AreEquivalent(new[] { 5, 6 }, session.LocalCapturedRoomIds);
            Assert.IsEmpty(session.OnLocalFlagReached(6), "geri yüklenen bayrak tekrar sayılmamalı");
            Assert.AreEqual(2, session.OnLocalFlagReached(7).Count, "3. bayrak: FlagCaptured + Penalty");
        }

        [Test]
        public void SaveData_FromSession_CapturesEverything()
        {
            var data = RaceSaveData.From(SessionAtRoom7(), RaceId, "Rakip", 123.5f);

            Assert.AreEqual(RaceId, data.Id);
            Assert.AreEqual(RaceMode.Race, data.RaceMode);
            Assert.AreEqual(7, data.RoomId);
            Assert.AreEqual(2, data.FlagCount);
            CollectionAssert.AreEquivalent(new[] { 5, 6 }, data.CapturedRoomIds);
            Assert.AreEqual(123.5f, data.ElapsedSeconds, 1e-4f);
            Assert.AreEqual(4, data.OpponentRoomId);
            Assert.AreEqual("Rakip", data.OpponentName);
        }

        [Test]
        public void Store_WriteThenRead_RoundTrips()
        {
            var store = new RaceSaveStore(_path);
            store.Write(RaceSaveData.From(SessionAtRoom7(), RaceId, "Rakip", 50f));

            Assert.IsTrue(store.TryRead(out var data));
            Assert.AreEqual(RaceId, data.Id);
            Assert.AreEqual(7, data.RoomId);
            CollectionAssert.AreEquivalent(new[] { 5, 6 }, data.CapturedRoomIds);

            var restored = data.CreateSession();
            Assert.AreEqual(RaceMode.Race, restored.Mode);
            Assert.AreEqual(7, restored.LocalRoomId);
            Assert.AreEqual(2, restored.LocalFlagCount);
        }

        [Test]
        public void Store_EditorSaveOff_DoesNotAffectExplicitPath()
        {
            bool before = New_Scripts.Multiplayer.Dev.MultiplayerDevSettings.SaveEnabled;
            New_Scripts.Multiplayer.Dev.MultiplayerDevSettings.SaveEnabled = false;
            try
            {
                var store = new RaceSaveStore(_path);
                store.Write(RaceSaveData.From(SessionAtRoom7(), RaceId, "Rakip", 50f));
                Assert.IsTrue(store.Exists, "özel yola yazma menü ayarından etkilenmez (testler buna güvenir)");
            }
            finally
            {
                New_Scripts.Multiplayer.Dev.MultiplayerDevSettings.SaveEnabled = before;
            }
        }

        [Test]
        public void Store_MissingOrCorrupt_ReadsFalse_DeleteRemoves()
        {
            var store = new RaceSaveStore(_path);
            Assert.IsFalse(store.TryRead(out _));

            File.WriteAllText(_path, "{ bozuk json");
            Assert.IsFalse(store.TryRead(out _));

            store.Write(RaceSaveData.From(new RaceSession(RaceMode.Coop), RaceId, "B", 0f));
            Assert.IsTrue(store.Exists);
            store.Delete();
            Assert.IsFalse(store.Exists);
        }

        [Test]
        public void Watch_SilenceAfterThreeSeconds()
        {
            var watch = new ConnectionWatch();
            watch.OnPacket(10.0);

            Assert.IsFalse(watch.IsSilent(12.9));
            Assert.IsTrue(watch.IsSilent(13.1));
            watch.OnPacket(13.2);
            Assert.IsFalse(watch.IsSilent(13.3));
        }

        [Test]
        public void Watch_ReconnectWindow_ThirtySeconds()
        {
            var watch = new ConnectionWatch();
            Assert.IsFalse(watch.IsWaitingForReconnect);

            watch.BeginReconnectWait(100.0);

            Assert.IsTrue(watch.IsWaitingForReconnect);
            Assert.AreEqual(30, watch.SecondsLeft(100.0));
            Assert.AreEqual(1, watch.SecondsLeft(129.2));
            Assert.IsFalse(watch.HasExpired(129.9));
            Assert.IsTrue(watch.HasExpired(130.0));

            watch.EndReconnectWait();
            Assert.IsFalse(watch.IsWaitingForReconnect);
        }
    }
}
