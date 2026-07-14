using System.Collections.Generic;
using UnityEngine;

public class TwoWayStair : MonoBehaviour
{
    //[SerializeField] private PlatformEffector2D platformEffector;
    [SerializeField] private CompositeCollider2D stairsCollider;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private PlatformEffector2D effector;
    [SerializeField] private GameObject stairs;
    [SerializeField] private Transform playerTransform;
    private bool isOnPlatform = false;
    private bool fellThrough = false;
    [SerializeField] private TwoWayPlatform platform;
    private readonly List<Bounds> stairBounds = new();
    private int currentStairIndex = -1;
    public bool DisabledByPlatform { get; set; }
    [SerializeField] private LayerMask environmentMask;
    private int platformLayer;
    private int defaultLayer;

    private void Start()
    {
        effector.surfaceArc = 180f;
        CacheStairBounds();
        platformLayer = LayerMask.NameToLayer("Platform");
        defaultLayer = LayerMask.NameToLayer("Default");
    }
    
    void CacheStairBounds()
    {
        stairBounds.Clear();
        for (int i = 0; i < stairsCollider.pathCount; i++)
        {
            Vector2[] points = new Vector2[stairsCollider.GetPathPointCount(i)];
            stairsCollider.GetPath(i, points);
            Bounds bounds = new Bounds(points[0], Vector3.zero);
            foreach (Vector2 point in points)
                bounds.Encapsulate(point);
            stairBounds.Add(bounds);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider == playerCollider && !isOnPlatform)
        {
            isOnPlatform = true;
            currentStairIndex = FindNearestStair();
        }
    }

    void Update()
    {
        if (isOnPlatform && Input.GetKeyDown(KeyCode.S))
        {
            DisableStairs();
        }

        //if player is underneath stairs reset stair collider
        if (fellThrough && playerCollider.bounds.max.y <= stairBounds[currentStairIndex].min.y)
        {
            ResetStairs();
        }

        // Check if player is overlapping with anything, reset if not
        if (fellThrough)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(environmentMask);
            filter.useTriggers = false;

            Collider2D[] results = new Collider2D[1];
            int overlapCount = playerCollider.Overlap(filter, results);

            if (overlapCount == 0)
            {
                //Physics2D.IgnoreCollision(stairsCollider, playerCollider, true);
                ResetStairs();
            }
            else
            {
                //Physics2D.IgnoreCollision(stairsCollider, playerCollider, false);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider != playerCollider) return;
        
        //after player stops colliding and player is above the stairs, reset the stairs.    
        if (fellThrough && playerCollider.bounds.min.y >= stairBounds[currentStairIndex].min.y)
        {
            ResetStairs();
        }
    }

    public void ResetStairs()
    {
        fellThrough = false;
        effector.surfaceArc = 180f;
        platform.ResetPlatform();
        platform.DisabledByStairs = false;
        stairs.layer = platformLayer;
    }

    public void DisableStairs()
    {
        effector.surfaceArc = 0f;
        fellThrough = true;
        isOnPlatform = false;
        platform.DisabledByStairs = true;
        
        //remove stair layer to stop triggering run animation while falling through stairs.
        stairs.layer = defaultLayer; 
        
        //disable platforms as well to stop catching on platforms connected below stairs
        platform.DisablePlatform(); 
    }

    int FindNearestStair()
    {
        float closestDistance = Mathf.Infinity;
        int closestIndex = -1;
        Vector2 playerPos = playerTransform.position;

        for (int i = 0; i < stairBounds.Count; i++)
        {
            Bounds b = stairBounds[i];
            if (playerPos.x < b.min.x - 0.5f || playerPos.x > b.max.x + 0.5f)
                continue;

            float distance = Mathf.Abs(playerPos.y - b.max.y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
}