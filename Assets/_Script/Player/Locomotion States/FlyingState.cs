using UnityEngine;

/// <summary>
/// FlyingState - AC-style bird flight with joystick (left/right, up/down) and tap-to-flap boost
/// Refactored to use modular systems - acts as a coordinator for specialized components
/// </summary>
public class FlyingState : State
{
    // Core systems (from Character)
    private BirdPathFollower pathFollower;
    private BoostSystem boostSystem;
    private BoostInputDetector inputDetector;
    private FlightSpeedController speedController;
    
    // Modular flight components
    private FlightInputHandler inputHandler;
    private InputSmoother inputSmoother;
    private LateralMovementController lateralMovement;
    private VerticalMovementController verticalMovement;
    private FlightStateManager flightStateManager;

    public FlyingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Get cached systems from Character (Performance optimization)
        boostSystem = character.boostSystem;
        inputDetector = character.boostInputDetector;
        speedController = character.flightSpeedController;
        pathFollower = character.birdPathFollower;
        
        // Validate critical components
        ValidateComponents();
        
        // Initialize modular flight systems
        InitializeFlightSystems();
        
        // Subscribe to boost input
        if (inputDetector != null)
        {
            inputDetector.OnBoostInputDetected += OnBoostInput;
        }
    }
    
    /// <summary>
    /// Validate all required components are present
    /// </summary>
    private void ValidateComponents()
    {
        if (boostSystem == null)
            Debug.LogWarning("⚠️ BoostSystem not found on Character!");
        if (inputDetector == null)
            Debug.LogWarning("⚠️ BoostInputDetector not found on Character!");
        if (speedController == null)
            Debug.LogWarning("⚠️ FlightSpeedController not found on Character!");
        if (pathFollower == null)
            Debug.LogError("❌ BirdPathFollower NOT FOUND! Movement will not work!");
        else
            Debug.Log($"✓ BirdPathFollower found! Current speed: {pathFollower.followSpeed}");
        
        if (character.dynamicJoystick != null)
            Debug.Log("Joystick found - AC-style controls active");
        else
            Debug.Log("No joystick - using keyboard (WASD/Arrows + Space for boost)");
    }
    
    /// <summary>
    /// Initialize all modular flight systems
    /// </summary>
    private void InitializeFlightSystems()
    {
        // Input handling
        inputHandler = new FlightInputHandler(character.dynamicJoystick);
        
        // Input smoothing
        inputSmoother = new InputSmoother(character.inputResponsiveness);
        
        // Lateral movement (left/right)
        lateralMovement = new LateralMovementController(
            pathFollower,
            character.moveSpeed,
            character.maxLateralDistance,
            character.inputResponsiveness,
            character.airDrag
        );
        
        // Vertical movement (up/down)
        verticalMovement = new VerticalMovementController(
            pathFollower,
            character.verticalSpeed,
            character.maxVerticalOffset,
            character.inputResponsiveness,
            character.airDrag
        );
        
        // Flight state management (flying/gliding transitions)
        flightStateManager = new FlightStateManager(
            character.flyingDuration,
            character.glidingDuration
        );
    }

    /// <summary>
    /// Handle input detection from all sources (joystick, keyboard, gamepad)
    /// Called in Update()
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        if (inputHandler == null || inputSmoother == null) return;
        
        // Get raw input from all sources (handled by FlightInputHandler)
        Vector3 rawInput = inputHandler.GetRawInput();
        
        // Smooth input for AC-style feel (handled by InputSmoother)
        Vector3 smoothedInput = inputSmoother.SmoothInput(rawInput);
        
        // Store in character for other systems to access
        character.inputDirection = smoothedInput;
    }
    
    /// <summary>
    /// Callback when boost input is detected
    /// </summary>
    private void OnBoostInput()
    {
        if (boostSystem != null)
        {
            boostSystem.TryActivateBoost();
        }
    }

    /// <summary>
    /// Update timers and state logic
    /// Called in Update()
    /// </summary>
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (flightStateManager == null) return;
        
        // ===== AUTO FLYING/GLIDING TRANSITION =====
        // Pause transitions during boost
        bool shouldPauseTransitions = boostSystem != null && boostSystem.IsBoosting;
        flightStateManager.UpdateState(shouldPauseTransitions);
        
        // ===== UPDATE SPEED via FlightSpeedController =====
        if (speedController != null)
        {
            FlightSpeedController.FlightMode mode = flightStateManager.GetCurrentMode();
            speedController.UpdateSpeed(mode);
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();
        // Stay in flying state - add collision detection here if needed
    }

    /// <summary>
    /// Physics-based movement using modular controllers
    /// Called in FixedUpdate()
    /// </summary>
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Note: BirdPathFollower (Runner) controls forward movement along the path
        // Modular controllers handle lateral and vertical steering

        // ===== LATERAL MOVEMENT (LEFT/RIGHT) =====
        if (lateralMovement != null)
        {
            float horizontalInput = character.inputDirection.x;
            lateralMovement.UpdateMovement(horizontalInput);
        }
        
        // ===== VERTICAL MOVEMENT (UP/DOWN) =====
        if (verticalMovement != null)
        {
            float verticalInput = character.inputDirection.y;
            verticalMovement.UpdateMovement(verticalInput);
        }

        // ===== AIR DRAG (AC-STYLE RESISTANCE) =====
        ApplyAirDrag();

        // ===== BOOST PHYSICS =====
        if (boostSystem != null)
        {
            boostSystem.ApplyBoostPhysics();
        }
        
        // ===== GLIDING LIFT =====
        if (flightStateManager != null && flightStateManager.IsGliding && 
            (boostSystem == null || !boostSystem.IsBoosting))
        {
            // Subtle lift during gliding for realistic flight
            character.rb.AddForce(Vector3.up * 2f, ForceMode.Force);
        }
    }
    
    /// <summary>
    /// Apply gentle air drag for natural deceleration
    /// </summary>
    private void ApplyAirDrag()
    {
        Vector3 dragForce = -character.rb.linearVelocity * character.airDrag;
        dragForce.z *= 0.1f; // Less drag on forward movement
        character.rb.AddForce(dragForce, ForceMode.Force);
    }

    /// <summary>
    /// Update animations based on current state
    /// </summary>
    public override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (character.animationManager == null || flightStateManager == null) return;

        // Boost animation is handled by BoostSystem events
        // Handle gliding vs flying animations
        if (boostSystem != null && boostSystem.IsBoosting)
        {
            // Keep flapping animation during boost - don't override it!
        }
        else if (flightStateManager.IsGliding)
        {
            character.animationManager.PlayGliding();
        }
        else
        {
            character.animationManager.PlayFlying();
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Unsubscribe from boost input
        if (inputDetector != null)
        {
            inputDetector.OnBoostInputDetected -= OnBoostInput;
        }
    }
}
