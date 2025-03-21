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
    
    [SerializeField]
    private TMP_Dropdown sortDropdown;
    
    private List<GameObject> entryObjects = new List<GameObject>();
    private LeaderboardEntry[] currentLeaderboardData;
    
    private enum SortType
    {
        TotalScore,
        HighestScore,
        MostGamesPlayed
    }
    
    private SortType currentSortType = SortType.TotalScore;
    
    private void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(FetchLeaderboard);
        }
        
        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
            
            // Add options to the dropdown if they don't exist
            if (sortDropdown.options.Count == 0)
            {
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Total Score"));
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Highest Score"));
                sortDropdown.options.Add(new TMP_Dropdown.OptionData("Most Games Played"));
                sortDropdown.RefreshShownValue();
            }
        }
        
        FetchLeaderboard();
    }
    
    private void OnSortChanged(int index)
    {
        currentSortType = (SortType)index;
        
        if (currentLeaderboardData != null && currentLeaderboardData.Length > 0)
        {
            DisplayLeaderboard(currentLeaderboardData);
        }
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
        
        currentLeaderboardData = leaderboardEntries;
        
        DisplayLeaderboard(leaderboardEntries);
    }
    
    private void DisplayLeaderboard(LeaderboardEntry[] leaderboardEntries)
    {
        ClearLeaderboard();
        
        System.Array.Sort(leaderboardEntries, (a, b) => {
            switch (currentSortType)
            {
                case SortType.TotalScore:
                    return b.totalScore.CompareTo(a.totalScore);
                case SortType.HighestScore:
                    return b.score.CompareTo(a.score);
                case SortType.MostGamesPlayed:
                    return b.gamesPlayed.CompareTo(a.gamesPlayed);
                default:
                    return b.totalScore.CompareTo(a.totalScore);
            }
        });
        
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
        
        LeaderboardEntryPrefab entryScript = entryObject.GetComponent<LeaderboardEntryPrefab>();
        if (entryScript != null)
        {
            entryScript.SetData(rank, entry);
        }
        else
        {
            // just in case script isn't found
            
            TextMeshProUGUI rankText = entryObject.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            if (rankText != null)
            {
                rankText.text = rank.ToString();
            }
            
            TextMeshProUGUI playerIdText = entryObject.transform.Find("PlayerIdText")?.GetComponent<TextMeshProUGUI>();
            if (playerIdText != null)
            {
                playerIdText.text = entry.playerId;
            }
            
            // Set high score
            TextMeshProUGUI highScoreText = entryObject.transform.Find("HighScoreText")?.GetComponent<TextMeshProUGUI>();
            if (highScoreText != null)
            {
                highScoreText.text = entry.score.ToString();
            }
            
            // Set total score
            TextMeshProUGUI totalScoreText = entryObject.transform.Find("TotalScoreText")?.GetComponent<TextMeshProUGUI>();
            if (totalScoreText != null)
            {
                totalScoreText.text = entry.totalScore.ToString();
            }
            
            // Set games played
            TextMeshProUGUI gamesPlayedText = entryObject.transform.Find("GamesPlayedText")?.GetComponent<TextMeshProUGUI>();
            if (gamesPlayedText != null)
            {
                gamesPlayedText.text = entry.gamesPlayed.ToString();
            }
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
    
    public void ToggleLeaderboard(bool show)
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(show);
            
            if (show)
            {
                FetchLeaderboard();
            }
        }
    }
}