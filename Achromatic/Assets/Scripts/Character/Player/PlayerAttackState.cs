using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerAttackState : PlayerBaseState
{
    private Coroutine attackCoroutine;

    private GameObject attackPoint;
    private Attack meleeAttack;

    private Vector2 attackAngle;
    private bool canAttack = true;
    private bool sideAttackFormChangeTrigger = false;
    private float attackSideAnimationTimeScale = 1f;
    private int attackAnimationLayer = 2;
    public PlayerAttackState(Player player, GameObject atkPnt) : base(player) 
    {
        attackPoint = atkPnt;
        meleeAttack = attackPoint.transform.GetComponentInChildren<Attack>();

        meleeAttack.SetAttack(PlayManager.PLAYER_TAG, player);

        InputManager.Instance.LightAttackEvent.AddListener((Vector2 dir) =>
        {
            attackAngle = dir;

            if (player.CanChangeState)
            {
                player.ChangeState(EPlayerState.ATTACK);
            }
        });
    }

    public override void OnStateEnter()
    {
        if (canAttack)
        {
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
        CheckDirectionAndPlayAnimation(angle);
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        Vector2 angleVec = new Vector2(attackAngle.x - player.transform.position.x, attackAngle.y - player.transform.position.y);

        if (!player.IsCriticalAttack)
        {
            meleeAttack.AttackEnable(angleVec.normalized, player.GetPlayerStat.attackDamage, player.GetPlayerStat.colorAttackDamage);
        }
        else
        {
            player.IsCriticalAttack = false;
            meleeAttack.AttackEnable(angleVec.normalized, player.GetPlayerStat.attackDamage * player.GetPlayerStat.criticalAttackDamageMultiple, 
                player.GetPlayerStat.colorAttackDamage * player.GetPlayerStat.criticalAttackDamageMultiple);
        }

        yield return Yields.WaitSeconds(player.GetPlayerStat.attackTime);
        meleeAttack.AttackDisable();

        yield return Yields.WaitSeconds(player.GetPlayerStat.attackCooldown);
        canAttack = true;
    }

    private void CheckDirectionAndPlayAnimation(float angle)
    {
        angle += 180;
        if (angle > 135 && angle <= 225) // right
        {
            sideAttackFormChangeTrigger = !sideAttackFormChangeTrigger;
            player.AnimationComp.AnimationState.SetAnimation(attackAnimationLayer, PlayerAnimationNameCaching.ATTACK_ANIMATION[0, sideAttackFormChangeTrigger ? 0 : 1], false).TimeScale = attackSideAnimationTimeScale;
        }
        else if (angle > 225 && angle <= 315) // up
        {
            player.AnimationComp.AnimationState.SetAnimation(attackAnimationLayer, PlayerAnimationNameCaching.ATTACK_ANIMATION[1, 0], false);
        }
        else if (angle > 315 && angle <= 360 ||
            angle > 0 && angle <= 45) // left
        {
            sideAttackFormChangeTrigger = !sideAttackFormChangeTrigger;
            player.AnimationComp.AnimationState.SetAnimation(attackAnimationLayer, PlayerAnimationNameCaching.ATTACK_ANIMATION[0, sideAttackFormChangeTrigger ? 0 : 1], false).TimeScale = attackSideAnimationTimeScale;
        }
        else if (angle > 45 && angle <= 135) // down
        {
            player.AnimationComp.AnimationState.SetAnimation(attackAnimationLayer, PlayerAnimationNameCaching.ATTACK_ANIMATION[2, 0], false);
        }
        player.AnimationComp.state.AddEmptyAnimation(attackAnimationLayer, 0, 0f);
    }

    public override void OnStateFixedUpdate()
    {
    }

    public override void OnStateExit()
    {

    }

}
