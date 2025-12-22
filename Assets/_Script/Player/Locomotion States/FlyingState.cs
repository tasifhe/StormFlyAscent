using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// FlyingState - AC-style bird flight with joystick (left/right, up/down) and tap-to-flap boost
/// Refactored to use modular systems
/// </summary>
public class FlyingState : State
{
    // Input references
    private DynamicJoystick joystick;
    private BirdPathFollower pathFollower;
    
    // Modular systems
    private BoostSystem boostSystem;
    private BoostInputDetector inputDetector;
    private FlightSpeedController speedController;
    
    // Input smoothing for AC-style responsiveness
    private Vector3 smoothedInput = Vector3.zero;
    private Vector3 inputVelocity = Vector3.zero;
    
    // Auto gliding mechanic variables
    private bool isGliding = false;
    private float flightTimer = 0f;
    
    // Movement tracking
    private float currentLateralPosition = 0f;
    private float currentVerticalOffset = 0f;
    private float lateralVelocity = 0f;
    private float verticalVelocity = 0f;

    public FlyingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Use cached modular systems from Character (Performance optimization)
        boostSystem = character.boostSystem;
        inputDetector = character.boostInputDetector;
        speedController = character.flightSpeedController;
        pathFollower = character.birdPathFollower;
        joystick = character.dynamicJoystick;
        
        // Validate critical components
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
        
        if (joystick != null)
            Debug.Log("Joystick found - AC-style controls active");
        else
            Debug.Log("No joystick - using keyboard (WASD/Arrows + Space for boost)");
        
        // Subscribe to boost input
        if (inputDetector != null)
        {
            inputDetector.OnBoostInputDetected += OnBoostInput;
        }
        
        // Start with active flying (not gliding)
        isGliding = false;
        flightTimer = 0f;
        
        // Reset smooth values
        smoothedInput = Vector3.zero;
        inputVelocity = Vector3.zero;
        lateralVelocity = 0f;
        verticalVelocity = 0f;
        
