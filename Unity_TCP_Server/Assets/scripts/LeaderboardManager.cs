
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

[Serializable]
public class GameSessionResult
{
    public string sessionId;
    public string timestamp;
    public string player1Id;
    public string player2Id;
    public int player1Score;
    public int player2Score;
}

[Serializable]
public class LeaderboardEntry
{
    public string playerId;
    public string playerName;
    public int totalWins;
    public int totalMatches;
    public int totalScore;
}

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private string apiBaseUrl = "https://c6ia6wv9c3.execute-api.us-east-1.amazonaws.com/default/swordfightFunction";
    
    // Singleton pattern
    private static LeaderboardManager _instance;
    public static LeaderboardManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
    public void SubmitGameResult(string player1Id, string player2Id, int player1Score, int player2Score)
    {
        GameSessionResult result = new GameSessionResult
        {
            sessionId = Guid.NewGuid().ToString(),
            timestamp = DateTime.UtcNow.ToString("o"),
            player1Id = player1Id,
            player2Id = player2Id,
            player1Score = player1Score,
            player2Score = player2Score
        };
        
        StartCoroutine(PostGameSession(result));
    }
    
    public void GetLeaderboard(Action<List<LeaderboardEntry>> callback)
    {
        StartCoroutine(FetchLeaderboard(callback));
    }
    
    public void GetPlayerStats(string playerId, Action<LeaderboardEntry> callback)
    {
        StartCoroutine(FetchPlayerStats(playerId, callback));
    }
    
    // POST game session data to AWS API
    private IEnumerator PostGameSession(GameSessionResult result)
    {
        string json = JsonConvert.SerializeObject(result);
        
        using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + "/game-sessions", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error submitting game result: " + request.error);
            }
            else
            {
                Debug.Log("Game session submitted successfully");
            }
        }
    }
    
    // GET leaderboard data from AWS API
    private IEnumerator FetchLeaderboard(Action<List<LeaderboardEntry>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl + "/leaderboard"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching leaderboard: " + request.error);
                callback(new List<LeaderboardEntry>());
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                List<LeaderboardEntry> leaderboard = JsonConvert.DeserializeObject<List<LeaderboardEntry>>(jsonResponse);
                callback(leaderboard);
            }
        }
    }
    
    // GET specific player stats from AWS API
    private IEnumerator FetchPlayerStats(string playerId, Action<LeaderboardEntry> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl + "/players/" + playerId))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching player stats: " + request.error);
                callback(null);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                LeaderboardEntry playerStats = JsonConvert.DeserializeObject<LeaderboardEntry>(jsonResponse);
                callback(playerStats);
            }
        }
    }
}