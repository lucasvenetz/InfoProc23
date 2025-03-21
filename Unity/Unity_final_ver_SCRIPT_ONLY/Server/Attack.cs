using UnityEngine;

public class Attack : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;
    private float attackDuration = 0.5f;
    public GameObject Reference;
    public Vector3 direction;

    private bool isAttacking = false;
    private bool hasDealtDamage = false;
    private float attackTimer = 0f;


    public void EnableAttack(float duration, float damage){
        attackDamage = damage;
        attackDuration = duration;
        isAttacking = true;
        hasDealtDamage = false;
        attackTimer = 0f;
    }

    void Update()
    {
        Quaternion rotation = Reference.transform.rotation;

        Vector3 forwardDirection = rotation * Vector3.forward;
        Vector3 rightDirection = rotation * Vector3.right;
        Vector3 upDirection = rotation * Vector3.up;

        direction = forwardDirection + rightDirection + upDirection;

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer <= attackDuration && !hasDealtDamage)
            {
                DetectEnemies();
            }

            if (attackTimer >= attackDuration)
            {
                EndAttack();
            }
        }
    }

    void DetectEnemies()
    {
        RaycastHit2D hit = Physics2D.Raycast(Reference.transform.position, direction, attackRange, enemyLayer);

        if (hit.collider != null)
        {
            hit.collider.GetComponent<Health>().TakeDamage(attackDamage);
            hit.collider.GetComponent<Health>().ConsumeEnergy(attackDamage);
            hasDealtDamage = true;
        }
    }

    void EndAttack()
    {
        isAttacking = false;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Reference.transform.position, Reference.transform.position + direction * attackRange);
    }
}