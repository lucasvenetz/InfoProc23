using UnityEngine;

public class GroundHeightChecker : MonoBehaviour
{
    public float maxDistance = 100f; 
    public LayerMask groundLayer; 
    public float offset; 

    public float GetHeightAboveGround()
    {
        Vector3 rayStart = transform.position + Vector3.down * offset;
        Vector3 rayDirection = Vector3.down;

        if (Physics2D.Raycast(rayStart, rayDirection, maxDistance, groundLayer))
        {
            return 1f;
        }
        else{
            return -1f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.down * offset, transform.position + Vector3.down * (offset+maxDistance));
    }
}