using UnityEngine;

public class Character : MonoBehaviour
{
    // State Machine
    public StateMachine movementSM;
    public BirdFlyingState flyingState;

    // Core components
    [HideInInspector]
    public Vector3 inputDirection;

    // Component references (cached for performance)
    [HideInInspector]
    public CharacterPathFollower pathFollower;
    [HideInInspector]
    public CharacterInput characterInput;

    private void Start()
    {
        // Check if GameStartManager exists - if so, it will handle initialization
        GameStartManager gameStartManager = FindFirstObjectByType<GameStartManager>();
        if (gameStartManager != null)
        {
            Debug.Log("✓ GameStartManager found - waiting for initialization...");
            // GameStartManager will call InitializeCharacter when ready
            return;
        }

        // No GameStartManager - initialize normally (for testing/other scenes)
        InitializeCharacter();
    }

    public void InitializeCharacter()
    {
        Debug.Log("[Character] Initializing character...");

        // Get core components
        pathFollower = GetComponent<CharacterPathFollower>();
        characterInput = GetComponent<CharacterInput>();

        // Validate components
        if (pathFollower == null)
            Debug.LogError("❌ CharacterPathFollower not found on Character!");
        if (characterInput == null)
            Debug.LogWarning("⚠️ CharacterInput not found on Character!");

        // Initialize state machine
        movementSM = new StateMachine();

        // Create and initialize flying state
        flyingState = new BirdFlyingState(this, movementSM);
        movementSM.Initialize(flyingState);

        // Start path following
        if (pathFollower != null)
        {
            pathFollower.StartFollowing();
        }

        Debug.Log("✅ Character initialized with BirdFlyingState!");
    }

    private void Update()
    {
        if (movementSM?.currentState != null)
        {
            movementSM.currentState.HandleInput();
            movementSM.currentState.LogicUpdate();
            movementSM.currentState.UpdateAnimation();
            movementSM.currentState.ChangeState();
        }
    }

    private void FixedUpdate()
    {
        if (movementSM?.currentState != null)
        {
            movementSM.currentState.PhysicsUpdate();
        }
    }
}
