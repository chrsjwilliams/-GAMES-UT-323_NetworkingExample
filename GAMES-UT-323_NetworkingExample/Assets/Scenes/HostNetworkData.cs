using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;
using UnityEngine;

namespace GAMES_UT323.Networking
{
    [System.Serializable]
    public struct SomeGameInfo
    {
        public string message;
        public float x;
    }

    public class HostNetworkData : NetworkBehaviour
    {
        public static Action<HostNetworkData> JoinedRoomAsHost;

        bool joined = false;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            // Listen to when the client data is changed
            if (IsServer && !joined)
            {
                joined = true;
                JoinedRoomAsHost?.Invoke(this);
                return;
            }
        }

        public void SendDataToClient(SomeGameInfo info, ulong receiverId)
        {
            if (!IsServer) return;
            byte[] serializedData = Utils.ToBytes(info);
            var manager = NetworkManager.Singleton.CustomMessagingManager;
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(serializedData), Unity.Collections.Allocator.Temp);

            using (writer)
            {
                writer.WriteValueSafe(serializedData);
                manager.SendNamedMessage("SendDataToClient", receiverId, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        public void SendMaxParticipantErrorMessage(ulong receiverId)
        {
            if (!IsServer) return;
            string message = "";
            byte[] serializedData = Utils.ToBytes(message);
            var manager = NetworkManager.Singleton.CustomMessagingManager;
            var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(serializedData), Unity.Collections.Allocator.Temp);

            using (writer)
            {
                writer.WriteValueSafe(serializedData);
                manager.SendNamedMessage("SendMaxParticipantErrorMessage", receiverId, writer, NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
    }
}