using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private Coroutine dashCoroutine;
    private Coroutine parryDashCoroutine;
    private Coroutine parryCoroutine;

    private bool canParryDash = true;
    private bool canDash = true;
    private bool canDashAfterParry = true;
    private bool isParry = false;

    private Vector2 dashDirection;

    private float originGravityScale;
    private float originLiniearDrag;
    private float originMass;

    private float dashAnimationTimeScale = 0.5f;
    private int dashAnimationLayer = 1;
    public PlayerDashState(Player player) : base(player)
    {
        originGravityScale = player.RigidbodyComp.gravityScale;
        originLiniearDrag = player.RigidbodyComp.drag;
        originMass = player.RigidbodyComp.mass;

        InputManager.Instance.DashEvent.AddListener((Vector2 dir) =>
        {
            if ( player.CanChangeState && (canDash || canParryDash))
            {
                dashDirection = dir;

                player.ChangeState(EPlayerState.DASH);
            }
        });
    }

    public override void OnStateEnter()
    {
        if (canParryDash && isParry && !player.IsDash)
        {
            parryDashCoroutine = CoroutineHandler.StartCoroutine(ParryDashSequence(dashDirection));
        }
        else if (canDash && !isParry && canDashAfterParry && !player.IsParryDash)
        {
            dashCoroutine = CoroutineHandler.StartCoroutine(DashSequence(dashDirection));
        }
        else
        {
            player.ChangePrevState();
        }
    }

    public override void OnStateUpdate()
    {

    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        float elapsedTime = 0;
        player.CanChangeState = false;
        player.IsDash = true;
        canDash = false;
        canParryDash = false;
        player.StopDash = false;

        TogglePhysics(false);
        player.ControlParticles(EPlayerState.DASH, true);

        dashPos.x = dashPos.x - player.transform.position.x;
        dashPos.y = dashPos.y - player.transform.position.y;

        player.RigidbodyComp.velocity = dashPos.normalized * player.GetPlayerStat.dashPower;

        player.PlayerFaceRight = dashPos.x > 0 ? true : false;
        CheckDirectionAndPlayAnimation(Mathf.Atan2(dashPos.y, dashPos.x) * Mathf.Rad2Deg);

        while (true)
        {
            elapsedTime += Time.deltaTime;
            if (player.StopDash || elapsedTime > player.GetPlayerStat.dashingTime)
            {
                player.StopDash = false;
                TogglePhysics(true);
                break;
            }
            yield return null;
        }

        player.ControlParticles(EPlayerState.DASH, false);
        player.AnimationComp.AnimationState.SetEmptyAnimation(dashAnimationLayer, 0);
        isParry = player.ParryCondition;
        if (isParry)
        {
            parryCoroutine = CoroutineHandler.StartCoroutine(ParrySequence());
        }
        player.ParryCondition = false;
        player.IsDash = false;

        player.CanChangeState = true;
        player.ChangeState(EPlayerState.IDLE);
        yield return Yields.WaitSeconds(player.GetPlayerStat.dashAfterDelay);
        canParryDash = true;

        elapsedTime = 0;
        while (true)
        {
            elapsedTime += Time.deltaTime;

            UISystem.Instance.dashCooldownEvent.Invoke(elapsedTime / player.GetPlayerStat.dashCooldown);
            if (elapsedTime > player.GetPlayerStat.dashCooldown)
            {
                break;
            }
            yield return null;
        }
        yield return Yields.WaitSeconds(player.GetPlayerStat.dashAfterDelay);
        canDash = true;
    }
    IEnumerator ParrySequence()
    {
        player.IsInvincibility = true;

        Time.timeScale = player.GetPlayerStat.parryProduceTimescale;
        player.AnimationComp.AnimationState.SetAnimation(dashAnimationLayer, PlayerAnimationNameCaching.PARRY_ANIMATION, false);
        player.AnimationComp.AnimationState.AddEmptyAnimation(dashAnimationLayer, 0, 0);
        player.ControlParticles(EPlayerState.DASH ,true, 1);
        yield return Yields.WaitSeconds(player.GetPlayerStat.parryProduceTime);
        Time.timeScale = 1f;
        player.ControlParticles(EPlayerState.DASH, false, 1);

        yield return Yields.WaitSeconds(player.GetPlayerStat.invincibilityAfterParry);
        player.IsInvincibility = false;
    }


    IEnumerator ParryDashSequence(Vector2 dashPos)
    {
        float elapsedTime = 0;
        player.CanChangeState = false;
        isParry = false;
        player.IsParryDash = true;
        canParryDash = false;
        canDashAfterParry = false;
        player.IsCriticalAttack = true;

        TogglePhysics(false);
        player.ColliderComp.forceReceiveLayers &= ~PlayManager.Instance.EnemyMask;
        player.ColliderComp.forceSendLayers &= ~PlayManager.Instance.EnemyMask;
        player.ControlParticles(EPlayerState.DASH, true);

        dashPos.x = dashPos.x - player.transform.position.x;
        dashPos.y = dashPos.y - player.transform.position.y;

        player.RigidbodyComp.velocity = dashPos.normalized * player.GetPlayerStat.parryDashPower;

        CheckDirectionAndPlayAnimation(Mathf.Atan2(dashPos.y, dashPos.x) * Mathf.Rad2Deg);
        player.PlayerFaceRight = dashPos.x > 0 ? true : false;

        while (true)
        {
            elapsedTime += Time.deltaTime;
            if (player.StopDash || elapsedTime > player.GetPlayerStat.parryDashTime)
            {
                player.StopDash = false;
                TogglePhysics(true);
                break;
            }
            yield return null;
        }

        if (null != player.ParryDashCollision)
        {
            float horDistance = player.transform.position.x - player.ParryDashCollision.collider.bounds.center.x;
            if (horDistance < 0)
            {
                player.transform.position = new Vector3(player.ParryDashCollision.collider.bounds.min.x - player.GetPlayerStat.parryDashDistance,
                    player.transform.position.y, player.transform.position.z);
            }
            else
            {
                player.transform.position = new Vector3(player.ParryDashCollision.collider.bounds.max.x + player.GetPlayerStat.parryDashDistance,
                    player.transform.position.y, player.transform.position.z);
            }
            player.ParryDashCollision = null;
        }
        player.AnimationComp.AnimationState.SetEmptyAnimation(dashAnimationLayer, 0);
        player.ColliderComp.forceReceiveLayers |= LayerMask.GetMask(PlayManager.ENEMY_TAG);
        player.ColliderComp.forceSendLayers |= LayerMask.GetMask(PlayManager.ENEMY_TAG);

        player.ControlParticles(EPlayerState.DASH, false);
        player.IsParryDash = false;

        player.CanChangeState = true;
        player.ChangeState(EPlayerState.IDLE);
        yield return Yields.WaitSeconds(player.GetPlayerStat.dashAfterDelay);
        canParryDash = true;
        canDashAfterParry = true;
    }

    private void TogglePhysics(bool onoff)
    {
        if (onoff)
        {
            player.RigidbodyComp.velocity = Vector2.zero;
            player.RigidbodyComp.gravityScale = originGravityScale;
            player.RigidbodyComp.drag = originLiniearDrag;
            player.RigidbodyComp.mass = originMass;
        }
        else
        {
            player.RigidbodyComp.gravityScale = 0f;
            player.RigidbodyComp.drag = 0;
            player.RigidbodyComp.mass = 0;
        }
    }
    private void CheckDirectionAndPlayAnimation(float angle)
    {
        angle += 180;
        if (angle > 135 && angle <= 225) // right
        {
            player.AnimationComp.AnimationState.SetAnimation(dashAnimationLayer, PlayerAnimationNameCaching.DASH_ANIMATION[0], false).TimeScale = dashAnimationTimeScale;
        }
        else if (angle > 225 && angle <= 315) // up
        {
            player.AnimationComp.AnimationState.SetAnimation(dashAnimationLayer, PlayerAnimationNameCaching.DASH_ANIMATION[1], false).TimeScale = dashAnimationTimeScale;
        }
        else if (angle > 315 && angle <= 360 ||
            angle > 0 && angle <= 45) // left
        {
            player.AnimationComp.AnimationState.SetAnimation(dashAnimationLayer, PlayerAnimationNameCaching.DASH_ANIMATION[0], false).TimeScale = dashAnimationTimeScale;
        }
        else if (angle > 45 && angle <= 135) // down
        {
            player.AnimationComp.AnimationState.SetAnimation(dashAnimationLayer, PlayerAnimationNameCaching.DASH_ANIMATION[2], false).TimeScale = dashAnimationTimeScale;
        }
    }

    public override void OnStateExit()
    {

    }
}
