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
    public TextMeshProUGUI scoreText;
    
    public void SetData(int rank, string playerId, int score)
    {
        if (rankText != null)
            rankText.text = rank.ToString();
            
        if (playerIdText != null)
            playerIdText.text = playerId;
            
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}