using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform objectA;
    public Transform objectB;
    public float maxDistance = 10f;
    private Camera mainCamera;
    public float smoothSpeed = 0.125f;

    public TcpClient1 tcpClient1;

    public GameObject[] Arrows;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        float xA = objectA.position.x;
        float xB = objectB.position.x;
        float yA = objectA.position.y;
        float yB = objectB.position.y;

        float midPointX = (xA + xB) / 2f;
        float midPointY = (yA + yB) / 2f;

        Vector3 targetPosition = mainCamera.transform.position;
        targetPosition.x = midPointX;
        targetPosition.y = midPointY;

        Arrows[0].SetActive(false);
        Arrows[1].SetActive(false);

        float distance = Mathf.Abs(xA - xB);
        if (distance > maxDistance)
        {
            targetPosition.x = objectA.position.x;
            targetPosition.y = objectA.position.y;
            if (xB < xA){
                Arrows[0].SetActive(true);
            }
            else{
                Arrows[1].SetActive(true);
            }
        }

        if (!tcpClient1.isStart){
            Arrows[0].SetActive(false);
            Arrows[1].SetActive(false);
        }

        Vector3 smoothedPosition = Vector3.Lerp(mainCamera.transform.position, targetPosition, smoothSpeed);
        mainCamera.transform.position = smoothedPosition;
    }
}
