using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TMPro;

// this is the actual gamemanger of this game because everything should be controlled by the server

public class TcpClient1 : MonoBehaviour
{
    
    public string[] TextArray = new string[10];
    public Animator startAnim;
    public bool isControlled;
    public SpriteRenderer WIFIsp;
    public int[] playerScore = new int[2];
    const string Uppers= "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public int WIFIdelay;
    public TMP_InputField ipInputField1;
    public TMP_InputField portInputField;
    public TMP_InputField ipInputField2;
    public MapGenerator mapGenerator;
    public Button connectButton1;
    public Button connectButton2;
    public GameObject[] controlButtons;
    public GameObject[] relatedTexts;
    public GameObject[] endTexts;
    public GameObject Eye;
    public int timeInt;
    public bool isUpdatePosition;

    public Vector3[] positionCache = new Vector3[2];

    public string Ip;
    public int portNo;
    public Animator animator;
    public int playerID = 324;
    public int playerID2 = 324;
    public int[] KeysE = new int[7];
    public string Notice;

    [System.Serializable]
    public class D
    {
        public float H1;
        public float H2;
        public float E1;
        public float E2;
        public int F1;
        public int F2;
    }
    [System.Serializable]
    private class Wr
    {
        public D d1;
    }

    private bool isPyConnected = false;
    private TcpClient client;
    public TcpClient PyClient;
    public UdpClient1 udpClient1;
    private NetworkStream stream;
    private NetworkStream PyStream;
    private TcpClient client2;
    private NetworkStream stream2;
    private Thread receiveThread;
    private Thread serverThread; // connecting to python
    private TcpListener listener;
    public Vector3[] PlayerXTransformPosition = new Vector3[2];
    public string message1;
    private bool[] isKeyS = new bool[7];
    private bool[] isKeySUp = new bool[7];
    private bool[] isKeySDown = new bool[7];
    private bool isSend;
    public bool isStart;
    private bool isRestart;
    private bool[] isButton = new bool[1];
    public bool isStartCache;
    public int[] KeyS = new int[7];
    public int[] KeySInControlCache = new int[7];
    private string clientId = "";

    public float[] playerHealthData = new float[4];

