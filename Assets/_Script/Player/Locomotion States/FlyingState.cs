using UnityEngine;

public class FlyingState : State
{
    private DynamicJoystick joystick;
    private float flapTimer;
    private bool isFlapping;

    public FlyingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        character.playerVelocity = Vector3.zero;
        flapTimer = 0f;
        isFlapping = false;

        // Find joystick in scene
        joystick = UnityEngine.Object.FindFirstObjectByType<DynamicJoystick>();
    }

    public override void HandleInput()
    {
        base.HandleInput();

        // Joystick input for mobile
        if (joystick != null)
        {
            // Horizontal input for turning
            character.inputDirection = new Vector3(joystick.Horizontal, 0f, joystick.Vertical).normalized;
        }
        else
        {
            // Fallback to keyboard for testing
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            character.inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }

        // Flap input - check for joystick movement or button press
        bool shouldFlap = false;
        
        if (joystick != null)
        {
            // Flap when joystick is moved up strongly
            if (joystick.Vertical > 0.7f && !isFlapping)
            {
                shouldFlap = true;
            }
        }
        
        // Fallback keyboard input
        if (Input.GetButtonDown("Jump"))
        {
            shouldFlap = true;
        }
        
        if (shouldFlap)
        {
            isFlapping = true;
            flapTimer = 0.3f; // Flap duration
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (isFlapping)
        {
            flapTimer -= Time.deltaTime;
            if (flapTimer <= 0f)
            {
                isFlapping = false;
            }
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();

        // For now, just stay in flying state
        // Add other state transitions here when needed
        // Example: transition to crash state if hit obstacle
        // Example: transition to landing state when touching ground
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Calculate desired velocity
        Vector3 velocity = character.rb.linearVelocity;

        // Apply wing flap force
        if (isFlapping)
        {
            velocity.y = character.flapStrength;
        }
        else
        {
            // Apply gravity with some lift
            velocity.y += character.gravityValue * Time.fixedDeltaTime;
            velocity.y += character.liftForce * Time.fixedDeltaTime;
        }

        // Apply air resistance
        velocity *= character.airResistance;

        // Always move forward (endless runner style)
        velocity += character.transform.forward * character.glideSpeed * Time.fixedDeltaTime;
        
        // Handle turning based on joystick input
        if (character.inputDirection != Vector3.zero)
        {
            // Turn left/right based on horizontal input
            if (Mathf.Abs(character.inputDirection.x) > 0.1f)
            {
                // Smooth rotation for mobile
                float turnAmount = character.inputDirection.x * character.turnSpeed * 30f * Time.fixedDeltaTime;
                character.transform.Rotate(Vector3.up, turnAmount);
            }
        }

        // Set the velocity
        character.rb.linearVelocity = velocity;

        // Check ground (simple check)
        character.isGrounded = Physics.Raycast(character.transform.position, Vector3.down, 1.1f);
    }

    // public override void UpdateAnimation()
    // {
    //     base.UpdateAnimation();

    //     if (character.animationManager != null)
    //     {
    //         // Play appropriate animation based on flying state
    //         if (isFlapping)
    //         {
    //             // Play flapping animation with quick fade
    //             character.animationManager.PlayFlapping();
    //         }
    //         else
    //         {
    //             // Play flying/gliding animation
    //             character.animationManager.PlayFlying();
    //         }
    //     }
    // }

    public override void Exit()
    {
        base.Exit();
        isFlapping = false;
    }
}
