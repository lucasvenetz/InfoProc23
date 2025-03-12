using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField]
    private ApiHandler apiHandler;
    
    [SerializeField]
    private GameObject leaderboardPanel;
    
    [SerializeField]
    private GameObject leaderboardEntryPrefab;
    
    [SerializeField]
    private Transform leaderboardContainer;
    
    [SerializeField]
    private Button refreshButton;
    
    [SerializeField]
    private TextMeshProUGUI statusText;
    
    [SerializeField]
    private int maxEntriesToShow = 10;
    
    private List<GameObject> entryObjects = new List<GameObject>();
    
    private void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(FetchLeaderboard);
        }
        
        FetchLeaderboard();
    }
    
    public void FetchLeaderboard()
    {
        if (statusText != null)
        {
            statusText.text = "Loading leaderboard...";
        }
        
        ClearLeaderboard();
        
        StartCoroutine(apiHandler.GetLeaderboard(HandleLeaderboardData));
    }
    
    private void HandleLeaderboardData(LeaderboardEntry[] leaderboardEntries)
    {
        if (leaderboardEntries == null || leaderboardEntries.Length == 0)
        {
            if (statusText != null)
            {
                statusText.text = "No leaderboard data available.";
            }
            return;
        }
        
        if (statusText != null)
        {
            statusText.text = "";
        }
        
        System.Array.Sort(leaderboardEntries, (a, b) => b.score.CompareTo(a.score));
        
        int entriesToShow = Mathf.Min(leaderboardEntries.Length, maxEntriesToShow);
        
        for (int i = 0; i < entriesToShow; i++)
        {
            CreateLeaderboardEntry(i + 1, leaderboardEntries[i]);
        }
    }
    
    private void CreateLeaderboardEntry(int rank, LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null)
        {
            Debug.LogError("Leaderboard entry prefab or container not assigned!");
            return;
        }
        
        GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        entryObjects.Add(entryObject);
        
        // Set rank
        TextMeshProUGUI rankText = entryObject.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }
        
        // Set player ID
        TextMeshProUGUI playerIdText = entryObject.transform.Find("PlayerIdText")?.GetComponent<TextMeshProUGUI>();
        if (playerIdText != null)
        {
            playerIdText.text = entry.playerId;
        }
        
        // Set score
        TextMeshProUGUI scoreText = entryObject.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (scoreText != null)
        {
            scoreText.text = entry.score.ToString();
        }
    }
    
    private void ClearLeaderboard()
    {
        foreach (GameObject entry in entryObjects)
        {
            Destroy(entry);
        }
        
        entryObjects.Clear();
    }
    
    // Call this method to toggle the leaderboard visibility
    public void ToggleLeaderboard(bool show)
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(show);
            
            // Refresh the data when showing the leaderboard
            if (show)
            {
                FetchLeaderboard();
            }
        }
    }
}