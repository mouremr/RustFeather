using System;
using UnityEngine;

public class AttackState : PlayerState
{
    //private AnimatorStateInfo stateInfo;
    private float attackDuration;
    private float attackTimer = 0f;
    private string attackType;
    private float attackforce;
    private bool resetLegs;

    public AttackState(StateMachine stateMachine, PlayerStateConfig config, String attack, float force, bool resetLegs) : base(stateMachine, config)
    {
        attackType = attack;
        attackforce = force;
        this.resetLegs = resetLegs;
    }

    public override void Update()
    {
        //rb.linearVelocity = new Vector2(rb.linearVelocityX * input.HorizontalInput, rb.linearVelocityY);
        //rb.linearVelocity = Vector2.zero;
        //rb.AddForce(new Vector2(attackforce * input.HorizontalInput, 0), ForceMode2D.Impulse);
        // get attack animation length
        attackDuration = animator.GetCurrentAnimatorStateInfo(2).length;  //2 is the weapon attack layer
        attackTimer += Time.deltaTime;

        
        if(attackTimer >= attackDuration)
        {
            animator.SetBool(attackType, false);
            float legsNormalizedTime = animator.GetCurrentAnimatorStateInfo(1).normalizedTime % 1f;
            animator.Play("movement Body", 0, legsNormalizedTime);
            stateMachine.ChangeState(stateMachine.States.Grounded(resetLegs));
        }
    }

    public override void Enter()
    {
        animator.SetBool(attackType, true);        
    }



}