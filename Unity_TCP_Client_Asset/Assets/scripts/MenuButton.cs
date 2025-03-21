using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public GameObject[] gameObjects;
    public Button toggleButton;
    public TcpClient1 tcpClient1;
    private bool State1;
    public string message;

    void Start()
    {
        State1 = false;
        toggleButton.onClick.AddListener(ButtonTrigger);
    }

    void ButtonTrigger()
    {
        for (int i = 0;  i < gameObjects.Length; i++){
            gameObjects[i].SetActive(!gameObjects[i].activeSelf);
        }

        State1 = !State1;

        if (State1){
            tcpClient1.SendDataToServer(message);
            Debug.Log(">>> Send: "+message);
        }
    }
}