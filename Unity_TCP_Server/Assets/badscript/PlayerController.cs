using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public bool[] isKeyS = new bool[7];
    public int[] KeyS = new int[7];
    public int[] EnemyKeyS = new int[7];

    private ServerManager serverManager;

    private void Start()
    {
        serverManager = FindAnyObjectByType<ServerManager>();
        if (serverManager == null)
        {
            Debug.LogError("ServerManager not found!");
        }
        else {
            Debug.Log("ServerManager found!");
        }
    }

    private void Update()
    {
        if (IsOwner) // this is monitering the key movement
        {
            isKeyS[0] = Input.GetKey(KeyCode.A);
            isKeyS[1] = Input.GetKey(KeyCode.D);
            isKeyS[2] = Input.GetKey(KeyCode.W);
            isKeyS[3] = Input.GetKey(KeyCode.S);
            isKeyS[4] = Input.GetKey(KeyCode.E);
            isKeyS[5] = Input.GetKey(KeyCode.R);
            isKeyS[6] = Input.GetKey(KeyCode.Q);

            for (int i = 0; i < 7; i++){ // key detection: keyS > 0
                if(isKeyS[i]){ // key down detection: 0 < KeyS =< 3
                    KeyS[i] += 1;
                }
                else if (KeyS[i] <= 0){ // key up detection: -2 =< keyS < 0
                    KeyS[i] /= 2;
                }
                else{
                    KeyS[i] = -2;
                }
            }

            SendInputServerRpc(KeyS);
        }
    }

    [ServerRpc]
    private void SendInputServerRpc(int[] Key_Array, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        serverManager.ProcessClientInput(clientId, Key_Array);

        Debug.Log($"Received input array from client {clientId}:");
    }

    public void ProcessInput(int[] Key_Array)
    {
        EnemyKeyS = Key_Array;
    }
}