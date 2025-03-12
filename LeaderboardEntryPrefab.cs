using UnityEngine;
using TMPro;

/* something like this
 * LeaderboardEntry (GameObject)
 * ├── RankText (TextMeshProUGUI)
 * ├── PlayerIdText (TextMeshProUGUI)
 * └── ScoreText (TextMeshProUGUI)
 */

public class LeaderboardEntryPrefab : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI highscoreText;
    public TextMeshProUGUI totalscoreText;
    public TextMeshProUGUI gamesPlayedText;
    
    public void SetData(int rank, LeaderboardEntry entry)
    {
        if (rankText != null)
            rankText.text = rank.ToString();
            
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