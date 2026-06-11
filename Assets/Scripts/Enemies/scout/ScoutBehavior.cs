using System.Collections;
using UnityEngine;

public class ScoutBehavior : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRayLength = 0.5f; // player must be within 1 unit to begin attack

    [SerializeField] private float idledetectRayLength=6; // 6 units of sight
    [SerializeField] private float idlebehindDetectionDistance = 1;
    [SerializeField] private float aggresivedetectRayLength = 13f;
    [SerializeField] private float aggresivebehindDetectionDistance = 8f;
    [SerializeField] private Transform detectRayOrigin;
    
    [SerializeField] private Transform attackRayOrigin;

    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private Transform ScoutTransform;
    [SerializeField] private GameObject player;

    private float behindDetectionDistance;
    private float detectRayLength;

    private SpriteRenderer sprite;

    private LayerMask playerMask;

    private LayerMask climbableMask;

    private bool playerInSight;
    private bool isAttacking;

    private StateMachine playerStateMachine;

    void Awake()
    {
        detectRayLength = idledetectRayLength;
        behindDetectionDistance = idlebehindDetectionDistance;
        playerStateMachine = player.GetComponent<StateMachine>(); 
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMask = LayerMask.GetMask("Player"); 
        climbableMask = LayerMask.GetMask("Climbable");
        sprite = GetComponent<SpriteRenderer>(); 
    }

    void Update()
    {
        playerInSight = PlayerIsInSight();
        if (isAttacking){ //if attacking, finish attack 
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("chasePlayer", false);
            //Debug.Log("I must finish attacking");
            return;
        }

        

        if(playerInSight){
            //Debug.Log("I can see player!1");
            float facing = sprite.flipX ? -1f : 1f;
            behindDetectionDistance = aggresivebehindDetectionDistance;
            detectRayLength = aggresivedetectRayLength;

            Vector2 direction = sprite.flipX ? Vector2.left : Vector2.right;

            RaycastHit2D hit = Physics2D.Raycast(attackRayOrigin.position, direction, attackRayLength, playerMask);
            Debug.DrawRay(attackRayOrigin.position, direction * attackRayLength, Color.green);
            
            if (hit.collider==null) //if hit doesnt connect, chase player
            {
                anim.SetBool("chasePlayer", true); //chase
                anim.SetBool("attackPlayer", false);
                isAttacking = false; 

                rb.linearVelocity = new Vector2(facing * moveSpeed, rb.linearVelocity.y);
                //sprite.flipX = facing < 0;
                if(PlayerTransform.position.x < ScoutTransform.position.x)
                {
                    sprite.flipX = true;
                } else
                {
                    sprite.flipX = false;
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool("chasePlayer", false);
                anim.SetBool("attackPlayer", true);
                isAttacking = true;
                playerStateMachine.ModifyHealth(-1f);
                //Debug.Log(PlayerState.Currenthealth);
            }
        }

        if (!playerInSight)
        {
            Idlestate();
        }

    }

    bool PlayerIsInSight()
    {
        Vector2 facing = sprite.flipX ? Vector2.left : Vector2.right;
        
        //float facing = sprite.flipX ? -1f : 1f;
        Vector2 rayOrigin = (Vector2)detectRayOrigin.position;
        float behindOffsetDirection = sprite.flipX ? 1f : -1f;
        rayOrigin += new Vector2(behindDetectionDistance * behindOffsetDirection, 0);

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            facing,
            detectRayLength,
            playerMask | climbableMask
        );
        Debug.DrawRay(
           rayOrigin,              
            (Vector3)(facing * detectRayLength), 
            Color.blue
        );
        return hit.collider != null && ((1 << hit.collider.gameObject.layer) & playerMask) != 0; //return true if something is hit, and the first object hit is the player
    }

    // Call this from an animation event at the end of the punch
    public void ToggleCanAttack()
    {
        //Debug.Log("animation ended, stop attacking");
        isAttacking = false;
        anim.SetBool("attackPlayer", false);
    }

    private void Idlestate()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("chasePlayer", false);
        anim.SetBool("attackPlayer", false);
        //Debug.Log("I cannot see player"); 
        //return;
        if(detectRayLength == aggresivedetectRayLength)
        {
            StartCoroutine(ResetDetectDistance());
        }
    }

    IEnumerator ResetDetectDistance()
    {
        yield return new WaitForSeconds(2.0f);
        detectRayLength = idledetectRayLength;
        behindDetectionDistance = idlebehindDetectionDistance;
    }
}
