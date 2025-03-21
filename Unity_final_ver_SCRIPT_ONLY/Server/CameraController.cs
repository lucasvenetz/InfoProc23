using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform objectA;
    public Transform objectB;
    public float maxDistance = 10f;
    private Camera mainCamera;
    public float smoothSpeed = 0.125f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        float xA = objectA.position.x;
        float xB = objectB.position.x;

        float midPoint = (xA + xB) / 2f;

        Vector3 targetPosition = mainCamera.transform.position;
        targetPosition.x = midPoint;


        float distance = Mathf.Abs(xA - xB);
        if (distance > maxDistance)
        {
            targetPosition.x = objectA.position.x;
            
        }

        Vector3 smoothedPosition = Vector3.Lerp(mainCamera.transform.position, targetPosition, smoothSpeed);
        mainCamera.transform.position = smoothedPosition;
    }
}
