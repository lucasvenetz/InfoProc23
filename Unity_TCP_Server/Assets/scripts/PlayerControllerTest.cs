using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class PlayerControllerTest : NetworkBehaviour
{
    private GameServer gameServer;
    public UnityTransport transport;
    public NetworkManager networkManager;

    void Start()
    {
        if (transport != null)
        {
            transport.SetConnectionData("127.0.0.1", 7777); // this is for local transport
            Debug.Log($"Client will started on port: {transport.ConnectionData.Port}");
        }

        if (networkManager != null){
            networkManager.StartClient();
            Debug.Log("Client started!");
        }
        else{
            Debug.LogError("Client not started!");
        }
        
        gameServer = FindAnyObjectByType<GameServer>();
    }

    void Update()
    {
        if (IsOwner)
        {
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            transform.position += moveDirection * Time.deltaTime * 5f;

            if (IsClient)
            {
                UpdatePositionServerRpc(transform.position);
            }
        }
    }

    [ServerRpc]
    public void UpdatePositionServerRpc(Vector3 position)
    {
        if (IsServer)
        {
            if (IsOwnedByServer)
            {
                gameServer.UpdatePlayer1PositionServerRpc(position);
            }
            else
            {
                gameServer.UpdatePlayer2PositionServerRpc(position);
            }
        }
    }
}