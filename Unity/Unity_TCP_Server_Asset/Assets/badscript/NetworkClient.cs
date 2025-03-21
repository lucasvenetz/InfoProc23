using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    public string serverIP = "127.0.0.1";
    public int serverPort = 5000;
    public Player player;

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            Debug.Log("server connected");

            stream.BeginRead(buffer, 0, buffer.Length, ReceiveData, null);
        }
        catch (Exception e)
        {
            Debug.LogError("connection failed: " + e.Message);
        }
    }

    void ReceiveData(IAsyncResult result)
    {
        try
        {
            int bytesRead = stream.EndRead(result);
            if (bytesRead > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("received from server: " + message);

                stream.BeginRead(buffer, 0, buffer.Length, ReceiveData, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("data reception fail: " + e.Message);
        }
    }

    public void SendMessageToServer(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("message sent: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("message fail to send: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Close();
        }
    }
}