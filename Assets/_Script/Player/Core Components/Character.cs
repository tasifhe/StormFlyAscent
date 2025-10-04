using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Character : MonoBehaviour
{
   
    [Header("Ground Movement Controls")]
    public float playerSpeed = 5.0f;      
    [Space(10)]
    public float rotationSpeed = 5f;    
    [Space(10)]
    public float jumpHeight = 0.8f;   

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
    public StandingState standingState;    
    public JumpingState jumpingState;
    public FlyingState flyingState;
    public GlidingState glidingState;
    public DivingState divingState;
    

    [HideInInspector]
    public float gravityValue = -9.81f;
    [HideInInspector] 
    public Vector3 playerVelocity;
    [HideInInspector]
    public bool isGrounded;
    [HideInInspector]
    public Vector3 inputDirection;

    [HideInInspector]
    public CharacterController controller;
    [HideInInspector]
    public Animator animator;


    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        

        movementSM = new StateMachine();

        standingState = new StandingState(this, movementSM);        
        jumpingState = new JumpingState(this, movementSM);
        flyingState = new FlyingState(this, movementSM);
        glidingState = new GlidingState(this, movementSM);
        divingState = new DivingState(this, movementSM);
        

        movementSM.Initialize(standingState);        
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
