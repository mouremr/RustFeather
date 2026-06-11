using System;
using Unity.InferenceEngine;
using UnityEngine;

public abstract class PlayerState
{
    protected StateMachine stateMachine;
    protected PlayerStateConfig config;
    protected GameObject player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected PlayerInput input;
    protected BoxCollider2D playerCollider;
    protected SpriteRenderer bodySpriteRenderer;
    protected SpriteRenderer legsSpriteRenderer;
    protected CameraFollow camera;

    //LayerMask references
    protected LayerMask climbableMask;
    protected LayerMask groundMask;
    protected LayerMask platformMask;
    

    public PlayerState(StateMachine stateMachine, PlayerStateConfig config)
    {
        //TODO: pass these in?
        this.stateMachine = stateMachine;
        this.config = config;
        player = stateMachine.gameObject;
        player = stateMachine.gameObject;
        rb = stateMachine.rb;
        animator = stateMachine.animator;
        input = stateMachine.input;
        bodySpriteRenderer = stateMachine.bodySpriteRenderer;
        legsSpriteRenderer = stateMachine.legsSpriteRenderer;
        playerCollider = stateMachine.playerCollider;
        camera = stateMachine.cam;
        groundMask = stateMachine.groundMask;
        climbableMask = stateMachine.climbableMask;
        platformMask = stateMachine.platformMask;

    }

    public virtual void Enter() { }
    public virtual void Update(){ }



    public virtual void FixedUpdate() { }
    public virtual void Exit() { }



    protected bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.1f);
        Vector2 boxCenter = bounds.center - new Vector3(0, bounds.extents.y, 0);
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.down,
            .1f,
            groundMask | platformMask
        );

        //Debug.DrawRay(boxCenter, Vector2.down * .1f, Color.green);

        
        return hit.collider != null;
    }


    protected bool IsWalled(out float direction)
    {
        Vector2 hipOrigin = (Vector2)player.transform.position + Vector2.up * 1f;
        float rayLength = 0.4f;

        RaycastHit2D left = Physics2D.Raycast(hipOrigin, Vector2.left, rayLength,climbableMask);
        RaycastHit2D right = Physics2D.Raycast(hipOrigin, Vector2.right, rayLength,climbableMask);

        Debug.DrawRay(hipOrigin, Vector2.left * rayLength, Color.red);
        Debug.DrawRay(hipOrigin, Vector2.right * rayLength, Color.blue);

        if (left.collider != null)
        {
            direction = -1;
            return true;
        }

        if (right.collider != null)
        {
            direction = 1;
            return true;
        }

        direction = 0;
        return false;
    }
    protected bool canMantle()
    {
        Vector2 hipOrigin = (Vector2)player.transform.position + Vector2.up * 1f;
        Vector2 headOrigin = hipOrigin + Vector2.up * 1f;

        Vector2 castDir = bodySpriteRenderer.flipX ? Vector2.left : Vector2.right;
        float rayLength = 0.5f;
        RaycastHit2D hipHit = Physics2D.Raycast(hipOrigin, castDir, rayLength,platformMask);
        RaycastHit2D headHit = Physics2D.Raycast(headOrigin, castDir, rayLength,platformMask);

        // Debug.DrawRay(hipOrigin, castDir * rayLength, Color.red);
        // Debug.DrawRay(headOrigin, castDir * rayLength, Color.blue);


        if (hipHit.collider != null && hipHit.collider.CompareTag("Mantleable") && headHit.collider == null)    
        {
            return true;

        }
        else
        {
            return false;
        }
        
    }



    public bool ConsumeStamina(int cost)
    {
        return stateMachine.ConsumeStamina(cost);
    }

}