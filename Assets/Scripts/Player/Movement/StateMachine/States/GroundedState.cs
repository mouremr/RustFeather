using UnityEngine;

public class GroundedState : PlayerState
{
    private static readonly int RollingHash = Animator.StringToHash("rolling");
    private static readonly int RunningHash = Animator.StringToHash("running");
    private static readonly int GroundedHash = Animator.StringToHash("grounded");
    private static readonly int XVelocityHash = Animator.StringToHash("xVelocity");
    private static readonly int OnStairHash = Animator.StringToHash("OnStair");
    private float moveSpeed;
    private float groundCheckCooldown = 0.1f;
    private float groundCheckTimer = 0f;
    private float rollCheckCooldown = .6f;
    private float rollCheckTimer = 0f;
    private float movementSmoothing = 7.5f;

    //gracePeriod you can jump while being not grounded 
    private float gracePeriod;
    private float coyoteTimer = 0f; 
    private float wallRegrabCooldown = 0.1f; // how long until you can re-grab wall
    private float wallRegrabTimer = 0f;

    private int rollCost;
    private int lightAttackCost;
    private int heavyAttackCost;

    private float lastDirectionX;
    private float currentDirectionX;

    private Vector2 colliderSize;
    private float slopeDownAngle;
    private float slopeDownAngleOld;
    private float slopeSideAngle;
    private Vector2 slopeNormalPerpendicular;
    private bool onSlope;
    private bool wasOnSlope;

    private readonly float slopeCheckDistance = 0.75f;
    private readonly float slopeSnapDistance = 0.5f; // Extra distance to check downward to stick to slopes

    private PhysicsMaterial2D fullFriction;
    private PhysicsMaterial2D noFriction;

    public GroundedState(StateMachine stateMachine, PlayerStateConfig config) : base(stateMachine, config)
    {
        moveSpeed = config.moveSpeed;
        gracePeriod = config.gracePeriod;
        rollCost = config.rollCost;
        lightAttackCost = config.lightAttackCost;
        heavyAttackCost = config.heavyAttackCost;
        noFriction = config.noFriction;
        fullFriction = config.fullFriction;
        colliderSize = playerCollider.size;
    }

    public override void Enter()
    {
        
        //animator.Play("Ground Movement");   
        animator.SetBool(GroundedHash, true);
        animator.SetBool(RunningHash, true);
        input.ConsumeRoll();
        legsSpriteRenderer.enabled = true;
                
        groundCheckTimer = groundCheckCooldown; // Start with cooldown
        rollCheckTimer = rollCheckCooldown;
        weaponSpriteRenderer.enabled = true;
        if (animator.GetBool(RollingHash))
            return;
    }

    public override void Update()
    {   
        base.Update();
        if (wallRegrabTimer > 0f)
            wallRegrabTimer -= Time.deltaTime;

        groundCheckTimer = Mathf.Max(0f, groundCheckTimer - Time.deltaTime);
        rollCheckTimer = Mathf.Max(0f, rollCheckTimer - Time.deltaTime);
        
        animator.SetFloat(XVelocityHash, Mathf.Abs(rb.linearVelocity.x), 0.05f, Time.deltaTime);

        if (IsGrounded())
        {
            coyoteTimer = gracePeriod;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            animator.SetBool(GroundedHash, false);
        }

        animator.SetBool(OnStairHash, onSlope);

        ChangeState(); //check if possible to change state

        if (Mathf.Abs(input.HorizontalInput) > 0.01f)
        {
            FlipX();
        }
    }

