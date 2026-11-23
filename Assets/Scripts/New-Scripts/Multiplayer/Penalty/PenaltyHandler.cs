using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Audio;
using New_Scripts.Death;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Rakibin cezasını uygular: yerel oyuncu bir balonla bir önceki odanın başlangıç checkpoint'ine taşınır
    /// (docs/multiplayer-ceza-balonu-plani.md). Dizi: kalkış + şişme → kapıya doğru yolculuk → kapıda sıfırlama, kamera
    /// kaydırması ve oda geçişi → varış, bekleme, patlama. Balon boyunca oyuncunun collider'ı kapalıdır (tehlikeler ve
    /// oda kapıları dokunmaz); checkpoint varışta etkinleşir, sonraki ölüm oraya döner.
    /// Oyuncu ölü, duraklatılmış ya da oda geçişindeyken ceza bekler; hedef uygulama anındaki odadan hesaplanır.
    /// Tek kişilik kodu yalnız çağırır, değiştirmez.
    /// </summary>
    public sealed class PenaltyHandler : MonoBehaviour
    {
        public const string ObjectName = "[MP Penalty]";

        private RaceSession _session;
        private PlayerController _player;
        private RoomManager _roomManager;
        private PenaltyBubbleSettingsSO _settings;
        private readonly List<Room> _rooms = new List<Room>();
        private CancellationTokenSource _cts;

        public int Pending { get; private set; }
        public bool IsApplying { get; private set; }
        /// <summary>
        /// Oyuncu şu an balonda (kalkıştan patlamaya). <c>LocalPlayerBroadcaster</c> rakibe bildirir (snapshot InBubble biti).
        /// </summary>
        public bool IsInBubble { get; private set; }

        /// <summary>Uygulanan her cezadan sonra; argüman gidilen oda.</summary>
        public event Action<int> PenaltyApplied;

        public static bool IsPlayerBusy(bool active, bool paused, bool inTransition) => !active || paused || inTransition;

        public static PenaltyHandler Create(RaceSession session, PlayerController player)
        {
            var handler = new GameObject(ObjectName).AddComponent<PenaltyHandler>();
            handler._session = session;
            handler._player = player;
            handler._settings = PenaltyBubbleSettingsSO.Load();
            handler._roomManager = FindFirstObjectByType<RoomManager>(FindObjectsInactive.Include);
            handler._rooms.AddRange(FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            return handler;
        }

        public void Enqueue() => Pending++;

        private void OnEnable() => _cts = new CancellationTokenSource();

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void Update()
        {
            if (Pending == 0 || IsApplying || _player == null) return;
            // Pause menüsü açıkken (timeScale 0) ceza uygulanmaz; menü kapanınca sürer.
            if (IsPlayerBusy(_player.gameObject.activeInHierarchy, _player.IsPaused || New_Scripts.Pause.PauseManager.IsPaused,
                    _player.CurrentState is PlayerTransitionState)) return;

            Pending--;
            Room current = FindRoom(_session.LocalRoomId);
            var neighbors = new List<int>();
            if (current != null && current.NeighborRooms != null)
                foreach (var n in current.NeighborRooms) if (n != null) neighbors.Add(n.RoomId);

            if (!_session.TryGetPenaltyTarget(neighbors, out int targetId))
            {
                Debug.Log("[MP] Ceza işlemedi (ilk oda, birlikte modu ya da yarış bitti)");
                return;
            }

            Room target = FindRoom(targetId);
            if (target == null)
            {
                Debug.LogWarning($"[MP] Ceza hedefi oda {targetId} sahnede yok");
                return;
            }

            ApplyAsync(target, current, _cts.Token).Forget();
        }

        private async UniTaskVoid ApplyAsync(Room target, Room left, CancellationToken token)
        {
            IsApplying = true;
            PenaltyBubbleView bubble = null;
            bool colliderWasEnabled = _player.PlayerCollider != null && _player.PlayerCollider.enabled;
            try
            {
                Debug.Log($"[MP] Ceza: oda {_session.LocalRoomId} → {target.RoomId} (balon)");
                _player.OnStartRespawn();
                // Balon boyunca tehlike, checkpoint ve oda kapısı tetikleyicileri oyuncuya dokunmasın.
                if (_player.PlayerCollider != null) _player.PlayerCollider.enabled = false;

                bubble = PenaltyBubbleView.Create(_settings.bubbleRadius, _player.SpriteRenderer, _settings.bubbleMaterial);
                IsInBubble = true;
                Vector2 origin = _player.transform.position;
                PlayCue(_settings.inflateCue);
                Vibrate(_settings.vibration != null ? _settings.vibration.InflateProfile : default);

                // Yol baştan bilinir: kalkışta uçma pozu gidiş yönüne yatmaya başlasın.
                Vector2 lifted = origin + Vector2.up * _settings.liftHeight;
                Vector2 arrival = ArrivalPoint(target);
                bool hasDoor = TryFindDoor(left, target, lifted, out Vector2 door);
                if (!hasDoor)
                {
                    // Sahne düzeni hatası: düz çizgi; oda geçişi yolun ortasında yapılır.
                    Debug.LogWarning($"[MP] Ceza: oda {left?.RoomId} → {target.RoomId} arası geçiş kapısı bulunamadı, düz çizgi");
                    door = Vector2.Lerp(lifted, arrival, 0.5f);
                }
                var path = PenaltyBubblePath.Build(lifted, door, arrival);
                float duration = _settings.TravelSeconds(path.Length);
                _poseAngle = 0f;
                _poseTarget = 0f;
                _speed = 0f;
                _lastPosition = origin;

                // 1–2. Kalkış ve şişme (üst üste).
                float riseSeconds = Mathf.Max(_settings.liftSeconds, _settings.inflateSeconds);
                float elapsed = 0f;
                while (elapsed < riseSeconds)
                {
                    elapsed += Time.deltaTime;
                    Vector2 p = origin + Vector2.up * PenaltyBubbleMath.LiftOffset(Progress(elapsed, _settings.liftSeconds), _settings.liftHeight);
                    Fly(bubble, p, PenaltyBubbleMath.InflateScale(Progress(elapsed, _settings.inflateSeconds), _settings.inflateOvershoot),
                        path.DirectionAt(0f), leaning: true);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    ThrowIfPlayerGone();
                }

                // 3. Yolculuk.
                StartTravelLoop();
                elapsed = 0f;
                bool crossed = false;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float u = Mathf.Clamp01(elapsed / duration);
                    float along = PenaltyBubbleMath.Ease(u);
                    Vector2 direction = path.DirectionAt(along);
                    Vector2 p = path.PointAt(along)
                                + PenaltyBubbleMath.Wobble(direction, u, duration, _settings.wobbleAmplitude, _settings.wobbleHz);
                    Fly(bubble, p, 1f, direction, leaning: true);

                    if (!crossed && PenaltyBubbleMath.HasPassedDoor(path, u))
                    {
                        crossed = true;
                        await CrossDoorAsync(target, left, door, token);
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    ThrowIfPlayerGone();
                }
                if (!crossed) await CrossDoorAsync(target, left, door, token);

                // 4. Varış: kısa bekleme (poz dikleşir), patlama, düşüş.
                elapsed = 0f;
                while (elapsed < _settings.arriveHoldSeconds)
                {
                    elapsed += Time.deltaTime;
                    Fly(bubble, arrival, 1f, path.DirectionAt(1f), leaning: false);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    ThrowIfPlayerGone();
                }

                bubble.Pop();
                bubble = null;
                IsInBubble = false;
                StopTravelLoop();
                PlayCue(_settings.popCue);
                Vibrate(_settings.vibration != null ? _settings.vibration.PopProfile : default);
                if (target.InitialCheckpoint != null) target.InitialCheckpoint.ActivateCheckpoint();
                ApplyPose(0f);
                if (_player.PlayerCollider != null) _player.PlayerCollider.enabled = colliderWasEnabled;
                _player.OnEndRespawn();
                PenaltyApplied?.Invoke(target.RoomId);
            }
            catch (OperationCanceledException)
            {
                // Başlatıcı kapandı, sahne değişti ya da oyuncu gitti; oyuncu kaldıysa serbest bırakılır.
                if (_player != null) ApplyPose(0f);
                if (_player != null && _player.gameObject.activeInHierarchy) _player.OnEndRespawn();
            }
            finally
            {
                if (bubble != null) Destroy(bubble.gameObject);
                IsInBubble = false;
                StopTravelLoop(); // iptalde uğultu kalmasın
                if (_player != null) ApplyPose(0f);
                if (_player != null && _player.PlayerCollider != null) _player.PlayerCollider.enabled = colliderWasEnabled;
                IsApplying = false;
            }
        }

        /// <summary>
        /// Kapıda normal oda geçişinin aynısı: sıfırlamalar, kamera kaydırması, oda değişimi. Balon bu sürede kapıda bekler.
        /// Sıfırlamalar oyuncu hem hedef odaya hem ayrıldığı odaya yeniden "ilk kez" girerken yapılır, checkpoint henüz
        /// etkin değilken (hiçbir şey kalıcı olmadan).
        /// </summary>
        private async UniTask CrossDoorAsync(Room target, Room left, Vector2 door, CancellationToken token)
        {
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.ResetAll();
            RoomStateReset.ResetRoom(target);
            RoomStateReset.ResetRoom(left);

            if (CameraController.Instance is ICameraTransitionHandler camera && target.RoomBounds != null)
            {
                camera.PrepareForTransition();
                try
                {
                    await camera.PanAndZoomCameraAsync(door, target.OverrideCameraSize, target.OverrideDynamicZoom,
                        target.RoomBounds, _settings.doorPanSeconds, token);
                }
                finally
                {
                    // İptalde bile kamera geçiş kilidinde kalmasın.
                    camera.FinalizeTransition(target.RoomBounds, target.OverrideCameraSize, target.OverrideDynamicZoom);
                }
            }

            // SetAsCurrent → Room.OnRoomEntered → yayıncı RoomEntered'ı yollar, RaceSession'ın odası güncellenir.
            if (_roomManager != null) _roomManager.TransitionToRoom(target);
        }

        /// <summary>
        /// İki oda arasındaki geçişin (RoomTransitionTrigger) merkezi; birden çoksa <paramref name="from"/>'a en yakını.
        /// Geçişler çoğunlukla tek yönlü kurulu: 4 → 5 kapısı yalnız oda 4'te durur. Bu yüzden 5 → 4 cezasında kapı,
        /// ayrılınan odada hedefe açılan tetikleyici ya da hedefte ayrılınan odaya açılan tetikleyicidir.
        /// </summary>
        public static bool TryFindDoor(Room left, Room target, Vector2 from, out Vector2 door)
        {
            door = default;
            if (left == null || target == null) return false;

            float best = float.MaxValue;
            bool found = false;
            Consider(left, target, from, ref best, ref door, ref found);
            Consider(target, left, from, ref best, ref door, ref found);
            return found;
        }

        // owner odasının altındaki, goesTo odasına açılan tetikleyicilerden from'a en yakınını seçer.
        private static void Consider(Room owner, Room goesTo, Vector2 from, ref float best, ref Vector2 door, ref bool found)
        {
            foreach (var trigger in owner.GetComponentsInChildren<RoomTransitionTrigger>(true))
            {
                if (trigger.TargetRoom != goesTo) continue;
                float d = ((Vector2)trigger.transform.position - from).sqrMagnitude;
                if (d >= best) continue;
                best = d;
                door = trigger.transform.position;
                found = true;
            }
        }

        private Vector2 ArrivalPoint(Room target)
        {
            Vector3 spawn = target.InitialCheckpoint != null ? target.InitialCheckpoint.transform.position : target.transform.position;
            return new Vector2(spawn.x, spawn.y + _settings.arriveHeight);
        }

        // Uçma pozu (BB4): Animator'da böyle bir durum yok; bütün görsel (gövde, gözler, kollar) birlikte yatsın diye
        // oyuncu kökü z ekseninde döndürülür. Dizi her çıkışta açıyı sıfırlar (ApplyPose(0)).
        private float _poseAngle;
        private float _poseTarget;
        private float _speed;
        private Vector2 _lastPosition;

        /// <summary>Bir karelik uçuş: oyuncuyu ve balonu konuma koyar, pozu yatırır, balonu hıza göre esnetir.</summary>
        private void Fly(PenaltyBubbleView bubble, Vector2 position, float scale, Vector2 direction, bool leaning)
        {
            float dt = Time.deltaTime;
            if (dt > 0f)
            {
                _speed = Mathf.Lerp(_speed, (position - _lastPosition).magnitude / dt, 0.3f);
                _poseTarget = leaning ? PenaltyBubbleMath.TargetPoseAngle(direction.x, _settings.poseAngleDegrees, _poseTarget) : 0f;
                float rate = _settings.poseAngleDegrees / Mathf.Max(0.05f, _settings.liftSeconds); // kalkışta tam yatar
                _poseAngle = Mathf.MoveTowards(_poseAngle, _poseTarget, rate * dt);
            }
            _lastPosition = position;

            MovePlayer(position);
            ApplyPose(_poseAngle);
            bubble.Follow(position, scale, direction,
                PenaltyBubbleMath.StretchAmount(_speed, _settings.stretchReferenceSpeed, _settings.maxStretch));
        }

        private void ApplyPose(float degrees)
        {
            _player.transform.rotation = Quaternion.Euler(0f, 0f, degrees);
            if (_player.PlayerRigidbody != null) _player.PlayerRigidbody.rotation = degrees;
        }

        // Kamera takip hedefi PlayerRigidbody.position'ı okur; transform ataması fizik adımına kadar ona yansımaz, ikisi de yazılır.
        private void MovePlayer(Vector2 position)
        {
            _player.transform.position = new Vector3(position.x, position.y, _player.transform.position.z);
            if (_player.PlayerRigidbody != null) _player.PlayerRigidbody.position = position;
        }

        // Ses ve titreşim eksik atanmışsa sessizce atlanır (EditMode testi atamayı doğrular).
        private void PlayCue(AudioCueSO cue)
        {
            if (cue != null && _settings.sfxPlayChannel != null) _settings.sfxPlayChannel.RaisePlayEvent(cue);
        }

        private static void Vibrate(RumbleProfile profile)
        {
            if (profile.duration > 0f && HapticManager.Instance != null) HapticManager.Instance.Vibrate(profile);
        }

        private bool _travelLoopOn;

        private void StartTravelLoop()
        {
            if (_travelLoopOn || _settings.travelCue == null || _settings.sfxPlayChannel == null) return;
            _travelLoopOn = true;
            _settings.sfxPlayChannel.RaisePlayEvent(_settings.travelCue);
        }

        private void StopTravelLoop()
        {
            if (!_travelLoopOn) return;
            _travelLoopOn = false;
            if (_settings.sfxStopChannel != null) _settings.sfxStopChannel.RaiseStopEvent(_settings.travelCue);
        }

        private static float Progress(float elapsed, float seconds) => seconds <= 0f ? 1f : Mathf.Clamp01(elapsed / seconds);

        // Balon sırasında oyuncu yok edildiyse ya da kapandıysa (ör. sahne yeniden yüklendi) diziyi bırakır.
        private void ThrowIfPlayerGone()
        {
            if (_player == null || !_player.gameObject.activeInHierarchy) throw new OperationCanceledException();
        }

        private Room FindRoom(int id) => _rooms.Find(r => r != null && r.RoomId == id);
    }
}
