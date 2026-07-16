using UnityEngine;

public class GroundedState : PlayerState
{
    private float moveSpeed;
    //private InteractionDetector interactionDetector;
    private float groundCheckCooldown = 0.1f;
    private float groundCheckTimer = 0f;
    private float rollCheckCooldown = .6f;
    private float rollCheckTimer = 0f;
    private float movementSmoothing = 7.5f;

    //gracePeriod you can jump while being not grounded 
    private float gracePeriod;
    private float coyoteTimer = 0f; 
    //float facingDirection;

    private float wallRegrabCooldown = 0.1f; // how long until you can re-grab wall
    private float wallRegrabTimer = 0f;

    private int rollCost;
    private int lightAttackCost;
    private int heavyAttackCost;

    private float lastDirectionX;
    private float currentDirectionX;

    private readonly bool resetAnims;

    private Vector2 colliderSize;
    private float slopeDownAngle;
    private float slopeDownAngleOld;
    private float slopeSideAngle;
    private Vector2 slopeNormalPerpendicular;
    private bool onSlope;


    private readonly float slopeCheckDistance = .5f; //move to config file

    private PhysicsMaterial2D fullFriction;
    private PhysicsMaterial2D noFriction;

    public GroundedState(StateMachine stateMachine, PlayerStateConfig config, bool resetAnims) : base(stateMachine, config)
    {
        moveSpeed = config.moveSpeed;
        gracePeriod = config.gracePeriod;
        rollCost = config.rollCost;
        lightAttackCost = config.lightAttackCost;
        heavyAttackCost = config.heavyAttackCost;
        noFriction = config.noFriction;
        fullFriction = config.fullFriction;
        this.resetAnims = resetAnims;
        colliderSize = playerCollider.size;
    }

    public override void Enter()
    {   
        
        if(resetAnims){
            // animator.Play("movement Body", 0, 0f);
            // animator.Play("movement Legs", 1, 0f);
        }   
        animator.SetBool("grounded", true);
        animator.SetBool("running", true);
        input.ConsumeRoll();
        
        
        groundCheckTimer = groundCheckCooldown; // Start with cooldown
        rollCheckTimer = rollCheckCooldown;
        weaponSpriteRenderer.enabled = true;
        if (animator.GetBool("rolling"))
            return;
    }

