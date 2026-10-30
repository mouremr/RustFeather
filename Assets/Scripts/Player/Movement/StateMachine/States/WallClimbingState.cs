using System;
using UnityEngine;

public class WallClimbingState : PlayerState
{
    private bool hanging;

    float yVelocity;

    float facingDirection;

    private float wallExitCooldown = 0.07f;
    //private LayerMask climbableMask;
    private float wallExitTimer = 0f;
    private float climbCost;
    public WallClimbingState(StateMachine stateMachine, PlayerStateConfig config) : base(stateMachine, config)
    {
        facingDirection = player.transform.localScale.x;
        wallExitTimer = wallExitCooldown; // start timer
        climbCost = config.climbCost;
        //climbableMask = LayerMask.GetMask("Climbable");

    }

    public override void Enter()
    {
        animator.SetBool("climbing", true);

        int wallSide = GetWallSide();
        if (wallSide == -1)
            player.transform.localScale = new Vector3(1f, 1f, 1f); // face right
        else if (wallSide == 1)
            player.transform.localScale = new Vector3(-1f, 1f, 1f);  // face left

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
        base.Update();
        
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
        if (wallExitTimer > 0f) wallExitTimer -= Time.deltaTime;

        float wallDir = player.transform.localScale.x;
        
        if(rb.linearVelocity.y > 0.01f)
        {
            if (!ConsumeStamina(climbCost * Time.deltaTime))
            {
                animator.SetBool("climbing", false);
                Debug.Log("Ran out of stamina!");
                rb.linearVelocity = Vector2.zero;
                float pushX = 10f * -wallDir; 
                float pushY= 2f;
                stateMachine.ChangeState(stateMachine.PlayerStates.Jumping(new Vector2(pushX,pushY)));
                return;
            }
        }

        


        ChangeState();

    }

    private void ChangeState()
    {

        if (input.InteractPressed && inventoryManager.ConsumeItem("Piton") && !stateMachine.InteractionDetector.HasInteractible)
        {
            Debug.Log("Placing piton!");
            stateMachine.ChangeState(stateMachine.PlayerStates.Hanging());
            
            Vector3 topOfCollider = new Vector3(
                player.transform.position.x,
                stateMachine.PlayerCollider.bounds.max.y,
                player.transform.position.z
            );

            UnityEngine.Object.Instantiate(
                config.pitonPrefab, 
                topOfCollider, 
                player.transform.rotation
            );
            
            //stateMachine.ChangeState(stateMachine.PlayerStates.Hanging());
            //animator.SetBool("Climbing", false);
            return;

        }
        float wallDir = player.transform.localScale.x;
        if(Math.Sign(Input.GetAxis("Horizontal")) == Math.Sign(-wallDir) && input.JumpPressed){
            animator.SetBool("climbing", false);
            rb.linearVelocity = Vector2.zero;
            float pushX = 2f * -wallDir; 
            float pushY= 5f;
            
            stateMachine.ChangeState(stateMachine.PlayerStates.Jumping(new Vector2(pushX,pushY)));
            return;
        }

        if (wallExitTimer <= 0f &&   input.HorizontalInput != 0 && Mathf.Sign(input.HorizontalInput) != facingDirection && IsGrounded())        {
            animator.SetBool("climbing", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.Grounded());
            return;
        }
        if (CanMantle())
        {
            animator.SetBool("climbing", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.Mantling());
            return;
        }
        if (!IsWalled(out float dum)) //slid off wall
        {
            animator.SetBool("climbing", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.Falling());   
            return;
        }
    }

    public override void Exit()
    {
        animator.SetBool("climbing", false);
    }


}