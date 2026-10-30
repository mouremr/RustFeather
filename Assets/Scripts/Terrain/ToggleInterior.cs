using UnityEngine;

public class ToggleInterior : MonoBehaviour
{
    [SerializeField] private LayerMask player;

    private bool interiorActive = false;

    void Start()
    {
        SetInteriorActive(interiorActive);
    }

    public void ToggleColliders()
    {
        SetInteriorActive(!interiorActive);
    }

    public void SetInteriorActive(bool active)
    {
        interiorActive = active;

        // When inactive, exclude the player layer; when active, exclude nothing
        LayerMask exclude = active ? (LayerMask)0 : player;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
        {
            col.excludeLayers = exclude;
        }
    }
}