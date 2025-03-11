using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D RB2D;
    private GroundHeightChecker GHC;
    private Attack attack;
    private Health health;
    public Health enemyhealth;
    public int direction = 1;
    public float moveForce = 500f;
    public float jumpForce = 7000f;
    public float walkSpeed = 12f;
    public float minSpeed = 0f;
    public float height;

    public int[] Keys = new int[7];
    public int[] KeysCashe = new int[7];

    public int moving_state = 0;
    private float startTime;
    public float dashDistance = 3.5f; 
    public float dashDuration = 0.1f;
    public float stepDistance = 3.5f; 
    public float stepDuration = 0.1f;
    public float slashDistance = 1f; 
    public float slashDuration = 0.1f;
    public float jumpDuration = 0.25f;
    private float speed;

    public float dashPostDelayDuration = 0.25f;
    public float stepPostDelayDuration = 0.3f;
    public float slashPostDelayDuration = 0.15f;
    private float postDelayEndTime;

    private Queue<System.Action> commandQueue = new Queue<System.Action>();
    public TcpServer tcpServer;

    void Start()
    {
        animator = GetComponent<Animator>();
        RB2D = GetComponent<Rigidbody2D>();
        GHC = GetComponent<GroundHeightChecker>();
        attack = GetComponent<Attack>();
        health = GetComponent<Health>();
        RB2D.freezeRotation = true; 
    }

    void Update()
    {
        animator.ResetTrigger("Stop");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Stab");
        animator.ResetTrigger("Slash");
        animator.ResetTrigger("BackStep");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Land");

        height = GHC.GetHeightAboveGround();
        /*
        if (Input.GetMouseButtonDown(0)) 
        {
            Attack();
        }

        if (Input.GetKey(KeyCode.A) && moving_state == 0)
        {
            MoveLeft();
            LimitVelocity(walkSpeed);
        }

        if (Input.GetKey(KeyCode.D) && moving_state == 0)
        {
            MoveRight();
            LimitVelocity(walkSpeed);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            BackStep();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Slash();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            StopMoving();
            if (height == 1){
                LimitVelocity(minSpeed);
            }
        } 
        */

        for (int i = 0; i < 7; i++){
            Keys[i] = tcpServer.Keys[0,i]; // ensuring that there is no subroutine synchronising problem and we make a copy
        }

        if (!health.isFailed && tcpServer.isStart){
            if (KeyChange(1, 6)) 
            {
                Attack();
            }

            if (KeyStay(1, 1) && moving_state == 0)
            {
                MoveLeft();
                LimitVelocity(walkSpeed);
            }

            if (KeyStay(1, 3) && moving_state == 0)
            {
                MoveRight();
                LimitVelocity(walkSpeed);
            }

            if (KeyChange(1, 2))
            {
                BackStep();
            }

            if (KeyChange(1, 4))
            {
                Slash();
            }

            if (KeyChange(1, 0))
            {
                Jump();
            }

            if (KeyChange(0, 3) || KeyChange(0, 1))
            {
                StopMoving();
                if (height == 1){
                    LimitVelocity(minSpeed);
                }
            } 
        }

        if (KeyChange(1, 5))
        {
            health.Revive();
            enemyhealth.Revive();
        }

        /*
        for (int i = 0; i < 2; i++) // test
        {
            if (KeyChange(1, i) || KeyChange(0, i)){
                Debug.Log(("Change", i,KeysCashe[i], tcpServer.Keys1[i]));
            }
            if (KeyStay(1, i) || KeyStay(0, i)){
                Debug.Log(("Stay", i, KeysCashe[i], tcpServer.Keys1[i]));
            }
        }
        */

        for (int i = 0; i < 7; i++){
            KeysCashe[i] = Keys[i]; // Make a copy, not moving pointer
        }


        if (moving_state == 1) // Dash forward
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime < dashDuration)
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }
            else
            {
                moving_state = -1;
                postDelayEndTime = Time.time + dashPostDelayDuration;
            }
        }

        if (moving_state == 2) // Back-step
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime < dashDuration)
            {
                transform.Translate(Vector3.right * speed * Time.deltaTime);
            }
            else
            {
                moving_state = -2;
                postDelayEndTime = Time.time + stepPostDelayDuration;
            }
        }

        if (moving_state == 3) // Slash
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime < dashDuration)
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }
            else
            {
                moving_state = -3;
                postDelayEndTime = Time.time + slashPostDelayDuration;
            }
        }

        if (moving_state == -1)
        {
            if (Time.time >= postDelayEndTime)
            {
                moving_state = 0;
            }
        }

        if (moving_state == -2)
        {
            if (Time.time >= postDelayEndTime)
            {
                moving_state = 0;
            }
        }

        if (moving_state == -3)
        {
            if (Time.time >= postDelayEndTime)
            {
                moving_state = 0;
            }
        }

        if (moving_state == 0)
        {
            if (commandQueue.Count > 0)
            {
                System.Action nextCommand = commandQueue.Dequeue();
                nextCommand.Invoke();
            }
        }

        if (moving_state == 4)
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime >= jumpDuration)
            {
                moving_state = -4;
                animator.SetTrigger("Land");
            }
        }

        if (moving_state == -4)
        {
            if (height == 1)
            {
                moving_state = 0;
            }
        }

        if (moving_state == 5) // Slash
        {
            float elapsedTime = Time.time - startTime;

            if (elapsedTime < dashDuration)
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }
            else
            {
                moving_state = -5;
                postDelayEndTime = Time.time + slashPostDelayDuration;
            }
        }

        if (moving_state == -5)
        {
            if (Time.time >= postDelayEndTime)
            {
                moving_state = 0;
            }
        }

        
    }

    bool KeyStay(int level, int KeyCode){
        return (Keys[KeyCode] == level && KeysCashe[KeyCode] == Keys[KeyCode]);
    }

    bool KeyChange(int level, int KeyCode){
        return (Keys[KeyCode] == level && KeysCashe[KeyCode] != Keys[KeyCode]);
    }

    void Attack()
    {
        if (health.EnergyLeft()){
            if (moving_state != -1 && moving_state != -3 && moving_state != 1 && moving_state != 3 && moving_state != -5 && moving_state != 5){
                speed = dashDistance / dashDuration;
                startTime = Time.time;
                moving_state = 1;
                animator.SetTrigger("Stab");
                attack.EnableAttack(0.25f, 45f);
                health.ConsumeEnergy(10f);
            }
            else{
                commandQueue.Enqueue(() => Attack());
            }
        }
    }

    void Jump()
    {
        if (health.EnergyLeft()){
            if (height == 1){
                if (moving_state != 4){
                    animator.SetTrigger("Jump");
                    RB2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
                    moving_state = 4;
                    startTime = Time.time;
                    health.ConsumeEnergy(10f);
                }
                else{
                    commandQueue.Enqueue(() => Jump());
                }
            }
        }
    }

    void Slash()
    {
        if (health.EnergyLeft()){
            if (moving_state == -4 && moving_state != -5 && moving_state != 5){
                speed = 0f;
                startTime = Time.time;
                moving_state = 5;
                animator.SetTrigger("Slash2");
                RB2D.AddForce(Vector2.down * jumpForce, ForceMode2D.Force);
                attack.EnableAttack(0.5f, 50f);
                health.ConsumeEnergy(10f);
            }
            else if (moving_state != -1 && moving_state != -3 && moving_state != 1 && moving_state != 3 && moving_state != -2 && moving_state != 2 && moving_state != -5 && moving_state != 5){
                speed = slashDistance / slashDuration;
                startTime = Time.time;
                moving_state = 3;
                animator.SetTrigger("Slash");
                attack.EnableAttack(0.25f, 30f);
                health.ConsumeEnergy(10f);
            }
            else{
                commandQueue.Enqueue(() => Slash());
            }
        }
        
    }

    void BackStep()
    {
        if (moving_state != -2 && moving_state != 2 && moving_state != -5 && moving_state != 5){
            speed = stepDistance / stepDuration;
            startTime = Time.time;
            moving_state = 2;
            animator.SetTrigger("BackStep");
        }
        else{
            commandQueue.Enqueue(() => BackStep());
        }
    }

    void MoveLeft()
    {
        if (moving_state != 4 && moving_state != -4){
            direction = 1;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            animator.SetTrigger("Walk");
            RB2D.AddForce(Vector2.left * direction * moveForce, ForceMode2D.Force);
        }
    }

    void MoveRight()
    {
        if (moving_state != 4 && moving_state != -4){
            direction = -1;      
            transform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetTrigger("Walk");
            RB2D.AddForce(Vector2.left * direction * moveForce, ForceMode2D.Force);
        }
    }

    void StopMoving()
    {
        animator.SetTrigger("Stop");
    }

    void LimitVelocity(float speed)
    {
        if (RB2D.linearVelocity.magnitude > speed)
        {
            RB2D.linearVelocity = RB2D.linearVelocity.normalized * speed;
        }
    }
}
