using System;
using Unity.InferenceEngine;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public abstract class PlayerState
{
    protected StateMachine stateMachine;
    protected PlayerStateConfig config;
    protected InventoryManager inventoryManager;
    protected GameObject player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected PlayerInput input;
    protected BoxCollider2D playerCollider;
    protected SpriteRenderer torsoSpriteRenderer;
    protected SpriteRenderer legsSpriteRenderer;
    protected SpriteRenderer weaponSpriteRenderer;
    protected CameraFollow camera;

    //LayerMask references
    protected LayerMask climbableMask;
    protected LayerMask groundMask;
    protected LayerMask platformMask;
    protected LayerMask defaultMask;
    

    public PlayerState(StateMachine stateMachine, PlayerStateConfig config)
    {
        //TODO: pass these in?
        this.stateMachine = stateMachine;
        this.config = config;
        inventoryManager = stateMachine.InventoryManager;
        player = stateMachine.gameObject;
        rb = stateMachine.PlayerRb;
        animator = stateMachine.PlayerAnimator;
        input = stateMachine.Input;
        torsoSpriteRenderer = stateMachine.TorsoSpriteRenderer;
        legsSpriteRenderer = stateMachine.LegsSpriteRenderer;
        weaponSpriteRenderer = stateMachine.WeaponSpriteRenderer;
        playerCollider = stateMachine.PlayerCollider;
        camera = stateMachine.Cam;
        groundMask = stateMachine.GroundMask;
        climbableMask = stateMachine.ClimbableMask;
        platformMask = stateMachine.PlatformMask;
        defaultMask = stateMachine.DefaultMask;

    }

    public virtual void Enter() { }
    public virtual void Update()
    {
        if (input.ToggleInventory)
        {
            //turn off playercontrols in inventory screen
            input.PlayerControlsEnabled = !input.PlayerControlsEnabled;
            inventoryManager.ToggleInventory();
            
        }
    }



    public virtual void FixedUpdate() { }
    public virtual void Exit() { }



    protected bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        Vector2 boxSize = new Vector2(bounds.size.x * .9f, .5f);
        Vector2 boxCenter = bounds.center - new Vector3(0, bounds.extents.y, 0);
        float castDistance = .1f;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundMask | platformMask);
        filter.useTriggers = false;
        filter.useLayerMask = true;

        RaycastHit2D[] results = new RaycastHit2D[1];
        int hitCount = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.down,
            filter,
            results,
            castDistance
        );

        #if UNITY_EDITOR
            DrawBoxCastDebug(boxCenter, boxSize, Vector2.down, castDistance, hitCount > 0);
        #endif

            return hitCount > 0;
    }

    #if UNITY_EDITOR
        // Draws the swept boxcast volume used by IsGrounded(): the box at its start position,
        // the box at its end position (start + direction * distance), and the four lines
        // connecting matching corners between them, so you can see the whole swept region
        // it's testing against — not just a single center ray.
        // Green = grounded this frame, red = not grounded.
        private void DrawBoxCastDebug(Vector2 origin, Vector2 size, Vector2 direction, float distance, bool grounded)
        {
            Color color = grounded ? Color.green : Color.red;
            Vector2 halfSize = size * 0.5f;
            Vector2 offset = direction.normalized * distance;

            // Corners of the box at the start of the cast
            Vector2 startTL = origin + new Vector2(-halfSize.x, halfSize.y);
            Vector2 startTR = origin + new Vector2(halfSize.x, halfSize.y);
            Vector2 startBL = origin + new Vector2(-halfSize.x, -halfSize.y);
            Vector2 startBR = origin + new Vector2(halfSize.x, -halfSize.y);

            // Corners of the box at the end of the cast (swept by `offset`)
            Vector2 endTL = startTL + offset;
            Vector2 endTR = startTR + offset;
            Vector2 endBL = startBL + offset;
            Vector2 endBR = startBR + offset;

            // Start box outline
            Debug.DrawLine(startTL, startTR, color);
            Debug.DrawLine(startTR, startBR, color);
            Debug.DrawLine(startBR, startBL, color);
            Debug.DrawLine(startBL, startTL, color);

            // End box outline
            Debug.DrawLine(endTL, endTR, color);
            Debug.DrawLine(endTR, endBR, color);
            Debug.DrawLine(endBR, endBL, color);
            Debug.DrawLine(endBL, endTL, color);

            // Connect matching corners to show the swept path
            Debug.DrawLine(startTL, endTL, color);
            Debug.DrawLine(startTR, endTR, color);
            Debug.DrawLine(startBL, endBL, color);
            Debug.DrawLine(startBR, endBR, color);
        }
    #endif


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
    protected bool CanMantle()
    {
        Vector2 hipOrigin = (Vector2)player.transform.position + Vector2.up * 1f;
        Vector2 headOrigin = hipOrigin + Vector2.up * 1f;

        Vector2 castDir = player.transform.localScale.x == -1 ? Vector2.left : Vector2.right;
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



    protected bool ConsumeStamina(float cost)
    {
        return stateMachine.ConsumeStamina(cost);
    }
    protected void FlipX()
    {
        Vector3 localScale = player.transform.localScale;
        if(input.HorizontalInput < 0)
        {
            
            localScale.x = -1;
            player.transform.localScale = localScale;
        } else
        {
            localScale.x = 1;
            player.transform.localScale = localScale;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        //either revert to grounded or falling state after damaged depending on if on ground.
        stateMachine.ChangeState(stateMachine.PlayerStates.DamagedState(
            () => IsGrounded() ? stateMachine.PlayerStates.Grounded() : stateMachine.PlayerStates.Falling(),
            amount
        ));
    }

    public virtual void ToggleIndoors()
    {
        config.isInside = !config.isInside;
    }
}