using UnityEngine;

public class Health : MonoBehaviour
{
    public TcpServer tcpServer;
    public float maxHealth = 100f;
    public float maxEnergy = 100f;
    public float currentHealth;
    public float currentEnergy;
    public HealthBar healthBar;
    public HealthBar energyBar;
    public Animator animator;
    public bool isFailed;
    public int[] ID = new int[2];
    public float respownTime;
    private float startTime = 0f;

    public int enemyScore = 0;

    void Start()
    {
        isFailed = false;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
    }

    void Update(){
        ConsumeEnergy(-0.2f);
        animator.ResetTrigger("Revive");
        animator.ResetTrigger("Fail");

        if (transform.position.y < (NeariestHeight()+20f)){
            currentHealth = 0f;
        }

        if (currentHealth == 0f && !isFailed){
            Debug.Log("<> Player "+ID[0]+" failed");
            
            startTime = respownTime;
            isFailed = true;
            animator.SetTrigger("Fail");
        }

        if (currentHealth == 100){
            animator.SetTrigger("Revive");
            isFailed = false;
        }

        if (isFailed){
            startTime -= Time.deltaTime;
            Debug.Log("???"+startTime);
            if (startTime < 0f){
                Revive();
            }
        }
    }

    public void Revive(){
        enemyScore += 1;
        isFailed = false;
        animator.SetTrigger("Revive");
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
        ReviveLocation();
    }

    float NeariestHeight(){
        float minValue = 200f;
        int[,] Rec = tcpServer.mapGenerator.Record;
        int count = tcpServer.mapGenerator.count;
        int loc = 0;

        for (int i = 0; i < count; i++){
            if (Mathf.Abs(transform.position.x - Rec[i,0]) < minValue){
                minValue = Mathf.Abs(transform.position.x - Rec[i,0]);
                loc = i;
            }
        }

        return Rec[loc,2];
    }

    public void ReviveLocation(){
        int[,] Rec = tcpServer.mapGenerator.Record;
        int count = tcpServer.mapGenerator.count;
        bool ispass = false;
        float X1 = 0f;
        Vector3 p2pos = tcpServer.PlayerX[ID[1]].transform.position;

        while (!ispass){
            ispass = true;
            for (int i = 0; i < count; i++){
                X1 = ((float)(1-Random.Range(0, 2)*2))*Random.Range(30f, 100f) + p2pos.x;
                if ((X1 > Rec[i,0]-5) && (X1 < Rec[i,1]+5)){
                    ispass = false;
                    Debug.Log(">>> "+X1);
                    break;
                }
            }
        }

        tcpServer.PlayerX[ID[0]].transform.position = new Vector3(X1, p2pos.y, 0);
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
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
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
