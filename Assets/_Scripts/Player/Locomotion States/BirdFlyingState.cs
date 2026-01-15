using UnityEngine;

/// <summary>
/// BirdFlyingState - Simple flying state for the bird
/// Handles smooth path following with basic input reading
/// </summary>
public class BirdFlyingState : State
{
    // Component references
    private CharacterPathFollower pathFollower;
    private CharacterInput characterInput;

    public BirdFlyingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        // Get component references
        pathFollower = character.GetComponent<CharacterPathFollower>();
        characterInput = character.GetComponent<CharacterInput>();

        // Validate components
        if (pathFollower == null)
        {
            Debug.LogError("❌ CharacterPathFollower not found on Character!");
        }
        else
        {
            Debug.Log($"✓ CharacterPathFollower found! Current speed: {pathFollower.GetSpeed()}");
        }

        if (characterInput == null)
        {
            Debug.LogWarning("⚠️ CharacterInput not found - input will not work!");
        }
        else
        {
            Debug.Log("✓ CharacterInput found!");
        }

        Debug.Log("🦅 BirdFlyingState: Entered flying state");
    }

    /// <summary>
    /// Handle input - reads input from CharacterInput
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        if (characterInput == null) return;

        // Read input and store in character
        Vector2 input = characterInput.GetInput();
        character.inputDirection = new Vector3(input.x, input.y, 0f);
    }

    /// <summary>
    /// Update logic - currently just path following
    /// </summary>
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Path follower handles movement automatically via Runner system
        // Input is read but not used yet (for future lateral/vertical movement)
    }

    /// <summary>
    /// Physics update - path following happens in CharacterPathFollower
    /// </summary>
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // CharacterPathFollower (Runner) handles all physics movement
        // We don't need to manually move the rigidbody here
    }

    /// <summary>
    /// Update animations (placeholder for future animation system)
    /// </summary>
    public override void UpdateAnimation()
    {
        base.UpdateAnimation();

        // TODO: Add animation updates here when animation system is ready
        // Example: character.animationManager.PlayFlying();
    }

    public override void ChangeState()
    {
        base.ChangeState();

        // Stay in flying state for now
        // Add state transitions here when you add more states
    }

    public override void Exit()
    {
        base.Exit();

        Debug.Log("🦅 BirdFlyingState: Exited flying state");
    }
}
