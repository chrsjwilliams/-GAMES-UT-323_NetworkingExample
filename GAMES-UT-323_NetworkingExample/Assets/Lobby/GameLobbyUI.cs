using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class GameLobbyUI : MonoBehaviour
{
    [SerializeField] CanvasGroup lobbyListUI;
    
    [SerializeField] LobbyUIButtion _lobbyUIButtonPrefab;
    [SerializeField] Transform lobbyButtonParent;
    [SerializeField] CanvasGroup startGameButton;
    [SerializeField] CanvasGroup leaveLobbyButton;
    [SerializeField] TextMeshProUGUI playerCount;

    [SerializeField] CanvasGroup lobbyButtons;
    private List<LobbyUIButtion> _activeLobbies = new List<LobbyUIButtion>();


    bool joinedLobby = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        startGameButton.alpha = 0;
        startGameButton.interactable = false;
        startGameButton.blocksRaycasts = false;

        leaveLobbyButton.alpha = 0;
        leaveLobbyButton.interactable = false;
        leaveLobbyButton.blocksRaycasts = false;

        LobbyManager.LobbyCreated += OnLobbyCreated;
        LobbyManager.JoinedLobby += OnJoinedLobby;
        LobbyManager.LeftLobby += OnLeft;
        LobbyManager.LeftGame += OnLeft;
        NetworkPlayer.PlayerSpawned += OnPlayerSpawned;

    }



    private void OnDisable()
    {
        LobbyManager.LobbyCreated -= OnLobbyCreated;
        LobbyManager.JoinedLobby -= OnJoinedLobby;
        LobbyManager.LeftLobby -= OnLeft;
        LobbyManager.LeftGame -= OnLeft;
        NetworkPlayer.PlayerSpawned -= OnPlayerSpawned;

    }

    private void OnPlayerSpawned()
    {
        lobbyListUI.alpha = 0;
        lobbyListUI.blocksRaycasts = false;
        lobbyListUI.interactable = false;

        lobbyButtons.alpha = 0;
        lobbyButtons.interactable = false;
        lobbyButtons.blocksRaycasts = false;
        joinedLobby = false;
    }

    public void OnJoinedLobby()
    {
        leaveLobbyButton.alpha = 1;
        leaveLobbyButton.interactable = true;
        leaveLobbyButton.blocksRaycasts = true;
        joinedLobby = true;
    }

    public void OnLobbyCreated(Lobby lobby)
    {
        joinedLobby = true;

        startGameButton.alpha = 1;
        startGameButton.interactable = true;
        startGameButton.blocksRaycasts = true;

        leaveLobbyButton.alpha = 1;
        leaveLobbyButton.interactable = true;
        leaveLobbyButton.blocksRaycasts = true;


        lobbyListUI.alpha = 0;
        lobbyListUI.blocksRaycasts = false;
        lobbyListUI.interactable = false;

        lobbyButtons.alpha = 0;
        lobbyButtons.interactable = false;
        lobbyButtons.blocksRaycasts = false;
    }

    private void OnLeft()
    {
        lobbyListUI.alpha = 1;
        lobbyListUI.blocksRaycasts = true;
        lobbyListUI.interactable = true;

        lobbyButtons.alpha = 1;
        lobbyButtons.interactable = true;
        lobbyButtons.blocksRaycasts = true;
    }

    private void Start()
    {
       RefreshLobbies();
    }

    public async void RefreshLobbies()
    {
        var result = await Services.LobbyManager.ListLobbies();
        if (result == null || result.Results == null) return;

        RemoveAllLobbyButtons();

        foreach(Lobby lobby in result.Results)
        {
            LobbyUIButtion lobbyButton = Instantiate(_lobbyUIButtonPrefab, lobbyButtonParent);
            lobbyButton.InitLobbyButtonUI(lobby);
        }
    }

    private void RemoveAllLobbyButtons()
    {
        foreach(LobbyUIButtion button in _activeLobbies)
        {
            Destroy(button.gameObject);
        }
    }

    public void StartGame()
    {
        Services.LobbyManager.StartGame();
        startGameButton.alpha = 0;
        startGameButton.interactable = false;
        startGameButton.blocksRaycasts = false;

        leaveLobbyButton.alpha = 0;
        leaveLobbyButton.interactable = false;
        leaveLobbyButton.blocksRaycasts = false;
        joinedLobby = false;
    }

    private void Update()
    {
        if (!joinedLobby) return;

        playerCount.text = "Players: " + Services.LobbyManager.CurrentLobby.Players.Count + " / " + Services.LobbyManager.CurrentLobby.MaxPlayers;
    }
}