    public override void Update()
    {   
        base.Update();
        if (wallRegrabTimer > 0f)
            wallRegrabTimer -= Time.deltaTime;

        groundCheckTimer = Mathf.Max(0f, groundCheckTimer - Time.deltaTime);
        rollCheckTimer = Mathf.Max(0f, rollCheckTimer - Time.deltaTime);
        

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));

        
        
        if(Mathf.Abs(rb.linearVelocityX) < .01f)
        {
            legsSpriteRenderer.enabled = false;
            
        }
        else
        {
            //weaponSpriteRenderer.enabled = true;
            legsSpriteRenderer.enabled = true;
            
        }

        if (IsGrounded())
        {
            coyoteTimer = gracePeriod;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            animator.SetBool("grounded", false);
        }



        ChangeState(); //check if possible to change state

        if (CheckDirectionChange())
        {
            Debug.Log("true");
            animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
            animator.Play("movement Body", 0, 0.0f);
            animator.Play("movement Legs", 1, 0.0f);
        }

        if (Mathf.Abs(input.HorizontalInput) > 0.01f)
        {
            FlipX();
        }
        
        
    }

    private void FlipX()
    {
        bool flip = input.HorizontalInput < 0;
        torsoSpriteRenderer.flipX = flip;
        legsSpriteRenderer.flipX = flip;
        weaponSpriteRenderer.flipX = flip;
    }

    public override void FixedUpdate()
    {
        slopeCheck();
        applyMovement();
    }

    private void applyMovement()
    {
        // if (Mathf.Abs(input.HorizontalInput) < 0.01f && IsGrounded())
        //     material.friction = 5f; 
        // else
        //     material.friction = 0f;
        if(!onSlope){
            rb.sharedMaterial = noFriction;
            float targetVelocityX = input.HorizontalInput * moveSpeed;
            float velocityDifferenceX = targetVelocityX - rb.linearVelocity.x;
    
            // Apply force to reach target velocity
            rb.AddForce(new Vector2(velocityDifferenceX * movementSmoothing * rb.mass, 0f), ForceMode2D.Force);
    
            if (Mathf.Abs(input.HorizontalInput) == 0)
            {
                float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x),1f); 
                amount *= Mathf.Sign(rb.linearVelocity.x);
                rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
            }
        }
        else{
            if (Mathf.Abs(input.HorizontalInput) == 0){
                // if (onSlope)
                // {
                //     rb.sharedMaterial = fullFriction;
                // }
                rb.sharedMaterial = fullFriction;
                // float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x),1f); 
                // amount *= Mathf.Sign(rb.linearVelocity.x);
                // rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
                //rb.linearVelocity = Vector3.zero;
            } else{
                rb.sharedMaterial = noFriction;
                float targetVelocityX = input.HorizontalInput * moveSpeed;
                float velocityDifferenceX = targetVelocityX - rb.linearVelocity.x;
        
                // Apply force to reach target velocity
                rb.AddForce(new Vector2(
                    -velocityDifferenceX * movementSmoothing * rb.mass * slopeNormalPerpendicular.x, 
                    moveSpeed * slopeNormalPerpendicular.y * -input.HorizontalInput), 
                    ForceMode2D.Force);
            }
            
        }
        
    }

    private void ChangeState()
    {
        if (wallRegrabTimer <= 0f && IsWalled(out float mrow) && !IsGrounded() && Mathf.Abs(input.HorizontalInput) > 0.01f)
        {
            //wallclimbing state
            legsSpriteRenderer.enabled = false;
            weaponSpriteRenderer.enabled = false;
            wallRegrabTimer = wallRegrabCooldown;
            animator.SetBool("grounded", false);
            animator.SetBool("running", false);
            //Debug.Log("entering wall cloimbing state from fall or standing");

            stateMachine.ChangeState(stateMachine.States.WallClimbing());
            return;
        }else  if ((input.JumpPressed && groundCheckTimer <= 0f && IsGrounded()) || (input.JumpPressed && groundCheckTimer <= 0f && coyoteTimer > 0f))
        {
            //jumping state
            legsSpriteRenderer.enabled = false;
            wallRegrabTimer = wallRegrabCooldown;
            animator.SetBool("grounded", false);
            animator.SetBool("running", false);

            stateMachine.ChangeState(stateMachine.States.Jumping(new Vector2(0f, config.jumpForce)));
            return;
        }
        else if(!IsGrounded()){
            //falling if not on ground
            legsSpriteRenderer.enabled = false;
            animator.SetBool("grounded", false);
            animator.SetBool("running", false);

            stateMachine.ChangeState(stateMachine.States.Falling());
        }
        else if (input.RollPressed && IsGrounded() && ConsumeStamina(rollCost))
        {   
            //roll state
            legsSpriteRenderer.enabled = false;
            weaponSpriteRenderer.enabled = false;
            animator.SetBool("grounded", false);
            stateMachine.ChangeState(stateMachine.States.Rolling(moveSpeed));
        }
        else if (input.HeavyAttackPressed && ConsumeStamina(heavyAttackCost))
        {
            //heavy attack
            legsSpriteRenderer.enabled = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("running", false);
            stateMachine.ChangeState(stateMachine.States.HeavyAttack());
        }
        else if (input.LightAttackPressed && ConsumeStamina(lightAttackCost))
        {
            //light attack
            //rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            weaponSpriteRenderer.enabled = false;
            animator.SetBool("running", false);
            stateMachine.ChangeState(stateMachine.States.LightAttack());
        }
        else
        {
            //otherwise move to running
            animator.SetBool("running", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }

    private void slopeCheck()
    {
        //Vector2 checkPos = player.transform.position - new Vector3(0f, colliderSize.y/2);
        SlopeCheckHorizontal(player.transform.position);
        SlopeCheckVertical(player.transform.position);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, 
            player.transform.right, 
            slopeCheckDistance, 
            groundMask | platformMask);
        
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, 
        -player.transform.right, 
        slopeCheckDistance, 
        groundMask | platformMask);
        
        Debug.DrawRay(checkPos, player.transform.right * slopeCheckDistance, Color.green);
        Debug.DrawRay(checkPos, -player.transform.right * slopeCheckDistance, Color.cyan);
        
        if (slopeHitFront)
        {
            onSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
            
            Debug.DrawRay(slopeHitFront.point, slopeHitFront.normal, Color.yellow);
            Debug.DrawRay(slopeHitFront.point, Vector2.up, Color.magenta);
        } 
        else if (slopeHitBack)
        {
        onSlope = true;
        slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        
        Debug.DrawRay(slopeHitBack.point, slopeHitBack.normal, Color.red);
        Debug.DrawRay(slopeHitBack.point, Vector2.up, Color.magenta);
        }
        else
        {
            slopeSideAngle = 0f;
            onSlope = false;
        }
    }    
    private void SlopeCheckVertical(Vector2 checkPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPos, 
        Vector2.down, 
        slopeCheckDistance, 
        groundMask | platformMask);

        if (hit)
        {
            slopeNormalPerpendicular = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);


            if(slopeDownAngle != slopeDownAngleOld)
            {
                onSlope = true;
            }
            slopeDownAngleOld = slopeDownAngle;

            // Debug.DrawRay(hit.point, hit.normal, Color.yellow);
            // Debug.DrawRay(hit.point, slopeNormalPerpendicular, Color.red);
        }
    }

    private bool CheckDirectionChange()
    {
        currentDirectionX = input.HorizontalInput;
        bool directionChanged = (currentDirectionX > 0.01f && lastDirectionX < -0.01f) 
                            || (currentDirectionX < -0.01f && lastDirectionX > 0.01f);
        if (directionChanged)
        {
            lastDirectionX = currentDirectionX;
        }
        return directionChanged;
    }
}