using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{

    public const string PLAYER_NAME_KEY = "PlayerName";
    public const string GAME_MODE_KEY = "GameMode";

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    private string playerName;

    private void Awake()
    {
        playerName = "player" + UnityEngine.Random.Range(10, 99);
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "Test Lobby";
            int maxPlayers = 5;
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        GAME_MODE_KEY, new DataObject(DataObject.VisibilityOptions.Public, "Demo")
                    }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void ListLobbies()
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
            Debug.Log("Lobbies found: " + response.Results.Count);

            foreach (Lobby lobby in response.Results)
            {
                Debug.Log(lobby.Name + " | Max Players: " + lobby.MaxPlayers);

            }
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public void JoinLobby(Lobby lobby)
    {
        JoinLobbyByCode(lobby.LobbyCode);
    }


    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions lobbyOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, lobbyOptions);
            joinedLobby = lobby;

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
            joinedLobby = lobby;

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
            if (hostLobby == null) return;

            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = player.Id
            });

            joinedLobby = hostLobby;
            ListPlayers(hostLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
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
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, player.Id);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private void ListPlayers()
    {
        ListPlayers(joinedLobby);
    }

    private void ListPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name + "| Game Mode: " + lobby.Data[GAME_MODE_KEY].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Data[PLAYER_NAME_KEY].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            if (hostLobby == null) return;

            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        GAME_MODE_KEY, new DataObject(DataObject.VisibilityOptions.Public, gameMode)
                    }
                }
            });

            joinedLobby = hostLobby;
            ListPlayers(hostLobby);
        }
        catch(LobbyServiceException ex)
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
                    PLAYER_NAME_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                }
            }
        };
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {

                {
                    PLAYER_NAME_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)
                }
            }
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private async void PollLobbyForUpdates()
    {
        if (joinedLobby == null) return;

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0)
        {
            float lobbyUpdateTimerMax = 1.1f;
            lobbyUpdateTimer = lobbyUpdateTimerMax;
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;
        }
    }

    private async void HeartBeat()
    {
        if (hostLobby == null) return;

        heartbeatTimer -= Time.deltaTime;
        if(heartbeatTimer < 0)
        {
            float heartbeatTimerMax = 15;
            heartbeatTimer = heartbeatTimerMax;
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    private void Update()
    {
        HeartBeat();
        PollLobbyForUpdates();
    }
}