    void Start()
    {
        isRestart = false;
        isStart = false;
        isControlled = false;

        Application.targetFrameRate = 30;
        
        UpdateStatus("Game Initialised");
        connectButton1.onClick.AddListener(OnConnectButtonClicked1);
        connectButton2.onClick.AddListener(OnConnectButtonClicked2);
    }
    private void OnConnectButtonClicked1()
    {
        Ip = ipInputField1.text;
        

        if (Ip == ""){
            Ip = "127.0.0.1";
        }

        string S1 = "";

        int charAmount = UnityEngine.Random.Range(10, 15);

        for(int i=0; i<charAmount; i++)
        {
            S1 += Uppers[UnityEngine.Random.Range(0, Uppers.Length)];
        }


        string S2 = ipInputField2.text;
        
        if (S2 == ""){
            clientId = "Player"+S1;
        }
        else{
            clientId = S2;
        }
        
        Debug.Log("<> Start Connecting to Server");
        Debug.Log("<> ServerIP: " + Ip + "; Port: 5000");

        ConnectToServer();
    }
    private void OnConnectButtonClicked2()
    {
        string port = portInputField.text;
        portNo = 12345;
        
        if (port != ""){
            portNo = int.Parse(portInputField.text);
        }

        listener = new TcpListener(IPAddress.Any, portNo);
        listener.Start();
        isPyConnected = true;
        Debug.Log("() Server started...");
        UpdateStatus("Connecting to python");

        // a subroutine from unity main
        serverThread = new Thread(new ThreadStart(HandleClient));
        serverThread.IsBackground = true; 
        serverThread.Start();
    }
    private void HandleClient()
    {
        PyClient = listener.AcceptTcpClient();
        Debug.Log("() Python Client connected!");
        UpdateStatus("Connected");
        PyStream = PyClient.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = PyStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                isControlled = true;
                Debug.Log("() Controlled by FPGA: "+isControlled);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("() Python sent data: "+data);

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
                        Debug.Log("() (Py) Key: "+key+", Content: "+value);

                        if (key == "Key"){
                            string pattern2 = @"\d+";
                            MatchCollection matches2 = Regex.Matches(value, pattern2);
                            //Debug.Log(("Received action from server: ", value));
                            for (int i = 0; i < matches2.Count; i++)
                            {
                                if (int.TryParse(matches2[i].Value, out int number))
                                {
                                    KeyS[i] = number;
                                }
                            }
                            Debug.Log("() (Regex) Key: "+key+", Content: "+KeyS);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("() Python Client handling error: " + e.Message);
        }
        finally
        {
            PyClient.Close();
            Debug.Log("() Python Client disconnected.");
        }
    }
    public void ConnectToServer()
    {
        try
        {
            client = new TcpClient(Ip, 5000);
            stream = client.GetStream();

            byte[] idData = Encoding.UTF8.GetBytes(clientId);
            stream.Write(idData, 0, idData.Length);
            Debug.Log("<> Sent ID to server: " + clientId);

            UpdateStatus("Waiting another player");

            // subroutine receiving server data
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("<> Client error: " + e.Message);
            UpdateStatus("Connection failed");
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
    void ParseData2(string data, int playerID, int playerID2)
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
                ReadParsedData2(key, value, playerID, playerID2);
            }
        }
    }
    void ReadParsedData1(string key, string value){

        if (key == "ID")
        {
            playerID = int.Parse(value);
            Debug.Log("<> Assigned Player ID: " + playerID);
            UpdateStatus("Your ID: " + playerID);

            if(playerID == 0){
                playerID2 = 1;
            }
            if(playerID == 1){
                playerID2 = 0;
            }
        }
        
        else if (key == "SEED")
        {
            mapGenerator.seed = int.Parse(value);
            mapGenerator.isStart = true;
        }
        else if (key == "ACT")
        {
            
        }
        if (key == "STT")
        {
            Debug.Log("STT");
            udpClient1.ConnectToServer("<SHK>"+clientId+"</SHK>");
            Debug.Log("<-> <SHK>"+clientId+"</SHK>");
        }
    }
    void ReadParsedData2(string key, string value, int playerID, int playerID2){
        if (key == "STT")
        {
            Debug.Log("STT");
            
        }
        if (key == "RES"){
            isRestart = true;
            Debug.Log(">>> RES");
        }
        if (key == "WEND"){
            isButton[0] = true;
            
            Debug.Log(">>> WEND");
        }
        if (key == "STAT")
        {
            Wr wrapper = JsonUtility.FromJson<Wr>(value);
            D playerPositions = wrapper.d1;
            // PlayerXTransformPosition[playerID] = playerPositions.P1;
            // PlayerXTransformPosition[playerID2] = playerPositions.P2;
            playerHealthData[playerID] = playerPositions.H1; 
            playerHealthData[playerID2] = playerPositions.H2;
            playerHealthData[playerID+2] = playerPositions.E1;
            playerHealthData[playerID2+2] = playerPositions.E2;
            playerScore[playerID] = playerPositions.F1;
            playerScore[playerID2] = playerPositions.F2;

            if (isPyConnected){
                SendDataToPy();
            }
        }
        if (key == "ACT")
        {
            string pattern = @"\d+";
            MatchCollection matches = Regex.Matches(value, pattern);
            //Debug.Log(("Received action from server: ", value));
            for (int i = 0; i < matches.Count; i++)
            {
                if (int.TryParse(matches[i].Value, out int number))
                {
                    KeysE[i] = number;
                }
            }
        }
        if (key == "TIME"){
            WIFIdelay = timeInt-int.Parse(value);
            Debug.Log("<> delay = "+(timeInt-int.Parse(value)).ToString());
        }
    }
    void SendDataToPy(){
        lock(PyClient){
            
            string jsonData = playerHealthData[0].ToString();

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(jsonData);
                PyStream.Write(data, 0, data.Length);
                Debug.Log("() Sent health to PyClient: " + jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError("() Error sending data to " + client + ": " + e.Message);
            }
        }
    }
    private void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;
        bool isUdp = false;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (playerID > 10)
                {
                    ParseData1(jsonData);

                    string shake = string.Format("<SHK>{0}</SHK>", playerID);

                    byte[] idData = Encoding.UTF8.GetBytes(shake);
                    stream.Write(idData, 0, idData.Length);
                    Debug.Log("<> Sent SHK to server: " + playerID);
                    isStartCache = true;
                }
                else
                {
                    isStart = true;
                    if (!isUdp){
                        udpClient1.playerID = playerID;
                        udpClient1.playerID2 = playerID2;
                        udpClient1.serverIP = Ip;
                        udpClient1.ConnectToServer("<SHK>"+clientId+"</SHK>");
                        Debug.Log("<-> <SHK>"+clientId+"</SHK>");
                        isUdp = true;
                    }
                    
                    ParseData2(jsonData, playerID, playerID2);
                }
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError("<> Receive data error: " + e.Message);
        }
    }
    void Update()
    {
        if (isRestart){
            animator.SetTrigger("FadeIn");
            if (Input.GetMouseButtonDown(0)){
                string currentSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentSceneName);
                isRestart = false;
            }

            Eye.SetActive(true);

            if (playerScore[0] > playerScore[1]){
                endTexts[0].SetActive(true);
            }
            else if (playerScore[0] < playerScore[1]){
                endTexts[1].SetActive(true);
            }
            else if (playerScore[0] == playerScore[1]){
                endTexts[2].SetActive(true);
            }
        }

        for (int i = 0; i < isButton.Length; i++){
            if (isButton[i]){
                controlButtons[i].SetActive(true);
                relatedTexts[i].SetActive(true);
            }
        }

        AddToTextArray();
        startAnim.ResetTrigger("Start");
        timeInt = (int)(Time.time*1000f);
        isSend = false;

        if (positionCache[0] != PlayerXTransformPosition[0] || positionCache[1] != PlayerXTransformPosition[1]){
            isUpdatePosition = true;
        }
        else{
            isUpdatePosition = false;
        }

        if (isStart){
            WIFIsp.color = Color.Lerp(Color.green, Color.red, Mathf.Clamp((float)(WIFIdelay+50)/460f, 0, 1));
        }

        positionCache[0] = PlayerXTransformPosition[0];
        positionCache[1] = PlayerXTransformPosition[1];

        if (!isStart){
            for (int i = 0; i < 4; i++){
                playerHealthData[i] = 100f;
            }
        }

        if (isStartCache){
            startAnim.SetTrigger("Start");
            isStartCache = false;
        }

        if (isStart){
            if (!isControlled){
                message1 = "";

                AssignKeys();

                isSend = false;
                message1 = clientId + ": ";

                for (int i = 0; i < 7; i++){
                    if(isKeySDown[i]){
                        KeyS[i] = 1;
                        isSend = true;
                    }
                    else if (isKeySUp[i]){
                        KeyS[i] = 0;
                        isSend = true;
                    }
                    string keystring = string.Format("{0}/", KeyS[i]);
                    message1 += keystring;
                }
            }

            if (isControlled){
                message1 = "";
                isSend = false;
                for (int i = 0; i < 7; i++){
                    if (KeyS[i] != KeySInControlCache[i]){
                        isSend = true;
                    }
                    KeySInControlCache[i] = KeyS[i];
                    // Debug.Log("<> Change Key: " + (i+1) + "Value" + KeyS[i]);
                }
                if (isSend){
                    for (int i = 0; i < 7; i++){
                        message1 += string.Format("{0}/", KeyS[i]);
                    }
                    Debug.Log("<> Will Sent data to server: " + message1);
                }
            }
            
            if (isSend){
                SendDataToServer("<ACT>"+message1+"</ACT><TIME>"+timeInt.ToString()+"</TIME>");
                isSend = false;
            }
        }
    }
    public void SendDataToServer(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("<> Sent data to server: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("<> Send data error: " + e.Message);
        }
    }
    void AddToTextArray(){
        TextArray[0] = Notice;
        TextArray[1] = Mathf.Clamp(WIFIdelay, 0, 460).ToString();
        TextArray[2] = playerScore[0].ToString();
        TextArray[3] = playerScore[1].ToString();
    }
    void AssignKeys(){
                isKeyS[0] = Input.GetKey(KeyCode.W);
                isKeyS[1] = Input.GetKey(KeyCode.A);
                isKeyS[2] = Input.GetKey(KeyCode.S);
                isKeyS[3] = Input.GetKey(KeyCode.D);
                isKeyS[4] = Input.GetKey(KeyCode.E);
                isKeyS[5] = Input.GetKey(KeyCode.R);
                isKeyS[6] = Input.GetMouseButton(0);

                isKeySDown[0] = Input.GetKeyDown(KeyCode.W);
                isKeySDown[1] = Input.GetKeyDown(KeyCode.A);
                isKeySDown[2] = Input.GetKeyDown(KeyCode.S);
                isKeySDown[3] = Input.GetKeyDown(KeyCode.D);
                isKeySDown[4] = Input.GetKeyDown(KeyCode.E);
                isKeySDown[5] = Input.GetKeyDown(KeyCode.R);
                isKeySDown[6] = Input.GetMouseButtonDown(0);

                isKeySUp[0] = Input.GetKeyUp(KeyCode.W);
                isKeySUp[1] = Input.GetKeyUp(KeyCode.A);
                isKeySUp[2] = Input.GetKeyUp(KeyCode.S);
                isKeySUp[3] = Input.GetKeyUp(KeyCode.D);
                isKeySUp[4] = Input.GetKeyUp(KeyCode.E);
                isKeySUp[5] = Input.GetKeyUp(KeyCode.R);
                isKeySUp[6] = Input.GetMouseButtonUp(0);
    }
    private void UpdateStatus(string message)
    {
        Notice = message;
    }
    void OnDestroy()
    {
        if (client != null)
        {
            client.Close();
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort(); // 停止接收线程
        }
    }
}