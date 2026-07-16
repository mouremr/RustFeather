using System;
using Unity.Mathematics;
using UnityEngine;


public class RollingState: PlayerState
{
    private float rollSpeed;
    private float moveSpeed;

    private float rollTimer;
    private float rollDuration;

    private int playerLayer = LayerMask.NameToLayer("Player");
    private int enemyLayer = LayerMask.NameToLayer("Enemy");

    public RollingState(StateMachine stateMachine, PlayerStateConfig config, float moveSpeed) : base(stateMachine, config)
    {
        this.moveSpeed=moveSpeed;
        rollSpeed = config.rollSpeed;
        rollDuration = config.rollDuration;
    }

    public override void Enter()
    {
        animator.SetBool("rolling",true);
        rollTimer=rollDuration;
        rb.linearVelocity=Vector2.zero;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        
        float dir = torsoSpriteRenderer.flipX ? -1f : 1f;

        rb.AddForce(new Vector2(rollSpeed * dir, 0), ForceMode2D.Impulse);
        return;
    }
    public override void Update()
    {
        rollTimer -= Time.deltaTime;

        if (rollTimer <= 0f )
        {
            //Debug.Log("end roll");
            animator.SetBool("rolling", false);
            stateMachine.ChangeState(stateMachine.States.Grounded(true));
            return;
        }
    }

    public override void Exit()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }
}