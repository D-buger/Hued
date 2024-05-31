using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackReboundState : PlayerBaseState
{
    private Coroutine onPostAttackCoroutine;

    public PlayerAttackReboundState(Player player) : base(player)
    {
    }

    public override void OnStateEnter()
    {

    }
    public override void OnStateUpdate()
    {

    }

    public void OnPostAttack(Vector2 attackDir)
    {
        if (!player.IsDash && !player.IsParryDash)
        {
            onPostAttackCoroutine = CoroutineHandler.StartCoroutine(OnPostAttackSequence(attackDir.normalized, player.GetPlayerStat.attackReboundPower, player.GetPlayerStat.attackReboundTime));
        }
    }

    IEnumerator OnPostAttackSequence(Vector2 dir, float reboundPower, float reboundTime)
    {
        player.CanChangeState = false;
        player.ControlParticles(EPlayerState.ATTACK_REBOUND, true);
        player.RigidbodyComp.velocity = -dir * reboundPower;
        //player.RigidbodyComp.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(reboundTime);
        yield return Yields.WaitSeconds(reboundTime);
        player.CanChangeState = true;
        player.ChangePrevState();
        player.ControlParticles(EPlayerState.ATTACK_REBOUND, false);
    }

    public override void OnStateExit()
    {

    }

}
