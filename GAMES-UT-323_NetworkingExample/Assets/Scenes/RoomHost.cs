using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GAMES_UT323.Networking
{
    public class RoomHost : MonoBehaviour
    {
        private bool ready;
        [SerializeField] TextMeshProUGUI playerCountText;
        [SerializeField] HostNetworkData host;
        [SerializeField] List<ulong> clientIds = new List<ulong>();

        private void Awake()
        {
            host = null;
        }

        private void OnEnable()
        {
            Services.MatchmakingService.RoomCreated += OnRoomCreated;
            HostNetworkData.JoinedRoomAsHost += OnJoinRoomAsHost;
            PlayerNetworkData.ClientJoined += OnClientJoined;
            PlayerNetworkData.ClientLeft += OnClientLeft;

            playerCountText.text = "";
        }

        private void OnDisable()
        {
            Services.MatchmakingService.RoomCreated -= OnRoomCreated;
            HostNetworkData.JoinedRoomAsHost -= OnJoinRoomAsHost;
            PlayerNetworkData.ClientJoined -= OnClientJoined;
            PlayerNetworkData.ClientLeft -= OnClientLeft;
        }

        private void OnRoomCreated()
        {
            RefreshPlayerCountText();
        }

        private void OnJoinRoomAsHost(HostNetworkData hostData)
        {
            if (host != null) return;

            host = hostData;
            ready = true;
            hostData.gameObject.name = "_HOST";

        }

        // Once PlayerNetworkData tells us a player has been connected, we can
        // determine if that player should be added to the list of playFabIds
        // we send push notifications to.
        private void OnClientJoined(PlayerNetworkData.PlayerData client)
        {
            if (!NetworkManager.Singleton.IsHost || client.id == host.OwnerClientId) return;

            if (NetworkManager.Singleton.ConnectedClients.Count >= Services.MatchmakingService.MaxPlayers)
            {
                host.SendMaxParticipantErrorMessage(client.id);
                return;
            }

            host.SendDataToClient(new SomeGameInfo()
            {
                message = "Welcome",
                x = (int)UnityEngine.Random.Range(0, 100)
            }, client.id) ;
            if (!clientIds.Contains(client.id))
            {
                clientIds.Add(client.id);
            }
            RefreshPlayerCountText();
        }

        private void OnClientLeft(PlayerNetworkData.PlayerData client)
        {
            // remove client id first, in both cases of disconnect or leave button being pressed
            clientIds.Remove(client.id);

            RefreshPlayerCountText();
        }

        void RefreshPlayerCountText()
        {
            playerCountText.text = clientIds.Count + "/ " + Services.MatchmakingService.MaxPlayers;
        }

        public void CloseRoom()
        {
            clientIds.Clear();
            Services.MatchmakingService.LeaveRoom(clearCachedJoinCode: true);
            RefreshPlayerCountText();
        }

        public void SendMessageToClients()
        {
            if (!ready) return;
            ready = false;

            Debug.Log("<color=magenta>[ROOM HOST] Sending message</color>");

            foreach (var client in clientIds)
            {
                host.SendDataToClient(new SomeGameInfo
                {
                    message = "This is neat.",
                    x = (int)UnityEngine.Random.Range(0, 100)
                }, client);
            }
            StartCoroutine(ElapsingDelay());
        }

        IEnumerator ElapsingDelay()
        {
            yield return new WaitForSeconds(1);
            ready = true;
        }
    }
}