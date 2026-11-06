using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Session;
using New_Scripts.Multiplayer.Transport;
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
        private MultiplayerUISettingsSO _settings;

        public string FlagsText => _flags.text;
        public string OpponentText => _opponent.text;
        public string StatusText => _status.text;
        public string CountdownText => _countdown.text;

        public static RaceHud Create(MultiplayerSession session)
        {
            var go = new GameObject(ObjectName) { layer = LayerMask.NameToLayer("UI") };
            if (Application.isPlaying) DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var hud = go.AddComponent<RaceHud>();
            hud._session = session;
            hud._settings = MultiplayerUISettingsSO.Load();
            float size = hud._settings.InfoFontSize;
            hud._flags = hud.MakeText("Flags", new Vector2(1f, 1f), new Vector2(-40f, -40f), size, TextAlignmentOptions.TopRight);
            hud._opponent = hud.MakeText("Opponent", new Vector2(1f, 1f), new Vector2(-40f, -40f - size * 1.3f), size, TextAlignmentOptions.TopRight);
            hud._status = hud.MakeText("Status", new Vector2(0.5f, 1f), new Vector2(0f, -40f), size, TextAlignmentOptions.Top);
            hud._countdown = hud.MakeText("Countdown", new Vector2(0.5f, 0.5f), Vector2.zero, hud._settings.CountdownFontSize, TextAlignmentOptions.Center);
            hud._status.color = hud._settings.WarningColor;
            hud.Refresh();
            return hud;
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
                _flags.text = _opponent.text = _countdown.text = "";
                _status.text = "";
                return;
            }

            _flags.text = race.Mode == RaceMode.Race ? RaceHudText.LocalFlags(race.LocalFlagCount) : "";
            _opponent.text = RaceHudText.Opponent(race.Mode, race.OpponentRoomId, race.OpponentFlagCount);
            var transport = _session.Transport != null ? _session.Transport.State : TransportState.Disconnected;
            _status.text = _session.IsWaitingForReconnect
                ? RaceHudText.Reconnecting(_session.ReconnectSecondsLeft)
                : RaceHudText.Status(flow.Phase, transport, flow.FailureReason);

            if (flow.Phase == LobbyPhase.Countdown)
                _countdown.text = RaceHudText.Countdown(flow.CountdownRemaining(Time.realtimeSinceStartupAsDouble));
            else if (flow.Phase == LobbyPhase.Racing && _session.RaceStartedAt >= 0f && Time.unscaledTime - _session.RaceStartedAt < GoTextSeconds)
                _countdown.text = RaceHudText.Countdown(0f);
            else
                _countdown.text = "";
        }
    }
}
