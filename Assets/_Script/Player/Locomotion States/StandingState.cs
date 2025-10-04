using UnityEngine;

public class StandingState: State
{    
    
    public StandingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
	{
		character = _character;
		stateMachine = _stateMachine;
	}

    public override void Enter()
    {
        base.Enter();     
        
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
        // Get movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        character.inputDirection = new Vector3(horizontal, 0f, vertical);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();       
        
    }

    public override void ChangeState()
    {
        base.ChangeState();

        // Jump to flying instead of jumping
        if (Input.GetButtonDown("Jump"))
        {
            stateMachine.ChangeState(character.flyingState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Ground movement
        if (character.inputDirection != Vector3.zero)
        {
            // Rotate towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(character.inputDirection);
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, targetRotation, character.rotationSpeed * Time.fixedDeltaTime);
            
            // Move forward
            Vector3 moveDirection = character.transform.forward * character.playerSpeed;
            character.controller.Move(moveDirection * Time.fixedDeltaTime);
        }
        
        // Apply gravity
        if (!character.controller.isGrounded)
        {
            character.playerVelocity.y += character.gravityValue * Time.fixedDeltaTime;
        }
        else
        {
            character.playerVelocity.y = -2f; // Small downward force to stay grounded
        }
        
        character.controller.Move(character.playerVelocity * Time.fixedDeltaTime);
        character.isGrounded = character.controller.isGrounded;
    }

    public override void UpdateAnimation()
    {
        base.UpdateAnimation();
        
        if (character.animator != null)
        {
            character.animator.SetBool("IsFlying", false);
            character.animator.SetBool("IsGliding", false);
            character.animator.SetBool("IsDiving", false);
            character.animator.SetBool("IsGrounded", true);
            character.animator.SetFloat("Speed", character.inputDirection.magnitude);
        }
    }

    public override void Exit()
    {
        base.Exit();      
        
    }


}
