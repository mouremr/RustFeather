using System;
using Unity.VisualScripting;
using UnityEngine;
public class WallClimbingState : PlayerState
{

    float yVelocity;

    float facingDirection;

    private float wallExitCooldown = 0.07f;
    //private LayerMask climbableMask;
    private float wallExitTimer = 0f;
    private float dynoCooldownTimer = .6f;
    private int dynoCost = 10;
    public WallClimbingState(StateMachine stateMachine, PlayerStateConfig config) : base(stateMachine, config)
    {
        facingDirection = torsoSpriteRenderer.flipX ? -1f : 1f;
        wallExitTimer = wallExitCooldown; // start timer
        //climbableMask = LayerMask.GetMask("Climbable");

    }

    public override void Enter()
    {
        animator.SetBool("climbing", true);

        int wallSide = GetWallSide();
        if (wallSide == -1)
            torsoSpriteRenderer.flipX = false; // face right
        else if (wallSide == 1)
            torsoSpriteRenderer.flipX = true;  // face left

        facingDirection = wallSide;

    }
    private int GetWallSide()
    {
        Vector2 hipOrigin = (Vector2)player.transform.position + Vector2.up * 1f;
        float rayLength = 0.4f;

        if (Physics2D.Raycast(hipOrigin, Vector2.left, rayLength,climbableMask))
            return 1; // wall on left

        if (Physics2D.Raycast(hipOrigin, Vector2.right, rayLength,climbableMask))
            return -1; // wall on right
        return 0;
    }

    public override void Update()
    {
        float currentY=2f; // units per second

        animator.SetFloat("yVelocity",rb.linearVelocity.y);


        if (!(Mathf.Abs(Input.GetAxis("Vertical") )> 0.01f)) //fall or climb normally
        {
            currentY = -1.2f;
            rb.linearVelocity = new Vector2(0f, currentY); //prevent horizontal movement

        }
        else if (Input.GetKey(KeyCode.W))
        {
            currentY = Mathf.Lerp(currentY, -2f, Time.deltaTime);
            rb.linearVelocity = new Vector2(0f, currentY); //prevent horizontal movement

        }
        float wallDir = torsoSpriteRenderer.flipX ? -1f : 1f;
        if(Math.Sign(Input.GetAxis("Horizontal")) == Math.Sign(-wallDir) && input.JumpPressed){
            animator.SetBool("climbing", false);
            Debug.Log("push away from wall");
            rb.linearVelocity = Vector2.zero;
            float pushX = 2f * -wallDir; 
            float pushY= 5f;
            
            stateMachine.ChangeState(stateMachine.States.Jumping(new Vector2(pushX,pushY)));
            return;

        }

        // if  (input.JumpPressed && ConsumeStamina(dynoCost) && Mathf.Abs(Input.GetAxis("Horizontal")) < 0.01f) // dyno up
        // {
        //     animator.SetBool("climbing", false);
        //     float pushX = 0;
        //     float pushY =15f;    

        //     stateMachine.ChangeState(new JumpingState(stateMachine, new Vector2(pushX,pushY), config));   
        //     return;
        // }

        if (wallExitTimer > 0f)
            wallExitTimer -= Time.deltaTime;
            
        dynoCooldownTimer = Mathf.Max(0f, dynoCooldownTimer - Time.deltaTime);


        if (wallExitTimer <= 0f &&   input.HorizontalInput != 0 && Mathf.Sign(input.HorizontalInput) != facingDirection && IsGrounded())        {
            animator.SetBool("climbing", false);
            stateMachine.ChangeState(stateMachine.States.Grounded(true));
            return;
        }
        if (canMantle())
        {
            animator.SetBool("climbing", false);
            stateMachine.ChangeState(stateMachine.States.Mantling());
            return;
        }
        if (!IsWalled(out float dum)) //slid off wall
        {
            animator.SetBool("climbing", false);
            stateMachine.ChangeState(stateMachine.States.Falling());   
            return;
        }

    }




}