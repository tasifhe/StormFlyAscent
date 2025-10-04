using UnityEngine;

public class DivingState : State
{
    private float diveTimer;
    private bool canRecover;

    public DivingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        diveTimer = 0f;
        canRecover = true;
        
        // Give initial dive momentum
        character.playerVelocity.y = -character.diveSpeed * 0.5f;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
        // Get horizontal input for steering during dive
        float horizontal = Input.GetAxis("Horizontal");
        character.inputDirection = new Vector3(horizontal, 0f, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        diveTimer += Time.deltaTime;
        
        // Allow recovery after minimum dive time
        if (diveTimer > 0.5f && canRecover)
        {
            if (Input.GetButtonDown("Jump") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                // Recover by flapping
                stateMachine.ChangeState(character.flyingState);
            }
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();
        
        // Auto-transition to gliding if dive speed decreases
        if (character.playerVelocity.y > -5f && diveTimer > 1f)
        {
            stateMachine.ChangeState(character.glidingState);
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
        
        // Accelerate downward
        character.playerVelocity.y += character.gravityValue * 1.5f * Time.fixedDeltaTime;
        
        // Clamp dive speed
        if (character.playerVelocity.y < -character.diveSpeed)
        {
            character.playerVelocity.y = -character.diveSpeed;
        }
        
        // Add forward momentum during dive
        character.playerVelocity += character.transform.forward * character.diveSpeed * 0.3f * Time.fixedDeltaTime;
        
        // Horizontal steering during dive
        if (character.inputDirection.x != 0f)
        {
            character.playerVelocity += character.transform.right * character.inputDirection.x * character.turnSpeed * Time.fixedDeltaTime;
        }
        
        // Apply some air resistance
        character.playerVelocity *= 0.99f;
        
        // Rotate to face dive direction
        Vector3 diveDirection = character.playerVelocity.normalized;
        if (diveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(diveDirection);
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, targetRotation, character.turnSpeed * Time.fixedDeltaTime);
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
            character.animator.SetBool("IsGliding", false);
            character.animator.SetBool("IsDiving", true);
            character.animator.SetBool("IsGrounded", false);
            character.animator.SetFloat("VerticalSpeed", character.playerVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        diveTimer = 0f;
        canRecover = true;
    }
}