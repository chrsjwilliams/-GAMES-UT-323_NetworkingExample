using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class NetworkPlayer : NetworkBehaviour, IDraggable3D
{
    public static Action PlayerSpawned;

    Finger _fingerID = null;
    [SerializeField] bool canMove = true;

    Finger IDraggable3D.id
    {
        get => _fingerID;
        set => _fingerID = value;
    }
    bool IDraggable3D.canMove
    {
        // This way the owner can move just thier objects
        get => IsOwner;
        set => canMove = IsOwner;
    }
    Vector3 IDraggable3D.pos
    {
        get => transform.position;
        set => transform.position = value;
    }
    Quaternion IDraggable3D.rot
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }
    Vector3 IDraggable3D.scale
    {
        get => transform.localScale;
        set => transform.localScale = value;
    }

    void IDraggable3D.OnTap()
    {
        playerColor.Value = UnityEngine.Random.ColorHSV(0.25f, 1f, 0.25f, 1f, 0.25f, 1f);
        GetComponent<MeshRenderer>().material.color = playerColor.Value;
    }

    // We use NetworkVariables to sync data over the network. You cannot send
    // reference type (i.e. classes, objects, arrays, strings). If you want to
    // send a chunk of data, consider packaging that information in a struct.
    // When defining a network varible they must be initalized on the same line.
    private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.black,
                                                                            NetworkVariableReadPermission.Everyone,
                                                                            NetworkVariableWritePermission.Owner);


   

    ////////////// PLEASE NOTE ///////////////////
    //                                          //
    //      If you want to send a struct        //
    //      you need to implement the           //
    //      INetworkSerializable interface      //
    //      on your struct                      //
    //                                          //
    //////////////////////////////////////////////

    //Example (this is not being used in this demo)
    public struct PlayerData : INetworkSerializable
    {
        // You can use a stirng in this case, if you assing it a value when initalizing
        // your network variable
        public string playerName;
        public int playerEnergy;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerEnergy);
        }
    }

    //  For dynamically spawned NetworkObjects(instantiating a network Prefab during runtime)
    //  the OnNetworkSpawn method is invoked before the Start method is invoked.So, it's
    //  important to be aware of this because finding and assigning components to a local
    //  property within the Start method exclusively will result in that property not being
    //  set in a NetworkBehaviour component's OnNetworkSpawn method when the NetworkObject
    //  is dynamically spawned.
    public override void OnNetworkSpawn()
    {
        playerColor.OnValueChanged += OnValueChanged;

        if (!IsOwner) return;
        playerColor.Value = UnityEngine.Random.ColorHSV(0.25f, 1f, 0.25f, 1f, 0.25f, 1f);
        GetComponent<MeshRenderer>().material.color = playerColor.Value;
        PlayerSpawned?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        playerColor.OnValueChanged -= OnValueChanged;

        if (!IsOwner) return;
        base.OnNetworkDespawn();
    }

    private void OnValueChanged(Color previousValue, Color newValue)
    {
        playerColor.Value = newValue;
        GetComponent<MeshRenderer>().material.color = playerColor.Value;
    }
}
