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
    [SerializeField] TextMeshProUGUI lobbyTitle;
    [SerializeField] LobbyUIButtion _lobbyUIButtonPrefab;
    [SerializeField] Transform lobbyButtonParent;
    [SerializeField] CanvasGroup startGameButton;
    [SerializeField] CanvasGroup leaveLobbyButton;
    [SerializeField] TextMeshProUGUI playerCount;

    [SerializeField] CanvasGroup lobbyButtons;
    private List<LobbyUIButtion> _activeLobbies = new List<LobbyUIButtion>();

    Lobby currentLobby;

    // Start is called before the first frame update
    void OnEnable()
    {
        lobbyTitle.text = "Lobby List";
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
        LobbyManager.LobbyUpdated += OnLobbyUpdated;
        NetworkPlayer.PlayerSpawned += OnPlayerSpawned;
        GAMES_UT323.Networking.UnityAuthentication.UnityAuthCompleted += OnAuth;

    }



    private void OnDisable()
    {
        LobbyManager.LobbyCreated -= OnLobbyCreated;
        LobbyManager.JoinedLobby -= OnJoinedLobby;
        LobbyManager.LeftLobby -= OnLeft;
        LobbyManager.LeftGame -= OnLeft;
        LobbyManager.LobbyUpdated -= OnLobbyUpdated;
        NetworkPlayer.PlayerSpawned -= OnPlayerSpawned;
        GAMES_UT323.Networking.UnityAuthentication.UnityAuthCompleted -= OnAuth;

    }

    public void OnAuth()
    {
        RefreshLobbies();
    }

    private void OnLobbyUpdated(Lobby lobby)
    {
        if (lobby == null) return;

        lobbyTitle.text = lobby.Name;
        currentLobby = lobby;
    }

    private void OnPlayerSpawned()
    {
        leaveLobbyButton.alpha = 0;
        leaveLobbyButton.interactable = false;
        leaveLobbyButton.blocksRaycasts = false;

        lobbyListUI.alpha = 0;
        lobbyListUI.blocksRaycasts = false;
        lobbyListUI.interactable = false;

        lobbyButtons.alpha = 0;
        lobbyButtons.interactable = false;
        lobbyButtons.blocksRaycasts = false;
    }

    public void OnJoinedLobby(Lobby lobby)
    {
        lobbyTitle.text = lobby.Name;
        currentLobby = lobby;
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

    public void OnLobbyCreated(Lobby lobby)
    {
        lobbyTitle.text = lobby.Name;
        currentLobby = lobby;

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
        lobbyTitle.text = "Lobby List";

        playerCount.text = "";

        lobbyListUI.alpha = 1;
        lobbyListUI.blocksRaycasts = true;
        lobbyListUI.interactable = true;

        lobbyButtons.alpha = 1;
        lobbyButtons.interactable = true;
        lobbyButtons.blocksRaycasts = true;

        startGameButton.alpha = 0;
        startGameButton.interactable = false;
        startGameButton.blocksRaycasts = false;

        leaveLobbyButton.alpha = 0;
        leaveLobbyButton.interactable = false;
        leaveLobbyButton.blocksRaycasts = false;
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
        playerCount.text = "";

        startGameButton.alpha = 0;
        startGameButton.interactable = false;
        startGameButton.blocksRaycasts = false;

        leaveLobbyButton.alpha = 0;
        leaveLobbyButton.interactable = false;
        leaveLobbyButton.blocksRaycasts = false;
    }

    private void Update()
    {
        if (currentLobby == null) return;

        playerCount.text = "Players: " + currentLobby.Players.Count + " / " + currentLobby.MaxPlayers;
    }
}
