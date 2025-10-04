using UnityEngine;

public class FlyingState : State
{
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
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
        // Get input direction for turning
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        character.inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Flap input (space or jump button)
        if (Input.GetButtonDown("Jump") && !isFlapping)
        {
            isFlapping = true;
            flapTimer = 0.2f; // Flap duration
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
        
        // Transition to gliding if no input and moving downward
        if (!isFlapping && character.playerVelocity.y < -1f && !Input.GetButton("Jump"))
        {
            stateMachine.ChangeState(character.glidingState);
        }
        
        // Transition to diving if down input
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            stateMachine.ChangeState(character.divingState);
        }
        
        // Check for ground collision to land
        if (character.isGrounded && character.playerVelocity.y <= 0f)
        {
            stateMachine.ChangeState(character.standingState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Apply wing flap force
        if (isFlapping)
        {
            character.playerVelocity.y = character.flapStrength;
        }
        else
        {
            // Apply gravity with some lift
            character.playerVelocity.y += character.gravityValue * Time.fixedDeltaTime;
            character.playerVelocity.y += character.liftForce * Time.fixedDeltaTime;
        }
        
        // Apply air resistance
        character.playerVelocity *= character.airResistance;
        
        // Handle turning
        if (character.inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(character.inputDirection);
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, targetRotation, character.turnSpeed * Time.fixedDeltaTime);
            
            // Add forward momentum
            character.playerVelocity += character.transform.forward * character.glideSpeed * Time.fixedDeltaTime;
        }
        
        // Move the character
        character.controller.Move(character.playerVelocity * Time.fixedDeltaTime);
        
        // Check ground
        character.isGrounded = character.controller.isGrounded;
    }

    public override void UpdateAnimation()
    {
        base.UpdateAnimation();
        
        if (character.animator != null)
        {
            character.animator.SetBool("IsFlying", true);
            character.animator.SetBool("IsGliding", false);
            character.animator.SetBool("IsDiving", false);
            character.animator.SetBool("IsGrounded", false);
            character.animator.SetBool("IsFlapping", isFlapping);
            character.animator.SetFloat("VerticalSpeed", character.playerVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        isFlapping = false;
    }
}