using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReboundState : PlayerBaseState
{
    private Coroutine reboundCoroutine;

    private float reboundMulVelocity = 40;
    public PlayerReboundState(Player player) : base(player)
    {

    }

    public override void OnStateEnter()
    {
    }

    public override void OnStateUpdate()
    {

    }

    public void Rebound(Vector2 dir, float reboundPower, float reboundTime)
    {
        reboundCoroutine = CoroutineHandler.StartCoroutine(ReboundSequence(dir.normalized, reboundPower, reboundTime));
    }

    private IEnumerator ReboundSequence(Vector2 dir, float reboundPower, float reboundTime)
    {
        player.CanChangeState = false;
        player.RigidbodyComp.velocity = new Vector2(-dir.x * reboundMulVelocity * reboundPower, 0);
        yield return Yields.WaitSeconds(reboundTime);
        player.CanChangeState = true;
        player.ChangePrevState();
    }

    public override void OnStateExit()
    {

    }
}
