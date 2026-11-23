using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using New_Scripts.Audio;
using New_Scripts.Collectible;
using New_Scripts.Door;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Ghost;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Penalty;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
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
        // Balon cezası: kalkış 0,35 + yolculuk 2,5–5 + kapıda kaydırma 0,5 + bekleme 0,3 + düşüş ≈ 9 sn'ye kadar.
        private const float Timeout = 15f;
        private const float LandSeconds = 1.5f;

        private PlayerController _player;
        private RoomManager _roomManager;
        private PenaltyHandler _handler;
        private RaceSession _session;
        private int _applied;
        private GameObject _broadcasterObject;

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
            if (_broadcasterObject != null) Object.Destroy(_broadcasterObject);
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
            if (_applied < 0) yield break;

            // Balon checkpoint'in biraz üstünde patlar; oyuncunun zemine inmesini bekle.
            Vector2 checkpoint = RoomById(_applied).InitialCheckpoint.transform.position;
            float landEnd = Time.realtimeSinceStartup + LandSeconds;
            while (Vector2.Distance(checkpoint, _player.transform.position) >= 0.5f && Time.realtimeSinceStartup < landEnd)
                yield return null;
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

        // ---- Balon cezası (BB3): docs/multiplayer-ceza-balonu-plani.md — İNSAN ONAYI BEKLİYOR (CLAUDE.md kural 3) ----

        private IEnumerator WaitUntilBubbleFlying()
        {
            float end = Time.realtimeSinceStartup + Timeout;
            while ((_handler == null || !_handler.IsApplying || _player.PlayerCollider.enabled) && Time.realtimeSinceStartup < end)
                yield return null;
            Assert.IsFalse(_player.PlayerCollider.enabled, "balon başlamadı");
        }

        [UnityTest]
        public IEnumerator Bubble_WhileFlying_PlayerIsFrozenAndColliderOff_ThenBothRestoredAfterPop()
        {
            yield return StartIn(3);

            _handler.Enqueue();
            yield return WaitUntilBubbleFlying();
            Assert.IsTrue(_player.IsPaused, "balondayken oyuncu donuk");
            Assert.IsNotNull(Object.FindFirstObjectByType<PenaltyBubbleView>(), "balon görünür olmalı");

            yield return WaitApplied();
            Assert.AreEqual(2, _applied);
            Assert.IsTrue(_player.PlayerCollider.enabled, "patlamadan sonra collider açık");
            Assert.IsFalse(_player.IsPaused, "patlamadan sonra oyuncu serbest");
            yield return new WaitForSeconds(0.7f); // patlama halkası 0,4 sn sürer
            Assert.IsNull(Object.FindFirstObjectByType<PenaltyBubbleView>(), "balon patlayıp kaybolmalı");
        }

        [UnityTest]
        public IEnumerator Bubble_FlightPose_TiltsThePlayerInFlight_AndEndsUpright()
        {
            yield return StartIn(3);
            float maxTilt = 0f;

            _handler.Enqueue();
            float end = Time.realtimeSinceStartup + Timeout;
            while (_applied < 0 && Time.realtimeSinceStartup < end)
            {
                maxTilt = Mathf.Max(maxTilt, Mathf.Abs(Mathf.DeltaAngle(0f, _player.transform.eulerAngles.z)));
                yield return null;
            }

            Assert.AreEqual(2, _applied);
            Assert.Greater(maxTilt, 45f, "uçarken oyuncu gidiş yönüne yatmalı (başlangıç değeri 70°)");
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, _player.transform.eulerAngles.z), 0.01f, "patlayınca oyuncu dik olmalı");
        }

        [UnityTest]
        public IEnumerator Bubble_Cancelled_RestoresUprightPose()
        {
            yield return StartIn(3);

            _handler.Enqueue();
            yield return WaitUntilBubbleFlying();
            yield return new WaitForSeconds(0.6f); // yatması için
            Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(0f, _player.transform.eulerAngles.z)), 20f, "ön koşul: balondayken yatık");
            Object.Destroy(_handler.gameObject);
            _handler = null;
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, _player.transform.eulerAngles.z), 0.01f, "iptalde oyuncu dik kalmalı");
        }

        [UnityTest]
        public IEnumerator Bubble_ChangesRoomAtTheDoor_BeforeItArrives()
        {
            yield return StartIn(3);
            int appliedWhenEntered = -2;
            RoomById(2).OnRoomEntered.AddListener(() => appliedWhenEntered = _applied);

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(-1, appliedWhenEntered, "oda 2'ye geçiş, balon varıp patlamadan önce kapıda olmalı");
        }

        [UnityTest]
        public IEnumerator Bubble_FlyingThroughHazard_DoesNotKillThePlayer()
        {
            yield return StartIn(3);
            int deaths = 0;
            var health = _player.GetComponent<New_Scripts.Death.PlayerHealth>();
            System.Action onDeath = () => deaths++;
            health.OnDeath += onDeath;

            _handler.Enqueue();
            yield return WaitUntilBubbleFlying();

            // Kapının tam üstüne ölümcül bir alan: balon buradan geçer.
            Assert.IsTrue(PenaltyHandler.TryFindDoor(RoomById(3), RoomById(2), _player.transform.position, out Vector2 door));
            var zone = new GameObject("TestDeathZone");
            zone.transform.position = door;
            var zoneCollider = zone.AddComponent<BoxCollider2D>();
            zoneCollider.size = new Vector2(4f, 4f);
            zoneCollider.isTrigger = true;
            zone.AddComponent<New_Scripts.Death.DeathZone>();
            Physics2D.SyncTransforms();

            bool passedThrough = false;
            float end = Time.realtimeSinceStartup + Timeout;
            while (_applied < 0 && Time.realtimeSinceStartup < end)
            {
                passedThrough |= zoneCollider.OverlapPoint(_player.transform.position);
                yield return null;
            }
            health.OnDeath -= onDeath;
            Object.Destroy(zone);

            Assert.AreEqual(2, _applied);
            Assert.IsTrue(passedThrough, "ön koşul: balon ölümcül alanın içinden geçmeli");
            Assert.AreEqual(0, deaths, "balon tehlikenin içinden geçerken oyuncu ölmemeli");
        }

        [UnityTest]
        public IEnumerator Bubble_OpponentSnapshots_CarryInBubble_AndNeverTeleport()
        {
            yield return StartIn(13); // en geniş oda: en uzun yol, en yüksek hız
            var (host, client) = InMemoryTransport.CreatePair();
            host.Host();
            client.Join("");
            for (int i = 0; i < 3; i++) { host.Poll(); client.Poll(); }
            Assert.AreEqual(TransportState.Connected, host.State);

            var received = new List<SnapshotMessage>();
            client.MessageReceived += bytes =>
            {
                if (RaceMessageCodec.TryDecode(bytes, out var message) && message is SnapshotMessage snapshot) received.Add(snapshot);
            };
            var broadcaster = LocalPlayerBroadcaster.Create(_player, _session, host);
            _broadcasterObject = broadcaster.gameObject;
            broadcaster.InBubbleSource = () => _handler.IsInBubble;

            _handler.Enqueue();
            float end = Time.realtimeSinceStartup + Timeout;
            while (_applied < 0 && Time.realtimeSinceStartup < end)
            {
                client.Poll();
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            client.Poll();

            var inBubble = received.FindAll(s => s.InBubble);
            Assert.AreEqual(12, _applied);
            Assert.Greater(inBubble.Count, 40, "balon süresince (≥3 sn × 20/sn) InBubble'lı snapshot gelmeli");
            Assert.IsEmpty(inBubble.FindAll(s => s.Teleported), "balondaki hızlı süzülme ışınlanma sayılmamalı");
            Assert.IsFalse(received[received.Count - 1].InBubble, "patlayınca bit kapanmalı");
            Assert.IsFalse(received[received.Count - 1].Teleported, "patlama anında konum sıçramamalı");
        }

        [UnityTest]
        public IEnumerator Bubble_Sounds_InflateThenLoopThenPop_AndLoopIsStopped()
        {
            var settings = PenaltyBubbleSettingsSO.Load();
            var played = new List<AudioCueSO>();
            var stopped = new List<AudioCueSO>();
            System.Action<AudioCueSO, AudioCuePlayParams> onPlay = (cue, _) => played.Add(cue);
            System.Action<AudioCueSO> onStop = cue => stopped.Add(cue);
            settings.sfxPlayChannel.OnPlayRequested += onPlay;
            settings.sfxStopChannel.OnStopRequested += onStop;

            yield return StartIn(3);
            played.Clear();
            stopped.Clear();
            _handler.Enqueue();
            yield return WaitApplied();
            settings.sfxPlayChannel.OnPlayRequested -= onPlay;
            settings.sfxStopChannel.OnStopRequested -= onStop;

            CollectionAssert.AreEqual(new[] { settings.inflateCue, settings.travelCue, settings.popCue }, played,
                "sıra: şişme, yolculuk döngüsü, patlama");
            CollectionAssert.AreEqual(new[] { settings.travelCue }, stopped, "yolculuk döngüsü varışta durmalı");
        }

        [UnityTest]
        public IEnumerator Bubble_Cancelled_StopsTheTravelLoop()
        {
            var settings = PenaltyBubbleSettingsSO.Load();
            var stopped = new List<AudioCueSO>();
            System.Action<AudioCueSO> onStop = cue => stopped.Add(cue);

            yield return StartIn(3);
            _handler.Enqueue();
            // Uğultu yolculukla başlar; kalkıştan sonra çalıyor olmalı.
            yield return WaitUntilBubbleFlying();
            yield return new WaitForSeconds(_handlerLiftWait);
            settings.sfxStopChannel.OnStopRequested += onStop;
            Object.Destroy(_handler.gameObject);
            _handler = null;
            yield return new WaitForSeconds(0.5f);
            settings.sfxStopChannel.OnStopRequested -= onStop;

            CollectionAssert.AreEqual(new[] { settings.travelCue }, stopped, "iptalde uğultu durmalı");
        }

        private const float _handlerLiftWait = 0.6f; // kalkış 0,35 sn

        [UnityTest]
        public IEnumerator Bubble_Cancelled_ReleasesPlayerAndRestoresCollider()
        {
            yield return StartIn(3);

            _handler.Enqueue();
            yield return WaitUntilBubbleFlying();
            Object.Destroy(_handler.gameObject);
            _handler = null;
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(_player.IsPaused, "iptalde oyuncu serbest kalmalı");
            Assert.IsTrue(_player.PlayerCollider.enabled, "iptalde collider geri açılmalı");
            Assert.IsNull(Object.FindFirstObjectByType<PenaltyBubbleView>(), "iptalde balon yok edilmeli");
        }

        // ---- Ceza sonrası oda sıfırlama (MPF1): docs/multiplayer-ceza-kapi-ve-platform-plani.md "Sorun 1" ----

        private static T Field<T>(object target, string name) =>
            (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

        private static RoomKeyManager[] ManagersOf(int roomId) => RoomById(roomId).GetComponentsInChildren<RoomKeyManager>(true);

        private static RoomDoor DoorOf(int roomId) => RoomById(roomId).GetComponentInChildren<RoomDoor>(true);

        private static RoomKeyManager ManagerWith(int roomId, KeyActionType action)
        {
            foreach (var m in ManagersOf(roomId))
                if (Field<KeyActionType>(m, "actionType") == action) return m;
            Assert.Fail($"Oda {roomId}'de {action} anahtar yöneticisi yok");
            return null;
        }

        private void CollectKeys(RoomKeyManager manager)
        {
            var collect = typeof(Collectible).GetMethod("Collect", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var key in manager.Keys) collect.Invoke(key, new object[] { _player });
        }

        private static void AssertRoomKeysFresh(int roomId)
        {
            foreach (var m in ManagersOf(roomId))
                foreach (var key in m.Keys)
                {
                    Assert.IsFalse(key.IsCollected, $"oda {roomId}: anahtar toplanmamış olmalı");
                    Assert.IsFalse(key.IsCommitted, $"oda {roomId}: anahtar kalıcı olmamalı");
                }
        }

        [UnityTest]
        public IEnumerator Penalty_Room11_KeysAndDoorReturnToFirstEntry()
        {
            // Önce oyuncu 12'de: oda 11 komşu olarak uyanık, anahtarlar checkpoint kanalına bağlı (uyuyan oda Start çalıştırmaz).
            yield return StartIn(12);
            RoomStateReset.ResetRoom(RoomById(11));
            var door = DoorOf(11);
            var doorCollider = Field<Collider2D>(door, "doorCollider");
            var openKey = ManagerWith(11, KeyActionType.Open);
            var closeKey = ManagerWith(11, KeyActionType.Close);

            // 11'in iki anahtarı alınır (kapı açılır sonra kapanır), 12'ye geçişte checkpoint hepsini kalıcı yapar.
            CollectKeys(openKey);
            Assert.IsTrue(door.IsOpen, "açan anahtardan sonra kapı açık");
            CollectKeys(closeKey);
            Assert.IsFalse(door.IsOpen, "kapatan anahtardan sonra kapı kapalı");
            RoomById(11).InitialCheckpoint.ActivateCheckpoint();
            Assert.IsTrue(openKey.Keys[0].IsCommitted, "ön koşul: anahtar checkpoint ile kalıcı olmuş");

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(11, _applied);
            AssertAtCheckpointOf(11);
            Assert.IsFalse(door.IsOpen, "kapı ilk girişteki gibi kapalı");
            Assert.IsTrue(doorCollider.enabled, "kapalı kapının collider'ı açık");
            AssertRoomKeysFresh(11);

            // Oyuncu kapıyı yeniden açabilmeli.
            CollectKeys(openKey);
            Assert.IsTrue(door.IsOpen, "açan anahtar yeniden alınınca kapı açılır");
        }

        [UnityTest]
        public IEnumerator Penalty_Room8_OpenedDoorClosesAgain()
        {
            yield return StartIn(9);
            RoomStateReset.ResetRoom(RoomById(8));
            var door = DoorOf(8);
            var openKey = ManagerWith(8, KeyActionType.Open);

            CollectKeys(openKey);
            Assert.IsTrue(door.IsOpen, "ön koşul: anahtarlarla kapı açılmış");
            RoomById(8).InitialCheckpoint.ActivateCheckpoint();

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(8, _applied);
            AssertAtCheckpointOf(8);
            Assert.IsFalse(door.IsOpen, "9 → 8 cezasından sonra kapı yeniden kapalı");
            AssertRoomKeysFresh(8);

            // Oyuncu üç anahtar parçasını yeniden toplayınca kapı açılmalı (playtest bildirimi: açılmıyordu).
            CollectKeys(openKey);
            Assert.IsTrue(door.IsOpen, "üç parça yeniden toplanınca kapı açılır");
        }

        [UnityTest]
        public IEnumerator Penalty_Room8_KeysCollectedByWalkingOpenTheDoor()
        {
            // Gerçek yol: oyuncu parçaların üstüne yürür, tetik collider'ı toplar (reflection yok).
            yield return StartIn(9);
            RoomStateReset.ResetRoom(RoomById(8));
            var door = DoorOf(8);
            var openKey = ManagerWith(8, KeyActionType.Open);
            CollectKeys(openKey);
            RoomById(8).InitialCheckpoint.ActivateCheckpoint();

            _handler.Enqueue();
            yield return WaitApplied();
            Assert.AreEqual(8, _applied);
            Assert.IsFalse(door.IsOpen);

            foreach (var key in openKey.Keys)
            {
                _player.transform.position = new Vector3(key.transform.position.x, key.transform.position.y, _player.transform.position.z);
                Physics2D.SyncTransforms();
                for (int i = 0; i < 4; i++) yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(door.IsOpen, "üç parçaya yürüyünce kapı açılır");

            // Görsel de açılmalı (DOTween dizisi); playtest "açılmıyor" dedi.
            var visual = door.GetComponentInParent<DoorJuicyVisual>() ?? door.GetComponentInChildren<DoorJuicyVisual>(true);
            Assert.IsNotNull(visual, "kapının DoorJuicyVisual'ı bulunamadı");
            var target = Field<Transform>(visual, "graphicTarget");
            yield return new WaitForSeconds(1f);
            Vector3 expectedOpen = new Vector3(Field<float>(visual, "openTargetX"),
                Field<float>(visual, "openTargetY"), 0f);
            Assert.Greater(Vector2.Distance(target.localPosition, Field<Vector3>(visual, "_initialLocalPosition")), 0.5f * expectedOpen.magnitude,
                "kapı görseli açık konuma gitmeli");
        }

        [UnityTest]
        public IEnumerator Penalty_9To8_ReopensRoom9DoorThatClosedBehindThePlayer()
        {
            // Oda 9'a girince 9'un kapısı (startOpen) arkandan kapanır ve oda 8'in kapısıyla aynı geçitte durur.
            // Ceza oyuncuyu 8'e atınca bu kapı kapalı kalıp geçidi tıkıyordu (playtest 24 Kasım).
            yield return StartIn(9);
            var door9 = DoorOf(9);
            Assert.IsFalse(door9.IsOpen, "ön koşul: 9'a girince kapısı kapanır");
            var door9Collider = Field<Collider2D>(door9, "doorCollider");
            Assert.IsTrue(door9Collider.enabled);

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(8, _applied);
            Assert.IsTrue(door9.IsOpen, "ayrılınan odanın kapısı ilk girişteki (açık) hâline döner");
            Assert.IsFalse(door9Collider.enabled, "açık kapı geçidi tıkamaz");
        }

        [UnityTest]
        public IEnumerator Penalty_ResetsTheLeftRoomToo()
        {
            // 8'den 7'ye ceza: hedef değil ayrılınan oda 8'in anahtarları ve kapısı sıfırlanır (oyuncu 8'e yeniden "ilk kez" girer).
            yield return StartIn(8);
            RoomStateReset.ResetRoom(RoomById(8));
            var door = DoorOf(8);
            CollectKeys(ManagerWith(8, KeyActionType.Open));
            RoomById(8).InitialCheckpoint.ActivateCheckpoint();
            Assert.IsTrue(door.IsOpen, "ön koşul: oda 8 kapısı açık");

            _handler.Enqueue();
            yield return WaitApplied();

            Assert.AreEqual(7, _applied);
            Assert.IsFalse(door.IsOpen, "ayrılınan odanın kapısı da kapanır");
            AssertRoomKeysFresh(8);
        }
    }
}
