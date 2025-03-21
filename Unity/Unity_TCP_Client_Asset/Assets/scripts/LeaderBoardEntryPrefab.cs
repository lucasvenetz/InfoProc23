using UnityEngine;
using TMPro;

public class LeaderboardEntryPrefab : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI gamesPlayedText;
    
    public void SetData(int rank, LeaderboardEntry entry)
    {
        if (rankText != null)
            rankText.text += rank.ToString();
            
        if (playerIdText != null)
            playerIdText.text = entry.playerId;
            
        if (highScoreText != null)
            highScoreText.text = entry.score.ToString();
            
        if (totalScoreText != null)
            totalScoreText.text = entry.totalScore.ToString();
            
        if (gamesPlayedText != null)
            gamesPlayedText.text = entry.gamesPlayed.ToString();
    }
}
