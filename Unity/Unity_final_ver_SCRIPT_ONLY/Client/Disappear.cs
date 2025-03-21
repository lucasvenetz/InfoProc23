using UnityEngine;

public class Disappear : MonoBehaviour
{
    public TcpClient1 tcpClient1;
    public GameObject UICanvas;
    void Update()
    {
        if (tcpClient1.isStart){
            UICanvas.SetActive(false);
        }
    }
}
