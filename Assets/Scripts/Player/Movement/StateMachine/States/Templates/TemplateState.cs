using UnityEngine;

public class TemplateState : PlayerState
{
    public TemplateState(StateMachine stateMachine, PlayerStateConfig config) : base(stateMachine, config)
    {

    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
