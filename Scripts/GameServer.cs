using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class GameServer : NetworkBehaviour
{
    public NetworkVariable<Vector3> player1Position = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> player2Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> player1Health = new NetworkVariable<int>(100);
    public NetworkVariable<int> player2Health = new NetworkVariable<int>(100);
    public float F1;

    public UnityTransport transport;
    public NetworkManager networkManager;

    void Start()
    {
        if (transport != null)
        {
            transport.SetConnectionData("127.0.0.1", 7777); // this is for local transport
            Debug.Log($"Server will start on port: {transport.ConnectionData.Port}");
        }

        if (networkManager != null){
            networkManager.StartServer();
            Debug.Log("Server started!");
        }
        else{
            Debug.LogError("Server not started!");
        }
    }

    [ServerRpc]
    public void UpdatePlayer1PositionServerRpc(Vector3 position)
    {
        player1Position.Value = position;
    }

    [ServerRpc]
    public void UpdatePlayer2PositionServerRpc(Vector3 position)
    {
        player2Position.Value = position;
    }

    [ServerRpc]
    public void UpdatePlayer1HealthServerRpc(int health)
    {
        player1Health.Value = health;
    }

    [ServerRpc]
    public void UpdatePlayer2HealthServerRpc(int health)
    {
        player2Health.Value = health;
    }

    [ClientRpc]
    public void SyncGameStateClientRpc(Vector3 player1Pos, Vector3 player2Pos, int player1Hp, int player2Hp)
    {
        F1 = player1Pos.x;
        player1Position.Value = player1Pos;
        player2Position.Value = player2Pos;
        player1Health.Value = player1Hp;
        player2Health.Value = player2Hp;
    }

    void Update()
    {
        if (IsServer)
        {
            SyncGameStateClientRpc(player1Position.Value, player2Position.Value, player1Health.Value, player2Health.Value);
        }
    }
}
/*

Example of the errors solved tonight: 

Socket error encountered; attempting recovery by creating a new one.
NetworkPrefab hash was not found! In-Scene placed NetworkObject soft synchronization failure for Hash: 12990006!
[Netcode] Failed to create object locally. [globalObjectIdHash=12990006]. NetworkPrefab could not be found. 
[Netcode] NetworkConfig mismatch. The configuration between the server and client does not match

...... 

Yet there are still errors bumping out. 
I find it very hard to understand some specific codes and errors because they are completely integrated, and there are many inviolable rules 
(like: I always need to instantiate a player prefab in server and send it to client, but not pre-declare one in the client scene)
(like: config of NetworkManager of server and client must be exactly (and there are a lot of parameters))

I think that maybe I should try using something like a python server instead (directly connected to the reference-game unity project, transfer data by tcp or udp between 2 unity clients project)

*/