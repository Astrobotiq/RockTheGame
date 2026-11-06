using System;
using System.Collections.Generic;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Multiplayer.Save;

namespace New_Scripts.Multiplayer.Session
{
    public enum LobbyPhase
    {
        Idle,
        Handshake,
        Loading,
        WaitingForOpponent,
        Countdown,
        Racing,
        Failed
    }

    /// <summary>
    /// Bağlantıdan yarışın başlamasına kadar olan akış (ağ ve Unity bilmez):
    /// bağlanınca Hello; sürüm uyuşmazsa Failed; modu host belirler (istemcinin RaceSession'ı host'un Hello'sundan kurulur);
    /// iki Hello görülünce ProgressSync ve LoadSceneRequested; sahne hazır olunca Ready; iki Ready görülünce
    /// CountdownSeconds geri sayım, bitince RaceStarted. Diğer mesajlar RaceSession'a gider.
    /// Metotlar karşı tarafa yollanacak mesajları döndürür.
    /// </summary>
    public sealed class RaceLobbyFlow
    {
        public const float CountdownSeconds = 3f;

        private readonly string _playerName;
        private readonly RaceMode _hostMode;
        private readonly RaceSaveData _resume;
        private readonly RaceSession _existingSession;
        private bool _helloSent;
        private bool _helloReceived;
        private bool _sceneReady;
        private bool _localReady;
        private bool _remoteReady;
        private double _goAt;

        public bool IsHost { get; }
        /// <summary>Kayıtlı yarışa devam ya da yarış içinde yeniden bağlanma.</summary>
        public bool IsResume { get; }
        /// <summary>Yarış kimliği. Yeni yarışta istemci host'un kimliğini Hello'dan alır.</summary>
        public Guid RaceId { get; private set; }
        public LobbyPhase Phase { get; private set; } = LobbyPhase.Idle;
        /// <summary>Host'ta baştan var; istemcide host'un Hello'su gelince kurulur.</summary>
        public RaceSession Session { get; private set; }
        public string FailureReason { get; private set; }
        public string OpponentName { get; private set; }

        public event Action LoadSceneRequested;
        public event Action CountdownStarted;
        public event Action RaceStarted;
        public event Action<string> Failed;

        /// <param name="resume">Lobide "Devam et": kimlik, mod ve yerel durum kayıttan.</param>
        /// <param name="existingSession">Yarış içinde kopup yeniden bağlanma: var olan oturum aynen sürer.</param>
        public RaceLobbyFlow(bool isHost, RaceMode hostMode, string playerName, Guid raceId,
            RaceSaveData resume = null, RaceSession existingSession = null)
        {
            IsHost = isHost;
            _playerName = playerName;
            _resume = resume;
            _existingSession = existingSession;
            IsResume = resume != null || existingSession != null;
            _hostMode = existingSession?.Mode ?? resume?.RaceMode ?? hostMode;
            RaceId = resume != null ? resume.Id : (isHost || IsResume ? raceId : Guid.Empty);
            if (isHost || existingSession != null) Session = CreateLocalSession();
        }

        private RaceSession CreateLocalSession() =>
            _existingSession ?? _resume?.CreateSession() ?? new RaceSession(_hostMode);

        public IReadOnlyList<IRaceMessage> OnConnected()
        {
            if (Phase == LobbyPhase.Failed || _helloSent) return Array.Empty<IRaceMessage>();
            Phase = LobbyPhase.Handshake;
            _helloSent = true;
            var sent = new List<IRaceMessage>
            {
                new HelloMessage { ProtocolVersion = RaceProtocol.Version, Mode = _hostMode, RaceId = RaceId, PlayerName = _playerName, Resume = IsResume }
            };
            TryCompleteHandshake(sent);
            return sent;
        }

