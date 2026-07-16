using UnityEngine;

public class MantlingState : PlayerState
{
    private RaycastHit2D hipHit;
    private Vector2 headOrigin;
    private float facingDirection;
    private Vector2 targetMantlePosition;

    private Vector2 intermediatePosition;
    private float topLedgeY;
    private float topLedgeX;

    private bool isMantleComplete = false;
    private float mantleTimer = 0f;
    private const float MANTLE_DURATION = 0.25f;

    private Vector2 oldSize;
    private Vector2 oldOffset;

    public MantlingState(StateMachine stateMachine, PlayerStateConfig config) : base(stateMachine, config){ }

    public override void Enter(){
        Vector2 hipOrigin = (Vector2)player.transform.position + Vector2.up * 1f;

        Vector2 castDir = torsoSpriteRenderer.flipX ? Vector2.left : Vector2.right;
        float rayLength = 0.5f;
    
        headOrigin = hipOrigin + Vector2.up * 1f;
        hipHit = Physics2D.Raycast(hipOrigin, castDir, rayLength,platformMask);
        
        // Save original collider size/offset
        oldSize = playerCollider.size;
        oldOffset = playerCollider.offset;
        animator.SetBool("mantling", true);

        if (hipHit.collider != null)
        {
            topLedgeX = hipHit.point.x;

            //raycast down from above ledge to find top
            Vector2 downCastOrigin = hipHit.point + Vector2.up * 1.0f;
            RaycastHit2D downHit = Physics2D.Raycast(
                downCastOrigin,
                Vector2.down,
                2f,
                platformMask
            );

            if (downHit)
            {
                topLedgeY = downHit.point.y;
            }
        }

        targetMantlePosition = new Vector2(player.transform.position.x+0.4f, topLedgeY);

        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        mantleTimer = 0f;
        isMantleComplete = false;
        camera.smoothTime = 0.3f;
    }

    public override void Update()
    {
        if (!isMantleComplete)
        {
            player.transform.position = new Vector2(topLedgeX, topLedgeY);
        }        
        Debug.DrawRay(targetMantlePosition, Vector2.up * 0.2f, Color.green);

        mantleTimer += Time.deltaTime;

        playerCollider.size = torsoSpriteRenderer.bounds.size;
        playerCollider.offset = torsoSpriteRenderer.bounds.center - player.transform.position;
       
        if (mantleTimer >= MANTLE_DURATION)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            isMantleComplete = true;
            //animator.SetBool("mantling", false);

        }
        
        if (isMantleComplete)
        {
            // rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            camera.smoothTime = 0.2f;
            animator.SetBool("mantling", false);

            playerCollider.size = oldSize;
            playerCollider.offset = oldOffset;
            rb.linearVelocity = Vector2.zero;
            stateMachine.ChangeState(stateMachine.States.Grounded(true));
            return;
        }
    }

}