using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextTcpRenew : MonoBehaviour
{
    public TcpClient1 tcpClient1;
    public TMP_Text text1;
    public string _right;
    public string _left;
    public int index;

    void Update()
    {
        text1.text = _left + tcpClient1.TextArray[index] + _right;
    }
}
