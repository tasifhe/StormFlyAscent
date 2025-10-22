using UnityEngine;

/// <summary>
/// FlyingState - Handles the eagle's constant forward flight with gyroscopic steering and tap-to-dive mechanic
/// </summary>
public class FlyingState : State
{
    // Input references
    private DynamicJoystick joystick;           // For editor testing
    private bool useGyroscope = false;           // Toggle for gyroscope vs joystick
    private BirdPathFollower pathFollower;       // Reference to path follower
    
    // Dive mechanic variables
    private bool isDiving = false;               // Currently performing a dive
    private float diveTimer = 0f;                // Tracks dive duration
    private float diveCooldownTimer = 0f;        // Tracks cooldown between dives
    private const float DIVE_DURATION = 0.5f;    // How long the dive lasts
    
    // Movement tracking
    private float currentLateralPosition = 0f;   // Track left/right position from center

    public FlyingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        
        isDiving = false;
        diveTimer = 0f;
        diveCooldownTimer = 0f;
        currentLateralPosition = character.transform.position.x;

        // Get path follower reference
        pathFollower = character.GetComponent<BirdPathFollower>();

        // Find joystick for editor testing
        joystick = UnityEngine.Object.FindFirstObjectByType<DynamicJoystick>();
        
        // Check if gyroscope is available (mobile)
        useGyroscope = SystemInfo.supportsGyroscope;
        if (useGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("Gyroscope enabled for steering");
        }
        else
        {
            Debug.Log("Gyroscope not available - using joystick/keyboard for testing");
        }
    }

    /// <summary>
    /// Handle input detection - gyroscope tilt and tap-to-dive
    /// Called in Update()
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        // ===== GYROSCOPIC STEERING INPUT =====
        if (useGyroscope)
        {
            // Get gyroscope tilt (X-axis rotation for left/right tilt)
            float tilt = Input.gyro.rotationRateUnbiased.y; // Y rotation rate for left/right
            character.inputDirection = new Vector3(tilt, 0f, 0f);
        }
        else
        {
            // Editor/Testing fallback - use joystick or keyboard
            if (joystick != null)
            {
                character.inputDirection = new Vector3(joystick.Horizontal, 0f, 0f);
            }
            else
            {
                float horizontal = Input.GetAxis("Horizontal");
                character.inputDirection = new Vector3(horizontal, 0f, 0f);
            }
        }

        // ===== TAP-TO-DIVE INPUT =====
        DetectDiveInput();
    }

    /// <summary>
    /// Detect tap input for dive mechanic
    /// Supports both touch (mobile) and keyboard (testing)
    /// </summary>
    private void DetectDiveInput()
    {
        bool tapDetected = false;

        // Mobile touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapDetected = true;
        }
        
        // Editor testing - Space or Down arrow
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            tapDetected = true;
        }

        // Execute dive if tap detected and cooldown ready
        if (tapDetected && diveCooldownTimer <= 0f && !isDiving)
        {
            StartDive();
        }
    }

    /// <summary>
    /// Initialize dive mechanic
    /// </summary>
    private void StartDive()
    {
        isDiving = true;
        diveTimer = DIVE_DURATION;
        diveCooldownTimer = character.diveCooldown;
        
        Debug.Log("Dive started!");
    }

    /// <summary>
    /// Update timers and state logic
    /// Called in Update()
    /// </summary>
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Update dive timer
        if (isDiving)
        {
            diveTimer -= Time.deltaTime;
            if (diveTimer <= 0f)
            {
                isDiving = false;
                Debug.Log("Dive ended");
            }
        }

        // Update cooldown timer
        if (diveCooldownTimer > 0f)
        {
            diveCooldownTimer -= Time.deltaTime;
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();
        // Stay in flying state - add collision detection here if needed
    }

    /// <summary>
    /// Physics-based movement - constant forward flight, gyroscopic steering, and dive force
    /// Called in FixedUpdate()
    /// </summary>
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Note: BirdPathFollower (Runner) controls forward movement along the path
        // We only apply lateral steering forces and dive forces here

        // ===== GYROSCOPIC STEERING (LATERAL MOVEMENT) =====
        // Apply left/right forces based on tilt/input
        if (Mathf.Abs(character.inputDirection.x) > 0.05f)
        {
            // Apply lateral force (works with Runner's position control)
            float lateralForce = character.inputDirection.x * character.moveSpeed * 10f;
            character.rb.AddForce(Vector3.right * lateralForce, ForceMode.Force);
            
            // Update lateral position tracking for path follower
            currentLateralPosition += character.inputDirection.x * character.moveSpeed * Time.fixedDeltaTime;
            currentLateralPosition = Mathf.Clamp(currentLateralPosition, -character.maxLateralDistance, character.maxLateralDistance);
            
            // Update path follower lateral offset if available
            if (pathFollower != null)
            {
                pathFollower.SetLateralOffset(currentLateralPosition);
            }
        }
        else
        {
            // No input - dampen lateral velocity
            Vector3 velocity = character.rb.linearVelocity;
            Vector3 dampingForce = Vector3.right * -velocity.x * 5f;
            character.rb.AddForce(dampingForce, ForceMode.Force);
        }

        // ===== TAP-TO-DIVE MECHANIC =====
        if (isDiving)
        {
            // Apply downward force for dive
            character.rb.AddForce(Vector3.down * character.diveForce, ForceMode.Force);
        }

        // Note: Rotation is handled by BirdPathFollower which aligns bird with path direction
    }

    /// <summary>
    /// Update animations based on current state
    /// </summary>
    public override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (character.animationManager != null)
        {
            if (isDiving)
            {
                character.animationManager.PlayDiving();
            }
            else
            {
                character.animationManager.PlayFlying();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        isDiving = false;
        
        // Disable gyroscope when exiting
        if (useGyroscope)
        {
            Input.gyro.enabled = false;
        }
    }
}
