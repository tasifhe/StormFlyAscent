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
    public float flapStrength = 8f;
    public float glideSpeed = 12f;
    public float diveSpeed = 20f;
    public float turnSpeed = 2f;
    public float liftForce = 2f;
    [Range(0, 1)]
    public float airResistance = 0.98f;

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
    //public BirdAnimationManager animationManager;


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
