using System;
using New_Scripts.Multiplayer.Transport;
using Steamworks;
using Steamworks.Data;
using SteamLobby = Steamworks.Data.Lobby;
using UnityEngine;

namespace New_Scripts.Multiplayer.Steam
{
    /// <summary>
    /// Steam ile arkadaş daveti. Host, iki kişilik ve yalnız arkadaşlara açık bir Steam lobisi kurar ve Steam'in davet
    /// arayüzünü açar. Davet kabul edilince (oyun açıksa OnGameLobbyJoinRequested, kapalıysa Steam oyunu
    /// "+connect_lobby id" ile başlatır) davet edilen lobiye girer, lobi sahibinin SteamID'si JoinRequested ile lobi
    /// ekranına verilir. Steam lobisi yalnız adresi taşır; bağlantı SteamTransport'un relay bağlantısıdır.
    /// Bağlantı kurulunca lobiden çıkılır.
    /// </summary>
    public sealed class SteamInviteService : IInviteService
    {
        public const uint AppId = 480; // Spacewar: Steam'in ücretsiz test uygulaması (kendi AppID'si alınana kadar)
        public const string ConnectLobbyArgument = "+connect_lobby";

        private SteamLobby? _lobby;
        private string _pendingJoin;

        public string DisplayName => "Steam";
        public bool IsAvailable => SteamClient.IsValid;
        public string LocalPlayerName => SteamClient.IsValid ? SteamClient.Name : "Oyuncu";

        public event Action<string> JoinRequested;

        public SteamInviteService()
        {
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;

            ulong lobbyId = ParseConnectLobby(Environment.GetCommandLineArgs());
            if (lobbyId != 0) JoinLobby(new SteamId { Value = lobbyId });
        }

        public static ulong ParseConnectLobby(string[] args)
        {
            if (args == null) return 0;
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], ConnectLobbyArgument, StringComparison.OrdinalIgnoreCase) &&
                    ulong.TryParse(args[i + 1], out ulong id))
                    return id;
            return 0;
        }

        public IRaceTransport CreateTransport() => new SteamTransport { ConnectedHook = LeaveLobby };

        public async void InviteFriends()
        {
            try
            {
                if (_lobby == null)
                {
                    SteamLobby? created = await SteamMatchmaking.CreateLobbyAsync(2);
                    if (!created.HasValue)
                    {
                        Debug.LogWarning("[MP] Steam lobisi kurulamadı");
                        return;
                    }
                    var lobby = created.Value;
                    lobby.SetFriendsOnly();
                    lobby.SetJoinable(true);
                    lobby.SetData("rtg-protocol", RaceProtocol.Version.ToString());
                    _lobby = lobby;
                }
                SteamFriends.OpenGameInviteOverlay(_lobby.Value.Id);
                Debug.Log("[MP] Steam davet arayüzü açıldı");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MP] Steam daveti açılamadı: {e.Message}");
            }
        }

        public void CloseInvites() => LeaveLobby();

        public string TakePendingJoin()
        {
            string join = _pendingJoin;
            _pendingJoin = null;
            return join;
        }

        private void OnGameLobbyJoinRequested(SteamLobby lobby, SteamId friend) => JoinLobby(lobby.Id);

        private async void JoinLobby(SteamId lobbyId)
        {
            try
            {
                SteamLobby? joined = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
                if (!joined.HasValue) Debug.LogWarning("[MP] Steam lobisine girilemedi");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MP] Steam lobisine girilemedi: {e.Message}");
            }
        }

        private void OnLobbyEntered(SteamLobby lobby)
        {
            if (lobby.Owner.Id == SteamClient.SteamId) return; // host kendi lobisine girdi
            _lobby = lobby;
            string hostAddress = lobby.Owner.Id.Value.ToString();
            Debug.Log($"[MP] Steam daveti kabul edildi: host {lobby.Owner.Name}");
            if (JoinRequested != null) JoinRequested.Invoke(hostAddress);
            else _pendingJoin = hostAddress;
        }

        private void LeaveLobby()
        {
            if (_lobby == null) return;
            try { _lobby.Value.Leave(); } catch (Exception) { }
            _lobby = null;
        }
    }
}
