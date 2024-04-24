using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private Coroutine hitCoroutine;

    public PlayerHitState(Player player) : base(player) { }

    public override void OnStateEnter()
    {
        //Debug.Log("Player State : Hit");
    }
    public override void OnStateUpdate()
    {

    }
    public void Hit(int damage, Vector2 attackDir)
    {
        player.ParryCondition = false;
        if (!player.IsInvincibility)
        {
            player.RigidbodyComp.velocity = Vector2.zero;
            attackDir.y = 0;
            player.currentHP -= damage;
            hitCoroutine = CoroutineHandler.StartCoroutine(HitReboundSequence(attackDir.normalized, player.GetPlayerStat.hitReboundPower, player.GetPlayerStat.hitReboundTime, 0.1f));
        }
    }

    IEnumerator HitReboundSequence(Vector2 dir, float reboundPower, float reboundTime, float shockAmount)
    {
        player.CanChangeState = false;
        player.IsInvincibility = true;
        player.AnimatorComp.SetTrigger("hitTrigger");
        player.ControlParticles(ePlayerState.HIT, true);
        player.RigidbodyComp.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(shockAmount);
        yield return Yields.WaitSeconds(reboundTime);

        yield return Yields.WaitSeconds(player.GetPlayerStat.hitBehaviourLimitTime);
        player.ControlParticles(ePlayerState.HIT, false);
        player.CanChangeState = true;
        player.ChangeState(ePlayerState.IDLE);

        yield return Yields.WaitSeconds(Mathf.Max(0, Mathf.Abs(player.GetPlayerStat.hitInvincibilityTime - player.GetPlayerStat.hitBehaviourLimitTime)));
        player.IsInvincibility = false;
    }

    public override void OnStateFixedUpdate()
    {

    }
    public override void OnStateExit()
    {

    }

    public void AfterAttack(Vector2 attackDir)
    {

    }

}