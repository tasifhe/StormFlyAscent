using UnityEngine;

/// <summary>
/// FlyingState - AC-style bird flight with joystick (left/right, up/down) and tap-to-flap boost
/// </summary>
public class FlyingState : State
{
    // Input references
    private DynamicJoystick joystick;           // For joystick input
    private BirdPathFollower pathFollower;       // Reference to path follower
    
    // Input smoothing for AC-style responsiveness
    private Vector3 smoothedInput = Vector3.zero;
    private Vector3 inputVelocity = Vector3.zero;
    
    // Speed management
    private float currentSpeedMultiplier = 1f;
    private float targetSpeedMultiplier = 1f;
    private float previousAltitude = 0f;
    
    // Flap boost mechanic (replaces dive)
    private bool isFlapBoosting = false;         // Currently boosting from flap
    private float flapBoostTimer = 0f;           // Tracks boost duration
    private float flapCooldownTimer = 0f;        // Tracks cooldown between flaps
    
    // Auto gliding mechanic variables
    private bool isGliding = false;              // Currently in gliding phase
    private float flightTimer = 0f;              // Tracks time in current flight phase
    
    // Movement tracking
    private float currentLateralPosition = 0f;   // Track left/right position from center
    private float currentVerticalOffset = 0f;    // Track up/down position from path
    private float lateralVelocity = 0f;          // Smooth lateral movement
    private float verticalVelocity = 0f;         // Smooth vertical movement

    public FlyingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        
        isFlapBoosting = false;
        flapBoostTimer = 0f;
        flapCooldownTimer = 0f;
        
        // Start with active flying (flapping)
        isGliding = false;
        flightTimer = 0f;
        
        // Reset smooth values
        smoothedInput = Vector3.zero;
        inputVelocity = Vector3.zero;
        currentSpeedMultiplier = 1f;
        targetSpeedMultiplier = 1f;
        lateralVelocity = 0f;
        verticalVelocity = 0f;
        
        currentLateralPosition = 0f;
        currentVerticalOffset = 0f;
        previousAltitude = character.transform.position.y;

        // Get path follower reference
        pathFollower = character.GetComponent<BirdPathFollower>();

        // Find joystick
        joystick = UnityEngine.Object.FindFirstObjectByType<DynamicJoystick>();
        
        if (joystick != null)
        {
            Debug.Log("Joystick found - AC-style controls active (Left/Right + Up/Down, Tap for boost)");
        }
        else
        {
            Debug.Log("No joystick - using keyboard (WASD/Arrows + Space for boost)");
        }
    }

    /// <summary>
    /// Handle input detection - AC-style joystick control (left/right + up/down) and tap-to-flap
    /// Called in Update()
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        Vector3 rawInput = Vector3.zero;

        // ===== JOYSTICK/KEYBOARD INPUT (AC-STYLE) =====
        if (joystick != null)
        {
            // Joystick: Horizontal = left/right, Vertical = up/down
            rawInput = new Vector3(joystick.Horizontal, joystick.Vertical, 0f);
        }
        else
        {
            // Keyboard fallback: WASD or Arrow keys
            float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
            float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrows
            rawInput = new Vector3(horizontal, vertical, 0f);
        }
        
        // ===== SMOOTH INPUT FOR AC-STYLE FEEL =====
        // Use SmoothDamp for natural acceleration/deceleration
        float smoothTime = 1f / character.inputResponsiveness;
        smoothedInput = Vector3.SmoothDamp(smoothedInput, rawInput, ref inputVelocity, smoothTime);
        
        // Apply smoothed input
        character.inputDirection = smoothedInput;

        // ===== TAP-TO-FLAP BOOST INPUT =====
        DetectFlapBoostInput();
    }

    /// <summary>
    /// Detect tap input for flap boost (AC-style speed burst)
    /// Supports both touch (mobile) and keyboard (testing)
    /// </summary>
    private void DetectFlapBoostInput()
    {
        bool tapDetected = false;

        // Mobile touch input (tap anywhere on screen)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Make sure touch isn't on the joystick area
            Touch touch = Input.GetTouch(0);
            if (joystick == null || !RectTransformUtility.RectangleContainsScreenPoint(
                joystick.GetComponent<RectTransform>(), touch.position))
            {
                tapDetected = true;
            }
        }
        
        // Editor testing - Space bar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            tapDetected = true;
        }

        // Execute flap boost if tap detected and cooldown ready
        if (tapDetected && flapCooldownTimer <= 0f && !isFlapBoosting)
        {
            StartFlapBoost();
        }
    }

    /// <summary>
    /// Initialize flap boost mechanic (AC-style speed burst)
    /// </summary>
    private void StartFlapBoost()
    {
        isFlapBoosting = true;
        flapBoostTimer = character.flapBoostDuration;
        flapCooldownTimer = character.flapCooldown;
        
        // Reset gliding timer - flapping resets the cycle
        isGliding = false;
        flightTimer = 0f;
        
        Debug.Log("Flap boost activated! 🦅");
    }

    /// <summary>
    /// Update timers and state logic
    /// Called in Update()
    /// </summary>
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Update flap boost timer
        if (isFlapBoosting)
        {
            flapBoostTimer -= Time.deltaTime;
            if (flapBoostTimer <= 0f)
            {
                isFlapBoosting = false;
                Debug.Log("Flap boost ended");
            }
        }

        // Update cooldown timer
        if (flapCooldownTimer > 0f)
        {
            flapCooldownTimer -= Time.deltaTime;
        }
        
        // ===== AUTO FLYING/GLIDING TRANSITION =====
        // Don't auto-transition while flap boosting
        if (!isFlapBoosting)
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
        
        // ===== DYNAMIC SPEED CALCULATION (AC-STYLE) =====
        // Calculate altitude change
        float currentAltitude = character.transform.position.y;
        float altitudeChange = (currentAltitude - previousAltitude) / Time.deltaTime;
        previousAltitude = currentAltitude;
        
        // Determine target speed multiplier based on state
        if (isFlapBoosting)
        {
            targetSpeedMultiplier = character.flapSpeedMultiplier;
        }
        else if (isGliding)
        {
            targetSpeedMultiplier = character.glideSpeedMultiplier;
        }
        else
        {
            targetSpeedMultiplier = 1f;
        }
        
        // Add altitude influence (descending = faster, climbing = slower)
        float altitudeSpeedMod = -altitudeChange * character.altitudeSpeedInfluence * 0.01f;
        targetSpeedMultiplier += altitudeSpeedMod;
        
        // Clamp speed multiplier to reasonable range
        targetSpeedMultiplier = Mathf.Clamp(targetSpeedMultiplier, 0.5f, 2.5f);
        
        // Smooth transition to target speed
        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, Time.deltaTime * 2f);
        
        // Apply speed to path follower
        if (pathFollower != null)
        {
            pathFollower.followSpeed = character.forwardSpeed * currentSpeedMultiplier;
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

        // ===== FLAP BOOST MECHANIC (AC-STYLE) =====
        if (isFlapBoosting)
        {
            // Apply forward boost force
            Vector3 boostDirection = character.transform.forward;
            character.rb.AddForce(boostDirection * character.flapBoostForce, ForceMode.Acceleration);
        }
        else if (isGliding)
        {
            // Subtle lift during gliding for realistic flight
            character.rb.AddForce(Vector3.up * 2f, ForceMode.Force);
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
            if (isFlapBoosting)
            {
                // Use flying animation during flap boost (active flapping)
                character.animationManager.PlayFlying();
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
        isFlapBoosting = false;
    }
}
