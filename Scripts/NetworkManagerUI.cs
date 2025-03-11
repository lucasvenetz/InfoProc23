using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NetworkManagerUI : MonoBehaviour
{
    public Button serverButton;
    public Button hostButton;
    public Button clientButton;

    private void Start()
    {
        serverButton.onClick.AddListener(StartServer);
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server Started");
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host Started");
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client Started");
    }
}