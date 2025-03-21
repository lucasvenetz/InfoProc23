using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class PlayerScore
{
    public string playerId;
    public int score;
}

[System.Serializable]
public class LeaderboardEntry
{
    public string playerId;
    public int score;
}

[System.Serializable]
public class LeaderboardResponse
{
    public LeaderboardEntry[] leaderboard;
}

public class ApiHandler : MonoBehaviour
{
    [SerializeField]
    private string apiBaseUrl = "https://c6ia6wv9c3.execute-api.us-east-1.amazonaws.com/default/swordfightFunction/scores";
    
    // Submit score to the database
    public IEnumerator SubmitScore(string playerId, int score, Action<bool, string> callback)
    {
        PlayerScore playerScore = new PlayerScore
        {
            playerId = playerId,
            score = score
        };
        
        string jsonPayload = JsonUtility.ToJson(playerScore);
        
        using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl, "POST"))
        {
            Debug.Log($"<--> WebRequest");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log($"<--> Submitting score: {jsonPayload}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"<--> Error submitting score: {request.error}");
                callback(false, request.error);
            }
            else
            {
                Debug.Log("<--> Score submitted successfully!");
                callback(true, request.downloadHandler.text);
            }
        }
    }
    
    // Fetch leaderboard data
    public IEnumerator GetLeaderboard(Action<LeaderboardEntry[]> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}?action=leaderboard"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"<--> Error fetching leaderboard: {request.error}");
                callback(new LeaderboardEntry[0]);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"<--> Leaderboard data: {responseJson}");
                
                try
                {
                    LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(responseJson);
                    callback(response.leaderboard);
                }
                catch (Exception e)
                {
                    Debug.LogError($"<--> Error parsing leaderboard data: {e.Message}");
                    callback(new LeaderboardEntry[0]);
                }
            }
        }
    }
}