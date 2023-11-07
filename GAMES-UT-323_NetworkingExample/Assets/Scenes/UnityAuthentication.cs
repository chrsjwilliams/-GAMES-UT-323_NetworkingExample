using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace GAMES_UT323.Networking
{
    public class UnityAuthentication : MonoBehaviour
    {
        // We can only authenticate AFTER UnityServices has been initialized.
        // In this example Unity Services is initialized in Services.cs in Start()
        void Awake()
        {
            Services.UnityServiceInitalized += OnUnityServiceInitialized;
        }

        private void OnDestroy()
        {
            Services.UnityServiceInitalized -= OnUnityServiceInitialized;
        }

        private void OnUnityServiceInitialized()
        {
            SetupEvents();
            _ = SignInAnonymouslyAsync();
        }

        // Since Authentication relies on waiting for a response over a connection,
        // we need to use an async function. This way our code can wait for a response
        // In async functions, execution pauses on lines with await then resume once
        // a reponse has been recorded
        private async Task SignInAnonymouslyAsync()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("<color=cyan>[Unity Auth] Successfuly signed in!</color>");
                Debug.Log($"<color=cyan>[Unity Auth] PlayerID: {AuthenticationService.Instance.PlayerId}</color>");

            }
            catch (AuthenticationException e)
            {
                Debug.Log("<color=cyan>[Unity Auth] ERROR: " + e.Message + "</color>");

            }
            catch (RequestFailedException e)
            {
                Debug.Log("<color=cyan>[Unity Auth] ERROR Request: " + e.Message + "</color>");
            }
        }

        void SetupEvents()
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                // Shows how to get a playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

                // Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

            };

            AuthenticationService.Instance.SignInFailed += (err) =>
            {
                Debug.LogError(err);
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                Debug.Log("Player signed out.");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("Player session could not be refreshed and expired.");
            };
        }
    }
}