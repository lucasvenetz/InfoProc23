using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxEnergy = 100f;
    public float currentHealth;
    public float currentEnergy;
    public HealthBar healthBar;
    public HealthBar energyBar;
    public Animator animator;
    public int player; // 1 or 2

    public TcpClient1 tcpClient;

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
    }

    void Update(){
        ConsumeEnergy(-0.2f);
        animator.ResetTrigger("Revive");
        animator.ResetTrigger("Fail");
        if (currentHealth == 0f){
            animator.SetTrigger("Fail");
        }
        
        currentHealth = tcpClient.playerHealthData[player-1];
        currentEnergy = tcpClient.playerHealthData[1+player];
        healthBar.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth > 0){
            animator.SetTrigger("StandUp");
        }
        if (currentHealth == maxHealth && currentEnergy == maxEnergy){
            Revive();
        }
    }

    public void Revive(){
        animator.SetTrigger("Revive");
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        animator.SetTrigger("BeingHit");
        if (currentHealth - damage < 0f){
            currentHealth = 0f;
        }
        else if (currentHealth - damage > 100){
            currentHealth = 100f;
        }
        else{
            currentHealth -= damage;
        }
    }

    public void ConsumeEnergy(float damage)
    {
        if (currentEnergy - damage < 0f){
            currentEnergy = 0f;
        }
        else if (currentEnergy - damage > 100f){
            currentEnergy = 100f;
        }
        else{
            currentEnergy -= damage;
        }
        if (energyBar == null){
            return;
        }
        energyBar.UpdateHealthBar(currentEnergy, maxEnergy);
    }

    public bool EnergyLeft(){
        return (currentEnergy < 10 ? false : true);
    }
}
