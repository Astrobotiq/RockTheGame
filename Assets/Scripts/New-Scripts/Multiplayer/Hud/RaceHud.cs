using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Flags;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
using New_Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Multiplayer.Hud
{
    /// <summary>
    /// Multiplayer HUD'u: sağ üstte bayrak sayacı ve rakip (oda, bayrak), üst ortada bağlantı durumu, ortada geri sayım.
    /// Kodla kurulan kendi overlay canvas'ı, UI katmanında; tek kişilik HUD'a dokunmaz.
    /// </summary>
    public sealed class RaceHud : MonoBehaviour
    {
        public const string ObjectName = "[MP HUD]";
        private const float GoTextSeconds = 1f;

        private MultiplayerSession _session;
        private TextMeshProUGUI _flags;
        private TextMeshProUGUI _opponent;
        private TextMeshProUGUI _status;
        private TextMeshProUGUI _countdown;
        private TextMeshProUGUI _timer;
        private TextMeshProUGUI _result;
        private MultiplayerUISettingsSO _settings;
        private RaceResultScreen _resultScreen;
        private RaceTrackView _track;
        private int _totalRooms;
        private float _nextTotalSearch;

        /// <summary>Bitiş ekranı (silindir/küp sütunları). Eski metin (<see cref="ResultText"/>) veri olarak durur, ekranda gösterilmez.</summary>
        public RaceResultScreen ResultScreen => _resultScreen;

        public string FlagsText => _flags.text;
        public string OpponentText => _opponent.text;
        public string StatusText => _status.text;
        public string CountdownText => _countdown.text;
        public string ResultText => _result.text;
        public string TimerText => _timer.text;

        public static RaceHud Create(MultiplayerSession session)
        {
            var go = new GameObject(ObjectName) { layer = LayerMask.NameToLayer("UI") };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            go.AddComponent<CanvasScaler>(); // ayarları UiPixelScale yapar (UR-3: tam sayı sanat pikseli)
            go.AddComponent<UiPixelScale>();

            var hud = go.AddComponent<RaceHud>();
            hud._session = session;
            hud._settings = MultiplayerUISettingsSO.Load();
            float size = hud._settings.InfoFontSize;
            hud._timer = hud.MakeText("Timer", new Vector2(1f, 1f), new Vector2(-40f, -40f), size, TextAlignmentOptions.TopRight);
            hud._flags = hud.MakeText("Flags", new Vector2(1f, 1f), new Vector2(-40f, -40f - size * 1.3f), size, TextAlignmentOptions.TopRight);
            hud._opponent = hud.MakeText("Opponent", new Vector2(1f, 1f), new Vector2(-40f, -40f - size * 2.6f), size, TextAlignmentOptions.TopRight);
            hud._result = hud.MakeText("Result", new Vector2(0.5f, 0.5f), Vector2.zero, size * 1.8f, TextAlignmentOptions.Center);
            ((RectTransform)hud._result.transform).sizeDelta = new Vector2(1600f, size * 1.8f * 4f);
            hud._status = hud.MakeText("Status", new Vector2(0.5f, 1f), new Vector2(0f, -40f), size, TextAlignmentOptions.Top);
            hud._countdown = hud.MakeText("Countdown", new Vector2(0.5f, 0.5f), Vector2.zero, hud._settings.CountdownFontSize, TextAlignmentOptions.Center);
            hud._status.color = hud._settings.WarningColor;
            hud._result.enabled = false;
            // Bayrak ve rakip metni artık pist şeridinde; metin veri olarak kalır, ekranda çizilmez.
            hud._flags.enabled = false;
            hud._opponent.enabled = false;
            hud._track = RaceTrackView.Create();
            hud._resultScreen = RaceResultScreen.Create();
            hud.Refresh();
            return hud;
        }

        private void OnDestroy()
        {
            if (_resultScreen != null) Destroy(_resultScreen.gameObject);
            if (_track != null) Destroy(_track.gameObject);
        }

        /// <summary>Ana yolun son odasının kimliği (bitiş işaretinin odası); sahnede yoksa 0. Bulununca saklanır.</summary>
        private int ResolveTotalRooms()
        {
            if (_totalRooms > 0 || Time.unscaledTime < _nextTotalSearch) return _totalRooms;
            _nextTotalSearch = Time.unscaledTime + 1f;
            var point = FindFirstObjectByType<RaceFinishPoint>(FindObjectsInactive.Include);
            var room = point != null ? point.GetComponentInParent<Room>(true) : null;
            if (room != null) _totalRooms = room.RoomId;
            return _totalRooms;
        }

        private void RefreshTrack(RaceSession race, LobbyPhase phase, bool localIsHost)
        {
            if (_track == null) return;
            if (race.LocalFinished)
            {
                _track.Hide();
                return;
            }

            _track.Present(new RaceTrackState
            {
                TotalRooms = ResolveTotalRooms(),
                LocalRoom = Mathf.Max(1, race.LocalRoomId),
                RivalRoom = race.OpponentRoomId,
                LocalFlags = race.LocalFlagCount,
                RivalFlags = race.OpponentFlagCount,
                LocalIsHost = localIsHost,
                ShowFlags = race.Mode == RaceMode.Race,
                Racing = phase == LobbyPhase.Racing && !race.OpponentFinished
            });
        }

        private TextMeshProUGUI MakeText(string name, Vector2 anchor, Vector2 position, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = gameObject.layer };
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(900f, fontSize * 1.4f);
            var text = go.AddComponent<TextMeshProUGUI>();
            if (_settings.Font != null) text.font = _settings.Font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = _settings.TextColor;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private void Update() => Refresh();

        public void Refresh()
        {
            var flow = _session != null ? _session.Flow : null;
            var race = flow?.Session;
            if (race == null)
            {
                _flags.text = _opponent.text = _countdown.text = _timer.text = _result.text = "";
                _status.text = "";
                if (_track != null) _track.Hide();
                return;
            }

            _flags.text = race.Mode == RaceMode.Race ? RaceHudText.LocalFlags(race.LocalFlagCount) : "";
            _opponent.text = RaceHudText.Opponent(race.Mode, race.OpponentRoomId, race.OpponentFlagCount);
            var transport = _session.Transport != null ? _session.Transport.State : TransportState.Disconnected;
            _status.text = _session.IsWaitingForReconnect
                ? RaceHudText.Reconnecting(_session.ReconnectSecondsLeft)
                : race.OpponentFinished && !race.LocalFinished
                    ? RaceHudText.OpponentFinished(race.OpponentElapsedSeconds)
                    : RaceHudText.Status(flow.Phase, transport, flow.FailureReason);
            _timer.text = flow.Phase == LobbyPhase.Racing ? RaceHudText.FormatTime(_session.ElapsedSeconds) : "";
            _result.text = RaceHudText.Result(race.Mode, race.LocalFinished, _session.ElapsedSeconds, race.OpponentFinished,
                race.OpponentElapsedSeconds, _session.CanReturnToLobby);
            RefreshResultScreen(race, flow.IsHost);
            RefreshTrack(race, flow.Phase, flow.IsHost);

            if (flow.Phase == LobbyPhase.Countdown)
                _countdown.text = RaceHudText.Countdown(flow.CountdownRemaining(Time.realtimeSinceStartupAsDouble));
            else if (flow.Phase == LobbyPhase.Racing && _session.RaceStartedAt >= 0f && Time.unscaledTime - _session.RaceStartedAt < GoTextSeconds)
                _countdown.text = RaceHudText.Countdown(0f);
            else
                _countdown.text = "";
        }

        private void RefreshResultScreen(RaceSession race, bool localIsFirst)
        {
            if (_resultScreen == null) return;
            if (!race.LocalFinished)
            {
                _resultScreen.Hide();
                return;
            }

            float local = _session.ElapsedSeconds;
            float opponent = race.OpponentElapsedSeconds;
            var kind = RaceResultMath.KindOf(race.Mode, race.OpponentFinished, local, opponent);
            RaceResultMath.Columns(race.Mode, localIsFirst, true, local, race.OpponentFinished, opponent, out var first, out var second);
            _resultScreen.Present(kind, first, second, _session.CanReturnToLobby);
            // Süre ve bayrak yazıları ekranın altında kalır; sonuç ekranı opak olduğundan ayrıca gizlenir.
            _timer.text = _flags.text = _opponent.text = "";
        }
    }
}
