using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerAttackState : PlayerBaseState
{
    private Coroutine attackCoroutine;
    private Coroutine afterAttackCoroutine;

    private GameObject attackPoint;
    private Attack meleeAttack;

    private Vector2 attackAngle;
    private bool canAttack = true;

    public PlayerAttackState(Player player, GameObject atkPnt) : base(player) 
    {
        attackPoint = atkPnt;
        meleeAttack = attackPoint.transform.GetComponentInChildren<Attack>();

        meleeAttack.SetAttack(PlayManager.PLAYER_TAG, player);

        InputManager.Instance.LightAttackEvent.AddListener((Vector2 dir) =>
        {
            attackAngle = dir;

            player.ChangeState(ePlayerState.ATTACK);
        });
    }

    public override void OnStateEnter()
    {
        if (canAttack)
        {
            player.AnimatorComp.SetTrigger("attackTrigger");
            attackCoroutine = CoroutineHandler.StartCoroutine(AttackSequence());
        }
        player.ChangePrevState();
    }

    public override void OnStateUpdate()
    {

    }

    IEnumerator AttackSequence()
    {
        canAttack = false;

        float angle;
        float horizontalValue = attackAngle.x - player.transform.position.x;
        float VerticalValue = attackAngle.y - player.transform.position.y;

        angle = Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg;
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        Vector2 angleVec = new Vector2(attackAngle.x - player.transform.position.x, attackAngle.y - player.transform.position.y);

        if (!player.IsCriticalAttack)
        {
            meleeAttack.AttackAble(angleVec.normalized, player.GetPlayerStat.attackDamage, false, player.GetPlayerStat.colorAttackDamage);
        }
        else
        {
            player.IsCriticalAttack = false;
            meleeAttack.AttackAble(angleVec.normalized, player.GetPlayerStat.criticalAttackDamage, false, player.GetPlayerStat.colorCriticalAttackDamage);
        }

        yield return Yields.WaitSeconds(player.GetPlayerStat.attackTime);
        meleeAttack.AttackDisable();

        yield return Yields.WaitSeconds(player.GetPlayerStat.attackCooldown);
        canAttack = true;
    }

    public void AfterAttack(Vector2 attackDir)
    {
        afterAttackCoroutine = CoroutineHandler.StartCoroutine(AttackReboundSequence(attackDir.normalized, player.GetPlayerStat.attackReboundPower, player.GetPlayerStat.attackReboundTime, 0.05f));

    }

    IEnumerator AttackReboundSequence(Vector2 dir, float reboundPower, float reboundTime, float shockAmount)
    {
        player.RigidbodyComp.velocity = Vector2.zero;
        player.ControlParticles(ePlayerState.ATTACK ,true);
        player.RigidbodyComp.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(shockAmount);
        yield return Yields.WaitSeconds(reboundTime);
        player.ControlParticles(ePlayerState.ATTACK, false);

    }

    public override void OnStateFixedUpdate()
    {
    }

    public override void OnStateExit()
    {

    }

}
