using System.Collections;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Penalty;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Ceza gerçek sahnede: oyuncu bir önceki odanın başlangıç checkpoint'ine gider, oda o odaya geçer; yan odada (22)
    /// öncesi 11; ilk odada işlemez; oyuncu ölüyken bekler. docs/superpowers/plans/2026-11-06-mp6-penalty.md → Task 3.
    /// </summary>
    public class PenaltyFlowTests
    {
        private const float Timeout = 6f;

        private PlayerController _player;
        private RoomManager _roomManager;
        private PenaltyHandler _handler;
        private RaceSession _session;
        private int _applied;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (Object.FindFirstObjectByType<PlayerController>() == null)
            {
                SceneManager.LoadScene("NewScene");
                yield return null;
                yield return new WaitForSeconds(0.5f);
            }

            _player = Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            _roomManager = Object.FindFirstObjectByType<RoomManager>(FindObjectsInactive.Include);
            Assert.IsNotNull(_player);
            Assert.IsNotNull(_roomManager);
            _applied = -1;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_handler != null) Object.Destroy(_handler.gameObject);
            if (!_player.gameObject.activeSelf) _player.gameObject.SetActive(true);
            yield return null;
        }

        private static Room RoomById(int id)
        {
            foreach (var room in Object.FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (room.RoomId == id) return room;
            Assert.Fail($"Oda {id} sahnede yok");
            return null;
        }

        private IEnumerator StartIn(int roomId)
        {
            var room = RoomById(roomId);
            _roomManager.TransitionToRoom(room);
            _player.OnStartRespawn();
            _player.transform.position = room.InitialCheckpoint.transform.position;
            _player.OnEndRespawn();
            yield return null;

            _session = new RaceSession(RaceMode.Race);
            _session.OnLocalRoomEntered(roomId);
            _handler = PenaltyHandler.Create(_session, _player);
            _handler.PenaltyApplied += id => _applied = id;
        }

        private IEnumerator WaitApplied()
        {
            float end = Time.realtimeSinceStartup + Timeout;
            while (_applied < 0 && Time.realtimeSinceStartup < end) yield return null;
        }

        private void AssertAtCheckpointOf(int roomId)
        {
            Vector2 expected = RoomById(roomId).InitialCheckpoint.transform.position;
            Assert.Less(Vector2.Distance(expected, _player.transform.position), 0.5f,
                $"oyuncu oda {roomId}'nin checkpoint'inde olmalı");
            Assert.IsFalse(_player.IsPaused, "ceza bitince oyuncu serbest");
        }

        [UnityTest]
        public IEnumerator Penalty_MainPath_GoesToPreviousRoomCheckpoint()
        {
            yield return StartIn(3);
            bool entered = false;
            RoomById(2).OnRoomEntered.AddListener(() => entered = true);

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(2, _applied);
            AssertAtCheckpointOf(2);
            Assert.IsTrue(entered, "oda 2 güncel oda olmalı (OnRoomEntered)");
        }

        [UnityTest]
        public IEnumerator AfterPenalty_DeathRespawnsAtTargetCheckpoint()
        {
            yield return StartIn(7);
            _handler.Enqueue();
            yield return WaitApplied();
            Assert.AreEqual(6, _applied);

            _player.GetComponent<New_Scripts.Death.PlayerHealth>().Kill();
            float end = Time.realtimeSinceStartup + Timeout;
            while ((!_player.gameObject.activeSelf || _player.IsPaused) && Time.realtimeSinceStartup < end) yield return null;

            AssertAtCheckpointOf(6);
        }

        [UnityTest]
        public IEnumerator Penalty_SideRoom22_GoesToRoom11()
        {
            yield return StartIn(22);

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(11, _applied);
            AssertAtCheckpointOf(11);
        }

        [UnityTest]
        public IEnumerator Penalty_FirstRoom_DoesNothing()
        {
            yield return StartIn(1);

            _handler.Enqueue();
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(-1, _applied);
            Assert.AreEqual(0, _handler.Pending);
            // Oyuncu checkpoint'ten zemine düşebilir; önemli olan odada kalması.
            Assert.IsTrue(RoomById(1).RoomBounds.OverlapPoint(_player.transform.position), "oyuncu oda 1'de kalmalı");
            Assert.IsFalse(_player.IsPaused);
        }

        [UnityTest]
        public IEnumerator Penalty_WhilePlayerDead_WaitsUntilRespawn()
        {
            yield return StartIn(5);
            _player.gameObject.SetActive(false);

            _handler.Enqueue();
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(-1, _applied, "oyuncu ölüyken ceza beklemeli");
            Assert.AreEqual(1, _handler.Pending);

            _player.gameObject.SetActive(true);
            yield return WaitApplied();

            Assert.AreEqual(4, _applied);
            AssertAtCheckpointOf(4);
        }
    }
}
