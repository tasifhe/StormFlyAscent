using UnityEngine;
using UnityEngine.InputSystem;
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
    public float diveForce = 15f;             // Strength of the dive
    public float diveCooldown = 1.5f;         // Cooldown between dives
    public float maxLateralDistance = 5f;     // Max distance bird can move left/right from center

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
   
}
