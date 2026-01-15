using UnityEngine;

public class State
{
    public Character character;
    public StateMachine stateMachine;

    public State(Character _character, StateMachine _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public virtual void Enter() { }

    public virtual void HandleInput() { }

    public virtual void LogicUpdate() { }

    public virtual void ChangeState() { }

    public virtual void PhysicsUpdate() { }

    public virtual void UpdateAnimation() { }

    public virtual void Exit() { }

}

