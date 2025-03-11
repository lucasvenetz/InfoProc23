using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;

public class UdpServer : MonoBehaviour
{
    public float sendInterval;
    private float timer = 0f;

    public bool[] pA = new bool[2];

    [System.Serializable]
    public class P
    {
        public Vector3 P1;
        public Vector3 P2;
    }

    [System.Serializable]
    private class Wr2
    {
        public P p21;
    }

    private UdpClient udpServer;
    private Thread serverThread;
    private IPEndPoint clientEndPoint;
    private Dictionary<IPEndPoint, int> clientNumbers = new Dictionary<IPEndPoint, int>();
    public Dictionary<string, int> clientsUDP = new Dictionary<string, int>();
    public GameObject[] PlayerX; 
    public bool isStart;
    public bool is1Connected;

    public void StartServer()
    {
        isStart = false;
        is1Connected = false;

        serverThread = new Thread(new ThreadStart(HandleClients));
        serverThread.IsBackground = true; 
        serverThread.Start();
    }

    private void HandleClients()
    {
        udpServer = new UdpClient(5001);

        while (true)
        {
            try
            {
                clientEndPoint = new IPEndPoint(IPAddress.Any, 5001);
                byte[] data = udpServer.Receive(ref clientEndPoint);
                string message = Encoding.UTF8.GetString(data);

                Debug.Log("<> Received from client " + message);

                if (!isStart){
                    ParseData1(message, clientEndPoint);
                    Debug.Log("<-> Received from client " + clientNumbers[clientEndPoint] + ": " + message);
                    if (pA[0] && pA[1]){
                        isStart = true;
                    }
                }
                if (isStart){
                    
                }
            }
            catch (SocketException e)
            {
                Debug.LogError("!!! Socket error: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError("!!! Error: " + e.Message);
            }
        }
    }

    void ParseData1(string data, IPEndPoint client0)
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
                Debug.Log("<-> Key: "+key+", Content: "+value);
                ReadParsedData1(key, value, client0);
            }
        }
    }

    void ReadParsedData1(string key, string value, IPEndPoint client0)
    {
        if (key == "SHK")
        {
            foreach (string S1 in clientsUDP.Keys){
                Debug.Log("<-> Playernames"+S1);
                Debug.Log("<-> Client ID: " + value);
                if (S1 == value){
                    clientNumbers[client0] = clientsUDP[value];
                    pA[clientNumbers[client0]] = true;
                    Debug.Log("<-> Client ID matched: " + value+" client No "+clientNumbers[client0]);
                }
            }
        }
    }

    private void BroadcastMessage1(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (var client1 in clientNumbers.Keys)
        {
            udpServer.Send(data, data.Length, client1);
        }
        Debug.Log("<> Broadcasted message to all clients: " + message);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= sendInterval){

            timer = 0f;

            if (isStart){
                P p2A = new P { 
                    P1 = PlayerX[0].transform.position,
                    P2 = PlayerX[1].transform.position,
                };

                string jsonData = "<POS>" + JsonUtility.ToJson(new Wr2 { p21 = p2A }) + "</POS>";

                BroadcastMessage1(jsonData);
            }
        }
    }

    void OnDestroy()
    {
        if (udpServer != null)
        {
            udpServer.Close();
            Debug.Log("Server stopped.");
        }

        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Abort();
        }
    }
}