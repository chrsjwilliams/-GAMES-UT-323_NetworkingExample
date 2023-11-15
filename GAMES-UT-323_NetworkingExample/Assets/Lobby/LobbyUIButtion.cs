using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;

public class LobbyUIButtion : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyName;
    [SerializeField] TextMeshProUGUI gameMode;
    [SerializeField] TextMeshProUGUI playerCount;

    private Lobby _lobby;

    public void InitLobbyButtonUI(Lobby lobby)
    {
        _lobby = lobby;
        OnUpdateLobbyInfo(lobby);
    }


    private void OnUpdateLobbyInfo(Lobby lobby)
    {
        lobbyName.text = lobby.Name;
        gameMode.text = lobby.Data[LobbyManager.KEY_GAME_MODE].Value;
        playerCount.text = lobby.Players.Count + " / " + lobby.MaxPlayers;

    }

    public void JoinLobby()
    {
        Services.LobbyManager.JoinLobby(_lobby);
        
    }
}
