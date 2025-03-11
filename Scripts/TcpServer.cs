using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;

public class TcpServer : MonoBehaviour
{
    const int PLAYER_COUNT = 2;
    public float sendInterval;
    private float timer = 0f;
    public MapGenerator mapGenerator;

    public bool p1A = false;
    public bool p2A = false;

    [System.Serializable]
    public class D
    {
        public float H1;
        public float H2;
        public float E1;
        public float E2;
        public Vector3 P1;
        public Vector3 P2;
        public int F1;
        public int F2;
    }

    [System.Serializable]
    private class Wr
    {
        public D d1;
    }

    private TcpListener listener;
    public UdpServer udpServer;
    private Thread serverThread;
    private int[] delays = new int[2];
    private Dictionary<string, int> clientsUDP = new Dictionary<string, int>();
    private Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>(); // 存储客户端ID和对应的TcpClient
    private Dictionary<TcpClient, int> clientNumbers = new Dictionary<TcpClient, int>();
    public int[] reflection = new int[2];
    public GameObject[] PlayerX; 
    public Health[] health;
    public bool isStart;
    public bool[] pOut = new bool[2];
    public int[,] Keys = new int[2,7];

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();
        Debug.Log("Tcp Server started...");

        // a subroutine from unity main
        serverThread = new Thread(new ThreadStart(HandleClients));
        serverThread.IsBackground = true; 
        serverThread.Start();
    }

    private void HandleClients()
    {
        while (true)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                Debug.Log("Client connected!");

                // a subroutine for client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }
            catch (Exception e)
            {
                Debug.LogError("Server error: " + e.Message);
                break;
            }
        }
    }

    private void AssignPlayerIds()
    {
        int playerId = 0;
        foreach (var pair in clients)
        {
            TcpClient client = pair.Key;
            clientNumbers[client] = playerId;
            reflection[playerId] = playerId;
            NetworkStream stream1 = client.GetStream();

            string playerIdMessage = "<ID>"+playerId.ToString()+"</ID>";
            string seedMessage = "<SEED>"+mapGenerator.seed.ToString()+"</SEED>";
            byte[] data = Encoding.UTF8.GetBytes(playerIdMessage+seedMessage);
            stream1.Write(data, 0, data.Length);
            Debug.Log("Assigned Player ID: " + playerId + " to " + pair.Value + ", ClientInteral ID: " + client);

            playerId++;
        }
    }

    void ParseData1(string data, int clientNo)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        string pattern = @"<([^>]+)>([^<]+)</\1>";
        Regex regex = new Regex(pattern);

        MatchCollection matches = regex.Matches(data);
        foreach (Match match in matches)
        {
            if (match.Groups.Count == 3)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                Debug.Log("<> Key: "+key+", Content: "+value);
                ReadParsedData1(key, value, clientNo);
            }
        }
    }

    void ReadParsedData1(string key, string value, int clientNo)
    {
        if (key == "SHK")
        {
            if (value == "0"){
                p1A = true;
            }
            if (value == "1"){
                p2A = true;
            }
        }
        else if (key == "TIME")
        {
            delays[clientNo] = int.Parse(value)-(int)(sendInterval*1000f);
            Debug.Log(value);
        }
        else if (key == "ACT"){

            string pattern = @"\d+";
            MatchCollection matches = Regex.Matches(value, pattern);

            for (int i = 0; i < matches.Count && i < 7; i++)
            {
                if (int.TryParse(matches[i].Value, out int number))
                {
                    Keys[clientNo,i] = number;
                }
            }

        }
        else if (key == "BX1")
        {
            Debug.Log(value);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        TcpClient client2;

        NetworkStream stream = client.GetStream();
        
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string clientId = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log("Client ID received: " + clientId);

            lock (clients) // ensure safety
            {
                clients[client] = clientId;
                
            }

            if (clients.Count == 2)
            {
                AssignPlayerIds();

                foreach (var clientX in clients.Keys){
                    clientsUDP[clients[clientX]] = clientNumbers[clientX];
                    Debug.Log("<<->> ID: "+ clientsUDP[clients[clientX]] + ", number: "+ clientNumbers[clientX]);
                }

                udpServer.clientsUDP = clientsUDP;
                
                udpServer.StartServer();
                Debug.Log("<> TCP Server : UDP Server start");
            }

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (isStart == false){
                    ParseData1(data, clientNumbers[client]);

                    if (p1A && p2A){

                        Debug.Log("Game started!");
                        isStart = true;

                        lock(clients){
                            foreach (var client5 in clients.Keys){
                                NetworkStream stream5 = client5.GetStream();
                                byte[] data3 = Encoding.UTF8.GetBytes("<STT>STT<STT>");
                                stream5.Write(data3, 0, data3.Length);
                                Debug.Log("STT");
                            }
                        }
                    }
                }
                
                Debug.Log("Received from Player" + clientNumbers[client]  + ": " + data);

                string sentToOpponent = data;

                ParseData1(data, clientNumbers[client]);

                for (int p = 0; p < PLAYER_COUNT; p++){

                    if (clientNumbers[client] == p){

                        lock (client) // All data sent to client 1
                        {
                            try
                            {
                                byte[] data3 = Encoding.UTF8.GetBytes("<TIME>"+delays[clientNumbers[client]].ToString()+"</TIME>");
                                stream.Write(data3, 0, data3.Length);
                                Debug.Log("Sent time back to Client"+client);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Error sending data to " + client + ": " + e.Message);
                            }
                        }

                        client2 = client;

                        foreach (var pair in clients)
                        {
                            Debug.Log(clientNumbers[pair.Key]+"??"+p);
                            if (clientNumbers[pair.Key] != p) client2 = pair.Key;
                        }

                        NetworkStream stream2 = client2.GetStream();

                        lock (client2) // All data sent to client 2
                        {
                            try
                            {
                                byte[] data2 = Encoding.UTF8.GetBytes(sentToOpponent);
                                stream2.Write(data2, 0, data2.Length);
                                Debug.Log("Sent action to client" + clientNumbers[client2] + " opponent action: " + sentToOpponent);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("Error sending data to " + client2 + ": " + e.Message);
                            }
                        }
                    }
                }  
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Client handling error: " + e.Message);
        }
        finally
        {
            lock (clients)
            {
                clients.Remove(client);
            }
            client.Close();
            Debug.Log("Client disconnected.");
        }
    }

    void Update()
    {
        
        timer += Time.deltaTime;

        if (timer >= sendInterval){

            timer = 0f;
            
            if (isStart){
                D d = new D { 
                    H1 = health[0].currentHealth,
                    H2 = health[1].currentHealth,
                    E1 = health[0].currentEnergy,
                    E2 = health[1].currentEnergy,
                    //P1 = PlayerX[0].transform.position,
                    //P2 = PlayerX[1].transform.position,
                    F1 = health[1].enemyScore,
                    F2 = health[0].enemyScore
                };

                lock (clients)
                {
                    foreach (var pair in clients)
                    {
                        TcpClient client = pair.Key;
                        string jsonData;

                        try
                        {
                            NetworkStream stream = client.GetStream();
                            jsonData = "<STAT>" + JsonUtility.ToJson(new Wr { d1 = d }) + "</STAT>";
                            byte[] data = Encoding.UTF8.GetBytes(jsonData);
                            stream.Write(data, 0, data.Length);
                            Debug.Log("Sent position to " + client + ": " + jsonData);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Error sending data to " + client + ": " + e.Message);
                        }
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        if (listener != null)
        {
            listener.Stop();
            Debug.Log("Server stopped.");
        }

        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Abort();
        }

        lock (clients)
        {
            foreach (var client in clients.Keys)
            {
                client.Close();
            }
            clients.Clear();
        }
    }
}
