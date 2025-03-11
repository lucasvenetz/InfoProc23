using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class ServerManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    // Dictionary to store player objects by their client ID
    private Dictionary<ulong, GameObject> playerObjects = new Dictionary<ulong, GameObject>();
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found!");
        }
        else
        {
            Debug.Log("NetworkManager found!");
            networkManager.OnServerStarted += OnServerStarted;
            networkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started!");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected with ID: {clientId}");
    }

    // Method to register player objects
    public void RegisterPlayer(ulong clientId, GameObject playerObject)
    {
        if (!playerObjects.ContainsKey(clientId))
        {
            playerObjects[clientId] = playerObject;
            Debug.Log($"Player registered with client ID: {clientId}");
        }
        else
        {
            Debug.LogWarning($"Player with client ID {clientId} is already registered.");
        }
    }

    // Method to process client input
    public void ProcessClientInput(ulong clientId, int[] keyArray)
    {
        Debug.Log($"Processing input for client ID: {clientId}");
        if (playerObjects.TryGetValue(clientId, out GameObject playerObject))
        {
            // Assuming the player object has a PlayerController component
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ProcessInput(keyArray);
                Debug.Log($"Processed input for client ID: {clientId}");
            }
            else
            {
                Debug.LogError($"PlayerController component not found on player object for client ID: {clientId}");
            }
        }
        else
        {
            Debug.LogError($"Player object not found for client ID: {clientId}");
        }
    }

    // Example method to spawn player objects (call this when a client connects)
    public void SpawnPlayerObject(ulong clientId)
    {
        Debug.Log($"Spawning player object for client ID: {clientId}");
        // Instantiate the player prefab (you need to define this prefab)
        if (playerPrefab != null)
        {
            GameObject playerObject = Instantiate(playerPrefab);
            NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
                RegisterPlayer(clientId, playerObject);
                Debug.Log($"Spawned player object for client ID: {clientId}");
            }
            else
            {
                Debug.LogError("NetworkObject component not found on player prefab.");
            }
        }
        else
        {
            Debug.LogError("Player prefab not found in Resources.");
        }
    }
}