        currentLateralPosition = 0f;
        currentVerticalOffset = 0f;
    }

    /// <summary>
    /// Handle input detection - ALL INPUTS WORK SIMULTANEOUSLY
    /// Joystick (touch/mouse) + Keyboard + Gamepad all work together
    /// Called in Update()
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        Vector3 rawInput = Vector3.zero;

        // ===== JOYSTICK INPUT (Works with BOTH touch AND mouse!) =====
        if (joystick != null)
        {
            Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            if (joystickInput.sqrMagnitude > 0.01f)
            {
                rawInput = new Vector3(joystickInput.x, joystickInput.y, 0f);
            }
        }
        
        // ===== KEYBOARD INPUT =====
        if (Keyboard.current != null)
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            
            Vector2 keyboardInput = new Vector2(horizontal, vertical);
            if (keyboardInput.sqrMagnitude > 0.01f)
            {
                rawInput += new Vector3(keyboardInput.x, keyboardInput.y, 0f);
            }
        }
        
        // ===== GAMEPAD INPUT =====
        if (Gamepad.current != null)
        {
            Vector2 stickInput = Gamepad.current.leftStick.ReadValue();
            if (stickInput.sqrMagnitude > 0.01f)
            {
                rawInput += new Vector3(stickInput.x, stickInput.y, 0f);
            }
        }
        
        // Clamp combined input
        if (rawInput.sqrMagnitude > 1f)
        {
            rawInput = rawInput.normalized;
        }
        
        // ===== SMOOTH INPUT FOR AC-STYLE FEEL =====
        float smoothTime = 1f / character.inputResponsiveness;
        smoothedInput = Vector3.SmoothDamp(smoothedInput, rawInput, ref inputVelocity, smoothTime);
        
        // Apply smoothed input
        character.inputDirection = smoothedInput;
        
        // Note: Boost input is now handled by BoostInputDetector
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

        // ===== AUTO FLYING/GLIDING TRANSITION =====
        // Don't auto-transition while boost is active
        if (boostSystem != null && !boostSystem.IsBoosting)
        {
            flightTimer += Time.deltaTime;
            
            if (isGliding)
            {
                // Currently gliding - check if it's time to start flapping again
                if (flightTimer >= character.glidingDuration)
                {
                    isGliding = false;
                    flightTimer = 0f;
                    Debug.Log("Switching from Gliding to Flying");
                }
            }
            else
            {
                // Currently flying - check if it's time to glide
                if (flightTimer >= character.flyingDuration)
                {
                    isGliding = true;
                    flightTimer = 0f;
                    Debug.Log("Switching from Flying to Gliding");
                }
            }
        }
        
        // ===== UPDATE SPEED via FlightSpeedController =====
        if (speedController != null)
        {
            FlightSpeedController.FlightMode mode = isGliding 
                ? FlightSpeedController.FlightMode.Gliding 
                : FlightSpeedController.FlightMode.Normal;
                
            speedController.UpdateSpeed(mode);
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();
        // Stay in flying state - add collision detection here if needed
    }

    /// <summary>
    /// Physics-based movement - AC-style smooth joystick control (left/right + up/down)
    /// Enhanced with momentum, drag, and flap boost mechanics
    /// Called in FixedUpdate()
    /// </summary>
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Note: BirdPathFollower (Runner) controls forward movement along the path
        // We apply lateral (X) and vertical (Y) steering forces, plus boost/lift

        // ===== SMOOTH LATERAL MOVEMENT (LEFT/RIGHT) =====
        float horizontalInput = character.inputDirection.x;
        
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // Calculate desired lateral velocity
            float targetLateralVelocity = horizontalInput * character.moveSpeed;
            
            // Smoothly accelerate towards target velocity
            lateralVelocity = Mathf.Lerp(lateralVelocity, targetLateralVelocity, Time.fixedDeltaTime * character.inputResponsiveness);
            
            // Apply smooth lateral movement
            currentLateralPosition += lateralVelocity * Time.fixedDeltaTime;
            currentLateralPosition = Mathf.Clamp(currentLateralPosition, -character.maxLateralDistance, character.maxLateralDistance);
            
            // Update path follower lateral offset
            if (pathFollower != null)
            {
                pathFollower.SetLateralOffset(currentLateralPosition);
            }
        }
        else
        {
            // No input - apply smooth deceleration using air drag
            lateralVelocity = Mathf.Lerp(lateralVelocity, 0f, Time.fixedDeltaTime * character.airDrag * 2f);
            
            // Continue drifting with remaining momentum
            currentLateralPosition += lateralVelocity * Time.fixedDeltaTime;
            currentLateralPosition = Mathf.Clamp(currentLateralPosition, -character.maxLateralDistance, character.maxLateralDistance);
            
            if (pathFollower != null)
            {
                pathFollower.SetLateralOffset(currentLateralPosition);
            }
        }
        
        // ===== SMOOTH VERTICAL MOVEMENT (UP/DOWN) =====
        float verticalInput = character.inputDirection.y;
        
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            // Calculate desired vertical velocity
            float targetVerticalVelocity = verticalInput * character.verticalSpeed;
            
            // Smoothly accelerate towards target velocity
            verticalVelocity = Mathf.Lerp(verticalVelocity, targetVerticalVelocity, Time.fixedDeltaTime * character.inputResponsiveness);
            
            // Apply smooth vertical movement
            currentVerticalOffset += verticalVelocity * Time.fixedDeltaTime;
            currentVerticalOffset = Mathf.Clamp(currentVerticalOffset, -character.maxVerticalOffset, character.maxVerticalOffset);
            
            // Update path follower vertical offset
            if (pathFollower != null)
            {
                pathFollower.heightOffset = currentVerticalOffset;
            }
        }
        else
        {
            // No input - apply smooth deceleration
            verticalVelocity = Mathf.Lerp(verticalVelocity, 0f, Time.fixedDeltaTime * character.airDrag * 2f);
            
            // Continue drifting with remaining momentum
            currentVerticalOffset += verticalVelocity * Time.fixedDeltaTime;
            currentVerticalOffset = Mathf.Clamp(currentVerticalOffset, -character.maxVerticalOffset, character.maxVerticalOffset);
            
            if (pathFollower != null)
            {
                pathFollower.heightOffset = currentVerticalOffset;
            }
        }

        // ===== AIR DRAG (AC-STYLE RESISTANCE) =====
        // Apply gentle drag to all movement for natural deceleration
        Vector3 dragForce = -character.rb.linearVelocity * character.airDrag;
        dragForce.z *= 0.1f; // Less drag on forward movement
        character.rb.AddForce(dragForce, ForceMode.Force);

        // ===== BOOST PHYSICS =====
        if (boostSystem != null)
        {
            boostSystem.ApplyBoostPhysics();
        }
        
        // ===== GLIDING LIFT =====
        if (isGliding && !boostSystem.IsBoosting)
        {
            // Subtle lift during gliding for realistic flight
            character.rb.AddForce(Vector3.up * 2f, ForceMode.Force);
        }

        // Note: Forward speed boost is handled by BoostSystem
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
            // Boost animation is handled by BoostSystem events
            // Just handle gliding vs flying here
            if (boostSystem != null && boostSystem.IsBoosting)
            {
                // Keep flapping animation during boost - don't override it!
            }
            else if (isGliding)
            {
                character.animationManager.PlayGliding();
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
        
        // Unsubscribe from boost input
        if (inputDetector != null)
        {
            inputDetector.OnBoostInputDetected -= OnBoostInput;
        }
    }
}
