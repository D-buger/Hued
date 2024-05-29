using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReboundState : PlayerBaseState
{
    private Coroutine reboundCoroutine;
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
        player.RigidbodyComp.velocity = Vector2.zero;
        player.RigidbodyComp.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(reboundTime);
        player.CanChangeState = true;
        player.ChangePrevState();
    }

    public override void OnStateExit()
    {

    }
}
