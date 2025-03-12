using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuButton : MonoBehaviour
{
    public GameObject[] gameObjects;
    public Button toggleButton;
    public TcpClient1 tcpClient1;
    private bool State1;
    public string message;
    
    public ApiHandler apiHandler;
    public TcpServer tcpServer;
    
    // For end game button
    public bool isEndGameButton = false;

    void Start()
    {
        State1 = false;
        toggleButton.onClick.AddListener(ButtonTrigger);
    }

    void ButtonTrigger()
    {
        for (int i = 0; i < gameObjects.Length; i++){
            gameObjects[i].SetActive(!gameObjects[i].activeSelf);
        }

        State1 = !State1;

        if (State1){
            tcpClient1.SendDataToServer(message);
            
            if (isEndGameButton && tcpClient1.isStart) {
                SubmitGameScores();
            }
        }
    }
    
    private void SubmitGameScores()
    {
        // If server, submit scores for both players
        if (tcpServer != null && tcpServer.isStart) {
            string player1Id = tcpServer.clients.Values.Count > 0 ? tcpServer.clients.Values[0] : "Player1";
            string player2Id = tcpServer.clients.Values.Count > 1 ? tcpServer.clients.Values[1] : "Player2";
            
            int player1Score = tcpServer.health[0].enemyScore;
            int player2Score = tcpServer.health[1].enemyScore;
            
            Debug.Log($"Submitting scores - {player1Id}: {player1Score}, {player2Id}: {player2Score}");
            
            StartCoroutine(apiHandler.SubmitScore(player1Id, player1Score, (success, response) => {
                if (success) {
                    Debug.Log($"Successfully submitted score for {player1Id}");
                }
            }));
            
            StartCoroutine(apiHandler.SubmitScore(player2Id, player2Score, (success, response) => {
                if (success) {
                    Debug.Log($"Successfully submitted score for {player2Id}");
                }
            }));
        }
        // If client, submit own score
        else if (tcpClient1 != null && tcpClient1.isStart) {
            int playerScore = tcpClient1.playerScore[tcpClient1.playerID];
            string playerId = tcpClient1.clientId;
            
            Debug.Log($"Submitting client score - {playerId}: {playerScore}");
            
            StartCoroutine(apiHandler.SubmitScore(playerId, playerScore, (success, response) => {
                if (success) {
                    Debug.Log($"Successfully submitted score for {playerId}");
                }
            }));
        }
    }
}