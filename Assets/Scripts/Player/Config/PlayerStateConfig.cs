// playerConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "playerStateConfig", menuName = "Player/Player State Config")]
public class PlayerStateConfig : ScriptableObject
{
    [Header("State")]
    public bool isInside = false;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    [Tooltip("grace period you can jump while being not grounded")]
    public float gracePeriod = .2f;
    public PhysicsMaterial2D noFriction;
    public PhysicsMaterial2D fullFriction;
    public float gravityScale = 1f;
    
    
    [Header("Climbing")]
    public float climbSpeed = 4f;
    public float climbCost = .5f;
    public GameObject pitonPrefab;
    
    [Header("Rolling")]
    public float rollSpeed = 12f;
    public float rollDuration = 0.5f;
    public int rollCost = 10;
    
    [Header("Health & Stamina")]
    public float maxHealth = 10f;
    public float maxStamina = 20f;
    public float staminaRegenRate = 5f;
    public float staminaRegenDelay = 0.5f;

    [Header("Attacking")]
    public int lightAttackCost = 5;
    public int lightAttackDamage = 3;

    public int heavyAttackCost = 10;
    public int heavyAttackDamage = 7;

}