    public override void FixedUpdate()
    {
        SlopeCheck();
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // zero out y velocity when leaving slope to smooth transition
        if (wasOnSlope && !onSlope)
        {
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }
        wasOnSlope = onSlope;

        if (!onSlope)
        {
            rb.sharedMaterial = noFriction;
            float targetVelocityX = input.HorizontalInput * moveSpeed;
            float velocityDifferenceX = targetVelocityX - rb.linearVelocity.x;
    
            rb.AddForce(new Vector2(velocityDifferenceX * movementSmoothing * rb.mass, 0f), ForceMode2D.Force);
    
            if (Mathf.Abs(input.HorizontalInput) == 0)
            {
                float amount = Mathf.Min(Mathf.Abs(rb.linearVelocity.x), 1f); 
                amount *= Mathf.Sign(rb.linearVelocity.x);
                rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
            }

            // snap down if walking downhill on a slope
            SnapToSlope();
        }
        else
        {
            if (Mathf.Abs(input.HorizontalInput) == 0)
            {
                rb.sharedMaterial = fullFriction;
            } 
            else
            {
                rb.sharedMaterial = noFriction;
                float targetVelocityX = input.HorizontalInput * moveSpeed;
                float velocityDifferenceX = targetVelocityX - rb.linearVelocity.x;
        
                rb.AddForce(new Vector2(
                    -velocityDifferenceX * movementSmoothing * rb.mass * slopeNormalPerpendicular.x, 
                    moveSpeed * slopeNormalPerpendicular.y * -input.HorizontalInput), 
                    ForceMode2D.Force);

                // cap vertical velocity to prevent launching off slopes
                if (rb.linearVelocity.y > moveSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveSpeed);
                }
            }
        }
    }

    private void SnapToSlope()
    {
        // Raycast down slightly further than normal check to detect step-down slopes early
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, Vector2.down, slopeCheckDistance + slopeSnapDistance, groundMask | platformMask);

        if (hit && Mathf.Abs(input.HorizontalInput) > 0.01f)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            
            // If ground below is a valid slope, ground the character onto it
            if (angle > 0.05f)
            {
                onSlope = true;
                slopeNormalPerpendicular = Vector2.Perpendicular(hit.normal).normalized;
                
                // Snap player position slightly down to prevent airborne state transition
                rb.position = new Vector2(hit.point.x, hit.point.y);
            }
        }
    }

    private void ChangeState()
    {
        if (wallRegrabTimer <= 0f && IsWalled(out float mrow) && !IsGrounded() && Mathf.Abs(input.HorizontalInput) > 0.01f)
        {
            legsSpriteRenderer.enabled = false;
            weaponSpriteRenderer.enabled = false;
            wallRegrabTimer = wallRegrabCooldown;
            
            animator.SetBool("grounded", false);
            animator.SetBool("running", false);

            stateMachine.ChangeState(stateMachine.PlayerStates.WallClimbing());
            return;
        }
        else if ((input.JumpPressed && groundCheckTimer <= 0f && IsGrounded()) || (input.JumpPressed && groundCheckTimer <= 0f && coyoteTimer > 0f))
        {
            //jumping state
            legsSpriteRenderer.enabled = false;
            wallRegrabTimer = wallRegrabCooldown;
            animator.SetBool("OnStair", false);
            animator.SetBool("grounded", false);
            animator.SetBool("running", false);
            rb.linearVelocityY = 0f;
            stateMachine.ChangeState(stateMachine.PlayerStates.Jumping(new Vector2(0f, config.jumpForce)));
            return;
        }
        else if (!IsGrounded() && !onSlope) // Added !onSlope check to avoid false falls going downhill
        {
            //falling if not on ground
            legsSpriteRenderer.enabled = false;
            animator.SetBool("OnStair", false);
            animator.SetBool("grounded", false);
            animator.SetBool("running", false);

            stateMachine.ChangeState(stateMachine.PlayerStates.Falling());
        }
        else if (input.RollPressed && IsGrounded() && ConsumeStamina(rollCost))
        {   
            //roll state
            legsSpriteRenderer.enabled = true;
            weaponSpriteRenderer.enabled = false;
            animator.SetBool("OnStair", false);
            animator.SetBool("grounded", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.Rolling(moveSpeed));
        }
        else if (input.HeavyAttackPressed && ConsumeStamina(heavyAttackCost))
        {
            //heavy attack
            legsSpriteRenderer.enabled = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("running", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.HeavyAttack());
        }
        else if (input.LightAttackPressed && ConsumeStamina(lightAttackCost))
        {
            //light attack
            legsSpriteRenderer.enabled = true;
            
            //lunge slightly forward if standing still
            if (rb.linearVelocityX < .01f && !onSlope)
            {
                Vector2 facingDirectionX = new Vector2(player.transform.localScale.x, 0);

                if (Mathf.Abs(rb.linearVelocityX) < 0.01f)
                {
                    rb.AddForce(facingDirectionX * 3f, ForceMode2D.Impulse);
                }
            }

            animator.SetBool("running", false);
            stateMachine.ChangeState(stateMachine.PlayerStates.LightAttack());
        }
        else
        {
            //otherwise move to running
            animator.SetBool("running", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }

    private void SlopeCheck()
    {
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
        
        Debug.DrawRay(checkPos, player.transform.right * slopeCheckDistance, Color.red);
        Debug.DrawRay(checkPos, -player.transform.right * slopeCheckDistance, Color.cyan);
        
        if (slopeHitFront)
        {
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
            onSlope = slopeSideAngle > 0f;
        } 
        else if (slopeHitBack)
        {
            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
            onSlope = slopeSideAngle > 0f;
        }
        else
        {
            slopeSideAngle = 0f;
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
            onSlope = slopeDownAngle > 0.05f;   // authoritative: ground truth wins, no exception
            slopeDownAngleOld = slopeDownAngle;
        }
        else
        {
            onSlope = false;
        }
    }
}