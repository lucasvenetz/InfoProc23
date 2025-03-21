using UnityEngine;
using UnityEngine.UI;

public class SingleButton : MonoBehaviour
{
    public GameObject[] gameObjects;
    public Button toggleButton;
    public bool State1;

    void Start()
    {
        toggleButton.onClick.AddListener(ButtonTrigger);
    }

    void ButtonTrigger()
    {
        for (int i = 0;  i < gameObjects.Length; i++){
            gameObjects[i].SetActive(State1);
        }
    }
}