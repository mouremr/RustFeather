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
    private int platformLayer;
    private int defaultLayer;
    private readonly List<Bounds> stairBounds = new();
    private int currentStairIndex = -1;

    private void Start()
    {
        effector.surfaceArc = 180f;
        platformLayer = LayerMask.NameToLayer("Platform");
        defaultLayer = LayerMask.NameToLayer("Default");
        CacheStairBounds();
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
            effector.surfaceArc = -180f;
            fellThrough = true;
            isOnPlatform = false;
            stairs.layer = defaultLayer;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider != playerCollider)
            return;
        //ColliderDistance2D distance = Physics2D.Distance(stairsCollider, playerCollider);
        if (fellThrough && playerCollider.bounds.min.y >= stairBounds[currentStairIndex].min.y)
        {
            fellThrough = false;
            effector.surfaceArc = 180f;
            stairs.layer = platformLayer;
        }
    }
    int FindNearestStair()
    {
        float closestDistance = Mathf.Infinity;
        int closestIndex = -1;

        Vector2 playerPos = playerTransform.position;

        for (int i = 0; i < stairBounds.Count; i++)
        {
            Bounds b = stairBounds[i];

            // Ignore staircases that are nowhere near the player.
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
