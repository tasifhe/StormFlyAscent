using UnityEngine;

public class JumpingState:State
{

    public JumpingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
	{
		character = _character;
		stateMachine = _stateMachine;
	}
   
    public override void Enter()
    {
        base.Enter();
        
        // Initial jump force
        character.playerVelocity.y = Mathf.Sqrt(character.jumpHeight * -2f * character.gravityValue);
    }

	public override void HandleInput()
	{
		base.HandleInput();     
       
    }

	public override void LogicUpdate()
    {
        base.LogicUpdate();    
       
    }

    public override void ChangeState()
    {
        base.ChangeState();

        // Transition to flying when jump peaks or on input
        if (character.playerVelocity.y < 0f || Input.GetButtonDown("Jump"))
        {
            stateMachine.ChangeState(character.flyingState);
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
        
        // Apply gravity
        character.playerVelocity.y += character.gravityValue * Time.fixedDeltaTime;
        
        // Air control
        if (character.inputDirection != Vector3.zero)
        {
            Vector3 airMovement = character.inputDirection * character.playerSpeed * character.airControl;
            character.playerVelocity.x += airMovement.x * Time.fixedDeltaTime;
            character.playerVelocity.z += airMovement.z * Time.fixedDeltaTime;
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

