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
            onPostAttackCoroutine = CoroutineHandler.StartCoroutine(OnPostAttackSequence(attackDir.normalized, player.GetPlayerStat.attackReboundPower, player.GetPlayerStat.attackReboundTime, 0.05f));
        }
    }

    IEnumerator OnPostAttackSequence(Vector2 dir, float reboundPower, float reboundTime, float shockAmount)
    {
        player.CanChangeState = false;
        player.RigidbodyComp.velocity = Vector2.zero;
        player.ControlParticles(ePlayerState.ATTACK_REBOUND, true);
        player.RigidbodyComp.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(reboundTime);
        yield return Yields.WaitSeconds(reboundTime);
        player.CanChangeState = true;
        player.ChangePrevState();
        player.ControlParticles(ePlayerState.ATTACK_REBOUND, false);

    }

    public override void OnStateExit()
    {

    }

}
