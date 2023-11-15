using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;


public class LobbyManager : MonoBehaviour
{

    public static Action<Lobby> LobbyCreated;
    public static Action<string> StartingGame;
    public static Action<Lobby> JoinedLobby;
    public static Action<Lobby> LobbyUpdated;
    public static Action LeftLobby;
    public static Action LeftGame;

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_GAME_CODE = "JoinCode";

    [SerializeField] private RelayHandler _relayHandler;

    private Lobby _hostLobby;
    private Lobby _joinedLobby;
    public Lobby CurrentLobby { get { return _joinedLobby; } }

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    private string playerName;

    private void Awake()
    {
        playerName = "player" + UnityEngine.Random.Range(10, 99);
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "Lobby " + UnityEngine.Random.Range(10, 99);
            int maxPlayers = 5;
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, "Demo") },
                    { KEY_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
            _hostLobby = lobby;
            _joinedLobby = _hostLobby;
            LobbyCreated?.Invoke(_joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async Task<QueryResponse> ListLobbies()
    {
        try
        {
            QueryLobbiesOptions lobbyOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(lobbyOptions);
            return response;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            return null;
        }
    }

    public void JoinLobby(Lobby lobby)
    {
        JoinLobbyById(lobby.Id);
    }


    private async void JoinLobbyById(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions lobbyOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, lobbyOptions);
            _joinedLobby = lobby;
            JoinedLobby?.Invoke(_joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            _joinedLobby = lobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void MigrateLobbyHost(Player player)
    {
        try
        {
            if (_hostLobby == null) return;

            _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = player.Id
            });

            _joinedLobby = _hostLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            _hostLobby = null;
            _joinedLobby = null;
            LeftLobby?.Invoke();
            
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void RemovePlayer(Player player)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, player.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                }
            }
        };
    }

    public async void PollLobbyForUpdates()
    {
        if (_joinedLobby == null) return;

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0)
        {
            float lobbyUpdateTimerMax = 1.1f;
            lobbyUpdateTimer = lobbyUpdateTimerMax;
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
            _joinedLobby = lobby;
            LobbyUpdated?.Invoke(_joinedLobby);
        }

        if (_joinedLobby.Data[KEY_GAME_CODE].Value != "0")
        {
            if(!IsLobbyHost())
            {
                _relayHandler.JoinRelay(_joinedLobby.Data[KEY_GAME_CODE].Value);
                StartingGame?.Invoke(_joinedLobby.Name);
            }

            _joinedLobby = null;
            LobbyUpdated?.Invoke(_joinedLobby);
        }
    }

    private async void HeartBeat()
    {
        if (_hostLobby == null) return;

        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer < 0)
        {
            float heartbeatTimerMax = 15;
            heartbeatTimer = heartbeatTimerMax;
            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }
    }

    public async void StartGame()
    {
        try
        {
            if (!IsLobbyHost()) return;

            string relayCode = await _relayHandler.CreateRelay();
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        KEY_GAME_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode)
                    }
                }
            });

            StartingGame?.Invoke(_joinedLobby.Name);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
        LeftGame?.Invoke();

    }

    private bool IsLobbyHost()
    {
        return _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }


    private void Update()
    {
        HeartBeat();
        PollLobbyForUpdates();
    }
}
