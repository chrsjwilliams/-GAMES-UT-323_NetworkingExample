using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Netcode;
using System.Threading.Tasks;
using GAMES_UT323.Networking;

/// <summary>
/// Singleton whose only purpose is to agreggate various other classes
/// that need to be globally accessible. It's a good idea to put all
/// classes that would be singletons here.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class Services : MonoBehaviour
{
    public static Action UnityServiceInitalized;

    private static Services checkedInstance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("Services instance is null.");
                return null;
            }
            return _instance;
        }
    }
    private static Services _instance;

    [SerializeField] private LobbyManager _lobbyManager;
    public static LobbyManager LobbyManager { get { return checkedInstance._lobbyManager; } }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        // Connected to UnityServices in Start() to ensure classes can subscribe
        // to ConnectedToUnityServicesEvent in their Awake() functions
        _ = SetupServices();
    }

    // Since Initializing Unity Services relies on waiting for a response over a
    // connection, we need to use an async function. This way our code can wait
    // for a response. In async functions, execution pauses on lines with await
    // then resume once a reponse has been recorded
    private async Task SetupServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
            NetworkManager.Singleton.SetSingleton();

            UnityServiceInitalized.Invoke();
        }
    }
}
