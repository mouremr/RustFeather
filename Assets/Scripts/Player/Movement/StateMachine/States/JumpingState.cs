using Unity.VisualScripting;
using UnityEngine;

public class JumpingState : PlayerState
{
    private float moveSpeed = 5f;
    private float minAirTime = 0.1f; // Minimum time before checking for ground
    private float airTimer = 0f;
    private float airControl = 5f; 
    private Vector2 jumpVector;

    private float wallRegrabCooldown = 0.08f;
    private float wallRegrabTimer = 0f;


    private LayerMask manteableMask;
    private bool hasIncreasedGravity;

    public JumpingState(StateMachine stateMachine, PlayerStateConfig config, Vector2 jumpVector) : base(stateMachine, config)
    {
        this.jumpVector= jumpVector;
        manteableMask = LayerMask.GetMask("Mantleable");
    }

    public override void Enter()
    {
        hasIncreasedGravity = false;
        animator.SetBool("jumping", true);
        input.ConsumeRoll();
        wallRegrabTimer = wallRegrabCooldown;
        rb.gravityScale = 0f;
        rb.AddForce(jumpVector, ForceMode2D.Impulse);
        airTimer = 0f; // Reset timer
        rb.gravityScale = 1;
    }

    public override void Update()
    {
        base.Update();

        input.ConsumeRoll();//kill buffered rolls

        //Debug.Log("jumping state");
        airTimer += Time.deltaTime;
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        float targetVelocityX = input.HorizontalInput * moveSpeed;
        float velocityDiff = targetVelocityX - rb.linearVelocity.x;

        rb.AddForce(new Vector2(velocityDiff * airControl, 0f));

    

        if (input.JumpReleased && rb.linearVelocity.y > 0.1) //jump cut
        {
            rb.AddForce(new Vector2(0f, -rb.linearVelocity.y * 0.5f), ForceMode2D.Impulse);
        }
        if (rb.linearVelocity.y < -0.05f && !hasIncreasedGravity) //if falling, iuncrease gravity a little bit
        {
            rb.gravityScale += 0.3f;
            hasIncreasedGravity = true;
        }

    
        if (CanMantle())
        {
            animator.SetBool("jumping", false);
            animator.SetBool("climbing", false);
            animator.SetBool("mantling", true);

            
            stateMachine.ChangeState(stateMachine.PlayerStates.Mantling());
            return;
        }
        wallRegrabTimer -= Time.deltaTime;

        if (wallRegrabTimer <= 0f && IsWalled(out float wallDir))
        {
            animator.SetBool("jumping", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.WallClimbing());
            return;
        }

        else if (airTimer >= minAirTime && IsGrounded())
        {
            animator.SetBool("jumping", false);
            animator.SetBool("grounded", true);
            stateMachine.ChangeState(stateMachine.PlayerStates.Grounded());
            return;
        }


        if (Mathf.Abs(input.HorizontalInput) > 0.01f)
        {
            FlipX();
        }


    }

}