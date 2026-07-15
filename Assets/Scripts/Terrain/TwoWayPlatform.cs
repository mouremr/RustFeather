using UnityEngine;

public class TwoWayPlatform : MonoBehaviour
{
    [SerializeField] private Collider2D platformCollider;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private GameObject platform;

    private bool isOnPlatform = false;
    private bool isFallingThrough = false;
    public bool DisabledByStairs { get; set; } 
    [SerializeField] private LayerMask playerMask;
    private Vector2 boxCenter;
    private Vector2 boxSize = Vector2.zero;

    [SerializeField] private LayerMask environmentMask;
    [SerializeField] private TwoWayStair stairs;
    private int platformLayer;
    private int defaultLayer;

    private void Start()
    {
        boxCenter = platformCollider.bounds.center;
        boxSize = platformCollider.bounds.size;
        platformLayer = LayerMask.NameToLayer("Platform");
        defaultLayer = LayerMask.NameToLayer("Default");
    }

    void Update()
    {
        if (DisabledByStairs) return;
        if (isOnPlatform && Input.GetKeyDown(KeyCode.S) && !isFallingThrough)
        {
            DisablePlatform();
            Debug.Log("should drop");
        }

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.up,
            .5f,
            playerMask
        );

        if (hit.collider == null && isFallingThrough)
        {
            ResetPlatform();
        }
        
        //check if overlapping with any ground and reset platform if not
        if (isFallingThrough)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(environmentMask);
            filter.useTriggers = false;

            Collider2D[] results = new Collider2D[1];
            int overlapCount = playerCollider.Overlap(filter, results);

            // if not overlapping with anything, reset
            if (overlapCount == 0)
            {
                ResetPlatform();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider == playerCollider)
        {
            isOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == playerCollider)
        {
            isOnPlatform = false;
        }
    }
    public void DisablePlatform()
    {
        isFallingThrough = true;
        Physics2D.IgnoreCollision(platformCollider, playerCollider, true);
        platform.tag = "Untagged";
        //platform.layer = defaultLayer;
        stairs.DisabledByPlatform = true;
        //stairs.DisableStairs();
    }
    public void ResetPlatform()
    {
        isFallingThrough = false;
        Physics2D.IgnoreCollision(platformCollider, playerCollider, false);
        platform.tag = "Mantleable";
        //platform.layer = platformLayer;
        stairs.DisabledByPlatform = false;
        //stairs.ResetStairs();
    }
}