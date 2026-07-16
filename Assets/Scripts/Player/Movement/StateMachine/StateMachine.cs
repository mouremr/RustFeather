
using UnityEngine;

public class StateMachine : MonoBehaviour
{

    [SerializeField] private PlayerStateConfig stateConfig;
    private GameObject torso;
    private GameObject legs;
    //[SerializeField] private LayerMask climbable;
    // [SerializeField] private PhysicsMaterial2D noFriction;
    // [SerializeField] private PhysicsMaterial2D fullFriction;

    private PlayerState _currentState;
    private InteractionDetector interactionDetector;


    private float currentStamina;
    private float maxStamina;
    private float currentHealth;
    private float maxHealth;
    
    // Stamina regeneration timers
    //private float staminaRegenTimer = 0f;
    //private float staminaRegenDelay;
    private float staminaRegenRate;

    public PlayerState CurrentState => _currentState;
    public InteractionDetector InteractionDetector => interactionDetector;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    //for the future maybe move health away from movement states?
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public PlayerStateFactory States { get; private set; }


    public Rigidbody2D Rb { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerInput Input { get; private set; }
    public SpriteRenderer TorsoSpriteRenderer { get; private set; }
    public SpriteRenderer LegsSpriteRenderer { get; private set; }
    public SpriteRenderer WeaponSpriteRenderer {get; private set; }
    public BoxCollider2D PlayerCollider { get; private set; }
    public CameraFollow Cam { get; private set; }
    public LayerMask GroundMask { get; private set; }
    public LayerMask ClimbableMask { get; private set; }
    public LayerMask PlatformMask { get; private set; }
    public LayerMask DefaultMask { get; private set; }

    private void Awake()
    {
        States = new PlayerStateFactory(this, stateConfig);
        maxHealth = stateConfig.maxHealth;
        currentHealth = maxHealth;
        maxStamina = stateConfig.maxStamina;
        currentStamina = maxStamina;
        staminaRegenRate = stateConfig.staminaRegenRate;
        //staminaRegenDelay = stateConfig.staminaRegenDelay;




        torso = transform.Find("Torso").gameObject;
        legs = transform.Find("Legs").gameObject;
        Rb = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Input = GetComponent<PlayerInput>();
        TorsoSpriteRenderer = torso.GetComponent<SpriteRenderer>();
        LegsSpriteRenderer = legs.GetComponent<SpriteRenderer>();
        WeaponSpriteRenderer = transform.Find("Weapon").GetComponent<SpriteRenderer>();
        PlayerCollider = GetComponent<BoxCollider2D>();
        Cam = Camera.main.GetComponent<CameraFollow>();
        GroundMask = LayerMask.GetMask("Ground");
        ClimbableMask = LayerMask.GetMask("Climbable");
        PlatformMask = LayerMask.GetMask("Platform");
        DefaultMask = LayerMask.GetMask("Default");       
    }

    private void Start()
    {
        interactionDetector = GetComponent<InteractionDetector>();
        ChangeState(new GroundedState(this, stateConfig, true));
    }

    private void Update()
    {
        _currentState?.Update();
        RegenStamina();

        // Handle global interaction input
        if (UnityEngine.Input.GetKeyDown(KeyCode.E) && interactionDetector.HasInteractible)
        {
            interactionDetector.CurrentInteractible.Interact(gameObject);
        }
    }

    private void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    public void ChangeState(PlayerState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    private void RegenStamina()
    {
        if (currentStamina >= maxStamina)
            return;

        currentStamina += staminaRegenRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
    public bool ConsumeStamina(int cost)
    {
        if (currentStamina >= cost)
        {
            currentStamina -= cost;
            return true;
        }
        return false;
    }
    
    public void ModifyHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}