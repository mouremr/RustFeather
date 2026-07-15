
using UnityEngine;

public class StateMachine : MonoBehaviour
{

    [SerializeField] private PlayerStateConfig stateConfig;
    private GameObject body;
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


    public Rigidbody2D rb { get; private set; }
    public Animator animator { get; private set; }
    public PlayerInput input { get; private set; }
    public SpriteRenderer bodySpriteRenderer { get; private set; }
    public SpriteRenderer legsSpriteRenderer { get; private set; }
    public BoxCollider2D playerCollider { get; private set; }
    public CameraFollow cam { get; private set; }
    public LayerMask groundMask { get; private set; }
    public LayerMask climbableMask { get; private set; }
    public LayerMask platformMask { get; private set; }

    private void Awake()
    {
        States = new PlayerStateFactory(this, stateConfig);
        maxHealth = stateConfig.maxHealth;
        currentHealth = maxHealth;
        maxStamina = stateConfig.maxStamina;
        currentStamina = maxStamina;
        staminaRegenRate = stateConfig.staminaRegenRate;
        //staminaRegenDelay = stateConfig.staminaRegenDelay;




        body = transform.Find("Torso").gameObject;
        legs = transform.Find("Legs").gameObject;
        rb = GetComponent<Rigidbody2D>();
        animator = body.GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        bodySpriteRenderer = body.GetComponent<SpriteRenderer>();
        legsSpriteRenderer = legs.GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        cam = Camera.main.GetComponent<CameraFollow>();
        groundMask = LayerMask.GetMask("Ground");
        climbableMask = LayerMask.GetMask("Climbable");
        platformMask = LayerMask.GetMask("Platform");       
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
        if (Input.GetKeyDown(KeyCode.E) && interactionDetector.HasInteractible)
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