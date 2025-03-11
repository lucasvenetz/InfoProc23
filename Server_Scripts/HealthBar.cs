using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform healthBarForeground;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthRatio = currentHealth / maxHealth;
        healthBarForeground.localScale = new Vector3(healthRatio, 1, 1);
    }
}