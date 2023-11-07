using Unity.Netcode;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

namespace GAMES_UT323.Networking
{
    // PlayerNetworkData is a class that is attached to both the client and the host.
    // Whenever someone connects to a room, a clone of the player prefab (which is just this)
    // is created on both the host and client accounts.
    public class PlayerNetworkData : NetworkBehaviour
    {
        [SerializeField]
        private NetworkVariable<PlayerData> clientData = new NetworkVariable<PlayerData>(new PlayerData { id = ulong.MinValue },
                                                                                            NetworkVariableReadPermission.Everyone,
                                                                                            NetworkVariableWritePermission.Owner);
        [System.Serializable]
        public struct PlayerData : INetworkSerializable
        {
            public ulong id;
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref id);
            }
        }

        public static Action<PlayerData> ClientJoined;
        public static Action<PlayerData> ClientLeft;

        // When someone connects to the room, this event is called on all entities
        public override void OnNetworkSpawn()
        {
            // Listen to when the client data is changed, both the client and host scripts need to
            // listen to this event.
            clientData.OnValueChanged += ValueChanged;

            // However, only the owner of that client data should change its own value and subscribe to
            // listen for info from the host
            if (!IsOwner) return;
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("SendDataToClient", ReceiveDataFromHost);
            ClientJoined?.Invoke(clientData.Value);
            clientData.Value = new PlayerData { id = OwnerClientId };
           
            gameObject.name = IsHost? "_HOST" : "Player " + OwnerClientId;
        }

        // OnNetworkDespawn is called on ALL active instances of PlayerNetworkData.
        // Therefore
        public override void OnNetworkDespawn()
        {
            if (IsHost) return;

            // Only the owner of the disconnect call unsubscribes. This is the same check done
            // in OnNetworkSpawn when we set the playFabID on clientData
            if (!IsOwner) return;
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("SendDataToClient");
            ClientLeft?.Invoke(clientData.Value);
            clientData.OnValueChanged -= ValueChanged;
        }

        // Once the value has been cahnged, we know the player has successfully
        // connected to the room. Now we can notify our listeneres we have joined and pass along
        // our PlayFabData
        private void ValueChanged(PlayerData previousValue, PlayerData newValue)
        {
            ClientJoined?.Invoke(newValue);
        }

        // This is an example of how the host can send information to clients
        public void ReceiveDataFromHost(ulong senderClientId, FastBufferReader messagePayload)
        {
            if (!IsOwner) return;

            byte[] data;

            messagePayload.ReadValueSafe(out data);

        }
    }
}
