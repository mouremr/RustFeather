using UnityEngine;

public class HangingVine : MonoBehaviour
{
    [SerializeField] Rigidbody2D anchor;
    [Header("Shape")]
    [SerializeField] GameObject[] segmentPrefabs;
    [SerializeField] int length = 8;
    [SerializeField] float segmentLength = 0.5f;
    [SerializeField] float overlap = 0.9f;
    [SerializeField] GameObject tipPrefab;

    [Header("Look")]
    [SerializeField] int seed = 0;
    [SerializeField] bool randomFlip = true;

    void Start() => Build();

    [ContextMenu("Rebuild")]
    public void Build()
    {
        Rigidbody2D previous = anchor;
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        var rng = new System.Random(seed);
        float step = segmentLength * overlap;

        for (int i = 0; i < length; i++)
        {
            bool isTip = i == length - 1 && tipPrefab != null;
            var prefab = isTip ? tipPrefab : segmentPrefabs[rng.Next(segmentPrefabs.Length)];

            var seg = Instantiate(prefab, transform);
            seg.SetActive(true);
            seg.transform.localPosition = Vector3.down * step * i;
            seg.transform.localRotation = Quaternion.identity;

            var sr = seg.GetComponentInChildren<SpriteRenderer>(true);
            bool flip = rng.NextDouble() > 0.5;   // always consume so the pattern stays stable
            if (sr != null && randomFlip && !isTip) sr.flipX = flip;

            var hinge = seg.GetComponent<HingeJoint2D>();
            hinge.connectedBody = previous;
            hinge.autoConfigureConnectedAnchor = false;
            hinge.anchor = new Vector2(0, segmentLength * 0.5f);            // top of this segment
            hinge.connectedAnchor = previous == anchor
                ? Vector2.zero
                : new Vector2(0, -segmentLength * 0.5f);                    // bottom of previous

            previous = seg.GetComponent<Rigidbody2D>();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, 0.06f);
        Vector3 end = transform.position + Vector3.down * segmentLength * overlap * (length - 1);
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawWireSphere(end, 0.05f);
    }
}