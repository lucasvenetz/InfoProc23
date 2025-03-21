using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class UdpClient1 : MonoBehaviour
{
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
    public Vector3[] PlayerXTransformPosition = new Vector3[2];
    public Vector3[] positionCache = new Vector3[2];
    private UdpClient udpClient;
    public string serverIP = "127.0.0.1";
    private Thread serverThread;
    public int playerID;
    public int playerID2;
    public bool isUpdatePosition;

    private void ReceiveData()
    {
        try
        {
            while (true){
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 5001);
                byte[] receivedData = udpClient.Receive(ref serverEndPoint);
                string receivedMessage = Encoding.UTF8.GetString(receivedData);
                Debug.Log("Received from Udp server: " + receivedMessage);
                ParseData1(receivedMessage);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("<> Udp Client handling error: " + e.Message);
        }
    }

    public void ConnectToServer(string message)
    {
        udpClient = new UdpClient();
        
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, serverIP, 5001);
            Debug.Log("<> Udp sent: " + message);
            
            serverThread = new Thread(new ThreadStart(ReceiveData));
            serverThread.IsBackground = true; 
            serverThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("<> Udp Client error: " + e.Message);
        }
    }

    void ParseData1(string data)
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
                ReadParsedData1(key, value);
            }
        }
    }

    void ReadParsedData1(string key, string value){

        if (key == "POS")
        {
            Wr2 wrapper = JsonUtility.FromJson<Wr2>(value);
            P playerPositions = wrapper.p21;
            PlayerXTransformPosition[playerID] = playerPositions.P1;
            PlayerXTransformPosition[playerID2] = playerPositions.P2;
            Debug.Log("<-> "+playerID+playerID2);
        }
    }

    void Update(){
        if (positionCache[0] != PlayerXTransformPosition[0] || positionCache[1] != PlayerXTransformPosition[1]){
            isUpdatePosition = true;
        }
        else{
            isUpdatePosition = false;
        }

        positionCache[0] = PlayerXTransformPosition[0];
        positionCache[1] = PlayerXTransformPosition[1];
    }
    void OnDestroy()
    {
        udpClient.Close();
        Debug.Log("<> Udp Client disconnected.");
    }
}