        public IReadOnlyList<IRaceMessage> Receive(IRaceMessage message, double now)
        {
            if (Phase == LobbyPhase.Failed || message == null) return Array.Empty<IRaceMessage>();
            var sent = new List<IRaceMessage>();

            switch (message)
            {
                case HelloMessage hello:
                    if (_helloReceived) break;
                    if (hello.ProtocolVersion != RaceProtocol.Version)
                    {
                        Fail($"Sürüm uyuşmuyor: rakip v{hello.ProtocolVersion}, bu oyun v{RaceProtocol.Version}");
                        return Array.Empty<IRaceMessage>();
                    }
                    string mismatch = ResumeMismatch(hello);
                    if (mismatch != null)
                    {
                        Fail(mismatch);
                        return Array.Empty<IRaceMessage>();
                    }
                    _helloReceived = true;
                    OpponentName = hello.PlayerName;
                    if (!IsHost)
                    {
                        if (!IsResume) RaceId = hello.RaceId;
                        Session ??= IsResume ? CreateLocalSession() : new RaceSession(hello.Mode);
                    }
                    TryCompleteHandshake(sent);
                    break;
                case ReadyMessage _:
                    _remoteReady = true;
                    TryStartCountdown(now);
                    break;
                default:
                    Session?.Receive(message);
                    break;
            }
            return sent;
        }

        public IReadOnlyList<IRaceMessage> OnSceneReady(double now)
        {
            _sceneReady = true;
            var sent = new List<IRaceMessage>();
            TrySendReady(sent, now);
            return sent;
        }

        public void Tick(double now)
        {
            if (Phase != LobbyPhase.Countdown || now < _goAt) return;
            Phase = LobbyPhase.Racing;
            RaceStarted?.Invoke();
        }

        public float CountdownRemaining(double now) =>
            Phase == LobbyPhase.Countdown ? (float)Math.Max(0, _goAt - now) : 0f;

        public void Fail(string reason)
        {
            if (Phase == LobbyPhase.Failed) return;
            Phase = LobbyPhase.Failed;
            FailureReason = reason;
            Failed?.Invoke(reason);
        }

        /// <summary>İki taraf aynı şeyi istemeli: ikisi de yeni yarış ya da ikisi de aynı kayıtlı yarış.</summary>
        private string ResumeMismatch(HelloMessage hello)
        {
            if (IsResume && !hello.Resume)
                return "Rakip yeni bir yarış başlatıyor; kayıtlı yarış için ikiniz de Devam et'i seçin";
            if (!IsResume && hello.Resume)
                return "Rakip kayıtlı bir yarışa devam etmek istiyor; ikiniz de Devam et'i ya da yeni yarışı seçin";
            if (IsResume && hello.RaceId != RaceId)
                return "Kayıtlar farklı yarışlara ait; bu ikiliyle devam edilemez";
            return null;
        }

        private void TryCompleteHandshake(List<IRaceMessage> sent)
        {
            if (!_helloSent || !_helloReceived || Phase != LobbyPhase.Handshake) return;
            Phase = LobbyPhase.Loading;
            sent.Add(Session.CreateProgressSync(0f));
            LoadSceneRequested?.Invoke();
            // Sahne önceden yüklüyse (geliştirici başlatıcısı) OnSceneReady çağrılmış olabilir.
            TrySendReady(sent, double.NaN);
        }

        private void TrySendReady(List<IRaceMessage> sent, double now)
        {
            if (!_sceneReady || _localReady || Phase != LobbyPhase.Loading) return;
            _localReady = true;
            Phase = LobbyPhase.WaitingForOpponent;
            sent.Add(new ReadyMessage());
            if (!double.IsNaN(now)) TryStartCountdown(now);
        }

        private void TryStartCountdown(double now)
        {
            if (!_localReady || !_remoteReady || Phase != LobbyPhase.WaitingForOpponent || double.IsNaN(now)) return;
            Phase = LobbyPhase.Countdown;
            _goAt = now + CountdownSeconds;
            CountdownStarted?.Invoke();
        }
    }
}
