using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Networking.Transport.Relay;
using TMPro;

namespace GAMES_UT323.Networking
{
    [RequireComponent(typeof(UnityTransport))]
    public class MatchmakingService : MonoBehaviour
    {
        public Action RoomCreated;

        UnityTransport transport;

        public int MaxPlayers { get { return players; } }

        [Range(0, 99), SerializeField] int players;
        [SerializeField] private TextMeshProUGUI _joinCode;

        private void Awake()
        {
            transport = GetComponent<UnityTransport>();
        }

        public async void TryJoinRoom(string joinCode, bool reconnect)
        {
            if(joinCode.Length == 0)
            {
                Debug.Log("<color=yellow>[Match Making Service] ERROR: Join Code cannot be empty.</color>");
                return;
            }

            if(joinCode.Length != 6)
            {
                Debug.Log("<color=yellow>[Match Making Service] ERROR: Join Code must by 6 characters.</color>");
                return;
            }

            if(joinCode.Contains(" "))
            {
                Debug.Log("<color=yellow>[Match Making Service] ERROR: Join Code cannot contain blank spaces.</color>");
                return;
            }

            try
            {
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                //Populate the joining data
                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

                transport.SetClientRelayData(allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.HostConnectionData);

                Debug.Log("<color=yellow>[Match Making Service] JOINING ROOM</color>");
                NetworkManager.Singleton.StartClient();
                //Events.Raise(new EndedReconnectAttempt());
                if (reconnect)
                {
                    Debug.Log("<color=yellow>[Match Making Service] Attempting to reconnect.</color>");
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("join code not found") && !reconnect)
                {
                    Debug.Log("<color=yellow>[Match Making Service] ERROR: Room not found.</color>");
                    return;
                }
                if(ex.Message.Contains("join code not found") && reconnect)
                {
                    Debug.Log("<color=yellow>[Match Making Service] ERROR: Cannot reconnect to room.</color>");
                    return;
                }
                // catch all
                Debug.Log("<color=yellow>[Match Making Service] ERROR: " +ex.Message + ".</color>");    
                return;
            }

        }

        public async Task CreateRoomTask()
        {
            try
            {
                //Ask Unity Services to allocate a Relay server that will handle up to players + 1. The +1 is for the host
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(players + 1);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                _joinCode.text = joinCode;
                
                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                Debug.Log("<color=yellow>[Match Making Service] ROOM MADE</color>");
                transport.SetHostRelayData(
                   allocation.RelayServer.IpV4,
                   (ushort)allocation.RelayServer.Port,
                   allocation.AllocationIdBytes,
                   allocation.Key,
                   allocation.ConnectionData);
                NetworkManager.Singleton.StartHost();

            }
            catch (RelayServiceException e)
            {
                Debug.Log(e.Message);
            }
        }

        // Hooked into Simple Event Listener for when the Create Room Button is pressed
        public void CreateRoom()
        {
            _ = CreateRoomTask();
        }

        public void LeaveRoom(bool clearCachedJoinCode)
        {
            if (clearCachedJoinCode)
            {
                PlayerPrefs.DeleteKey("cachedJoinCode");
                PlayerPrefs.Save();
            }
            NetworkManager.Singleton.Shutdown();
            Debug.Log("<color=yellow>[Match Making Service] Player has left lobby</color>");
        }

        public void Reconnect()
        {
            string cachedJoinCode = PlayerPrefs.GetString("cachedJoinCode");
            TryJoinRoom(cachedJoinCode, reconnect: true);
        }
    }
}