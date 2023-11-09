using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;

public class GameScreenUI : MonoBehaviour
{
    [SerializeField] CanvasGroup gameScreenUI;
    [SerializeField] TextMeshProUGUI lobbyName;


    void OnEnable()
    {
        LobbyManager.StartingGame += OnStartingGame;
        LobbyManager.LeftGame += OnLeftLobby;
    }

    private void OnDisable()
    {
        LobbyManager.LeftGame -= OnLeftLobby;
        LobbyManager.StartingGame += OnStartingGame;
    }

    private void OnStartingGame(string roomName)
    {
        lobbyName.text = roomName;
        ShowGameScreen(true);
    }

    private void OnLeftLobby()
    {
        ShowGameScreen(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        ShowGameScreen(false);
    }

    private void ShowGameScreen(bool show)
    {
        gameScreenUI.alpha = show? 1: 0;
        gameScreenUI.blocksRaycasts = show;
        gameScreenUI.interactable = show;
    }
}
