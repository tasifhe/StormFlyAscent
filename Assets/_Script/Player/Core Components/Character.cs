using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;
using Animancer;

public class Character : MonoBehaviour
{
    [Header("Ground Movement Controls")]
    public float playerSpeed = 5.0f;
    [Space(10)]
    public float rotationSpeed = 5f;
    [Space(10)]

    [Header("Flying Controls")]
    public float forwardSpeed = 10f;          // Constant forward movement speed
    public float moveSpeed = 5f;              // Lateral (left/right) movement sensitivity
    public float verticalSpeed = 3f;          // Vertical (up/down) movement sensitivity
    public float flapBoostForce = 20f;        // Speed boost when tapping to flap
    public float flapBoostDuration = 0.3f;    // How long flap boost lasts
    public float flapCooldown = 0.8f;         // Cooldown between flaps
    public float maxLateralDistance = 5f;     // Max distance bird can move left/right from center
    public float maxVerticalOffset = 5f;      // Max distance bird can move up/down from path
    
    [Header("Flight Smoothness (AC-Style)")]
    [Tooltip("How quickly input responds (lower = smoother, more momentum)")]
    [Range(1f, 20f)]
    public float inputResponsiveness = 8f;
    [Tooltip("Speed boost multiplier when flapping")]
    [Range(1f, 3f)]
    public float flapSpeedMultiplier = 1.8f;
    [Tooltip("Speed reduction multiplier when gliding")]
    [Range(0.5f, 1f)]
    public float glideSpeedMultiplier = 0.85f;
    [Tooltip("How much altitude change affects speed")]
    [Range(0f, 5f)]
    public float altitudeSpeedInfluence = 1.5f;
    [Tooltip("Drag coefficient for smooth deceleration")]
    [Range(0f, 5f)]
    public float airDrag = 1.2f;
    [Tooltip("Maximum turn rate (degrees per second)")]
    [Range(10f, 180f)]
    public float maxTurnRate = 90f;
    
    [Header("Flight Animation Timing")]
    [Tooltip("How long the bird actively flaps its wings before gliding")]
    public float flyingDuration = 3f;
    [Tooltip("How long the bird glides passively before flapping again")]
    public float glidingDuration = 2f;

    [Header("Animation Smoothing")]
    [Range(0, 1)]
    public float speedDampTime = 0.1f;    
    [Range(0, 1)]
    public float airControl = 0.5f;

    //States
    public StateMachine movementSM;
    public FlyingState flyingState;
    

    [HideInInspector]
    public float gravityValue = -9.81f;
    [HideInInspector] 
    public Vector3 playerVelocity;
    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public Vector3 inputDirection;

    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public BirdAnimationManager animationManager;


    private void Start()
    {
        // CRITICAL: Ensure EventSystem exists for joystick touch input
        EnsureEventSystem();
        
        rb = GetComponent<Rigidbody>();
        //animationManager = GetComponent<BirdAnimationManager>();
        

        movementSM = new StateMachine();
        
        // Initialize flying state
        flyingState = new FlyingState(this, movementSM);
        
        // Start with flying state
        movementSM.Initialize(flyingState);   
    }

    private void Update()
    {
        movementSM.currentState.HandleInput();
        movementSM.currentState.LogicUpdate();
        movementSM.currentState.UpdateAnimation();
        movementSM.currentState.ChangeState();
    }

    private void FixedUpdate()
    {
        movementSM.currentState.PhysicsUpdate();
    }
   
    /// <summary>
    /// Ensures an EventSystem exists for joystick touch input (CRITICAL!)
    /// FIXED: Now uses InputSystemUIInputModule for New Input System
    /// </summary>
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        
        if (eventSystem == null)
        {
            Debug.LogWarning("[Character] No EventSystem found! Creating one for joystick touch input...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>(); // NEW INPUT SYSTEM!
            Debug.Log("[Character] EventSystem created with InputSystemUIInputModule!");
        }
        else
        {
            Debug.Log("[Character] EventSystem found: " + eventSystem.gameObject.name);
            
            // Check if it has the correct input module
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null && 
                eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                Debug.LogWarning("[Character] EventSystem has OLD StandaloneInputModule! Replacing with InputSystemUIInputModule...");
                Destroy(eventSystem.GetComponent<StandaloneInputModule>());
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[Character] InputSystemUIInputModule added!");
            }
        }
    }
}
