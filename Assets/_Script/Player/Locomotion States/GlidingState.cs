using UnityEngine;

public class GlidingState : State
{
    public GlidingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        // Maintain current velocity but add glide characteristics
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
        // Get input direction for steering
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        character.inputDirection = new Vector3(horizontal, vertical, 0f).normalized;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void ChangeState()
    {
        base.ChangeState();
        
        // Flap to start flying
        if (Input.GetButtonDown("Jump"))
        {
            stateMachine.ChangeState(character.flyingState);
        }
        
        // Dive if pressing down
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            stateMachine.ChangeState(character.divingState);
        }
        
        // Land if grounded
        if (character.isGrounded)
        {
            stateMachine.ChangeState(character.standingState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Apply gravity but with better glide ratio
        character.playerVelocity.y += (character.gravityValue * 0.3f) * Time.fixedDeltaTime;
        
        // Maintain forward momentum
        if (character.playerVelocity.magnitude < character.glideSpeed)
        {
            character.playerVelocity += character.transform.forward * (character.glideSpeed * 0.5f) * Time.fixedDeltaTime;
        }
        
        // Apply air resistance (less than flying for better glide)
        character.playerVelocity *= 0.995f;
        
        // Handle steering
        if (character.inputDirection != Vector3.zero)
        {
            // Horizontal steering
            Vector3 steeringForce = character.transform.right * character.inputDirection.x * character.turnSpeed;
            character.playerVelocity += steeringForce * Time.fixedDeltaTime;
            
            // Vertical steering (limited)
            character.playerVelocity.y += character.inputDirection.y * character.turnSpeed * 0.5f * Time.fixedDeltaTime;
            
            // Rotate towards movement direction
            if (character.playerVelocity.magnitude > 1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(character.playerVelocity.normalized);
                character.transform.rotation = Quaternion.Slerp(character.transform.rotation, targetRotation, character.turnSpeed * 0.5f * Time.fixedDeltaTime);
            }
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
            character.animator.SetBool("IsFlying", false);
            character.animator.SetBool("IsGliding", true);
            character.animator.SetBool("IsDiving", false);
            character.animator.SetBool("IsGrounded", false);
            character.animator.SetFloat("VerticalSpeed", character.playerVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}