using System.Collections;
using System.Collections.Generic;
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
    public PlayerDashState(Player player) : base(player) 
    {
        InputManager.Instance.DashEvent.AddListener((Vector2 dir) =>
        {
            if ( player.CanChangeState && (canDash || canParryDash))
            {
                dashDirection = dir;

                player.ChangeState(ePlayerState.DASH);
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
            player.ChangeState(ePlayerState.IDLE);
        }
    }

    public override void OnStateUpdate()
    {

    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        player.CanChangeState = false;
        player.IsDash = true;
        canDash = false;
        canParryDash = false;

        float originGravityScale = player.RigidbodyComp.gravityScale;
        float originLiniearDrag = player.RigidbodyComp.drag;
        float originMass = player.RigidbodyComp.mass;
        player.RigidbodyComp.gravityScale = 0f;
        player.RigidbodyComp.drag = 0;
        player.RigidbodyComp.mass = 0;
        player.ControlParticles(ePlayerState.DASH, true);

        dashPos.x = dashPos.x - player.transform.position.x;
        dashPos.y = dashPos.y - player.transform.position.y;

        player.RigidbodyComp.velocity = dashPos.normalized * player.GetPlayerStat.dashPower;

        player.AnimatorComp.SetTrigger("dashTrigger");
        player.PlayerFaceRight = dashPos.x > 0 ? true : false;

        yield return Yields.WaitSeconds(player.GetPlayerStat.dashingTime);
        player.RigidbodyComp.velocity = Vector2.zero;
        player.RigidbodyComp.gravityScale = originGravityScale;
        player.RigidbodyComp.drag = originLiniearDrag;
        player.RigidbodyComp.mass = originMass;

        player.ControlParticles(ePlayerState.DASH, false);
        player.AnimatorComp.SetTrigger("dashEndTrigger");
        isParry = player.ParryCondition;
        if (isParry)
        {
            parryCoroutine = CoroutineHandler.StartCoroutine(ParrySequence());
        }
        player.ParryCondition = false;
        player.IsDash = false;

        player.CanChangeState = true;
        player.ChangeState(ePlayerState.IDLE);
        yield return Yields.WaitSeconds(player.GetPlayerStat.dashAfterDelay);
        canParryDash = true;

        yield return Yields.WaitSeconds(player.GetPlayerStat.dashCooldown - player.GetPlayerStat.dashAfterDelay);
        canDash = true;
    }
    IEnumerator ParrySequence()
    {
        player.IsInvincibility = true;

        Time.timeScale = player.GetPlayerStat.parryProduceTimescale;
        player.AnimatorComp.SetTrigger("parryTrigger");
        player.ControlParticles(ePlayerState.DASH ,true, 1);
        yield return Yields.WaitSeconds(player.GetPlayerStat.parryProduceTime);
        Time.timeScale = 1f;
        player.ControlParticles(ePlayerState.DASH, false, 1);

        yield return Yields.WaitSeconds(player.GetPlayerStat.invincibilityAfterParry);
        player.IsInvincibility = false;
    }


    IEnumerator ParryDashSequence(Vector2 dashPos)
    {
        player.CanChangeState = false;
        isParry = false;
        player.IsParryDash = true;
        canParryDash = false;
        canDashAfterParry = false;
        player.IsCriticalAttack = true;

        float originGravityScale = player.RigidbodyComp.gravityScale;
        float originLiniearDrag = player.RigidbodyComp.drag;
        float originMass = player.RigidbodyComp.mass;
        player.ColliderComp.forceReceiveLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        player.ColliderComp.forceSendLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        player.RigidbodyComp.gravityScale = 0f;
        player.RigidbodyComp.drag = 0;
        player.RigidbodyComp.mass = 0;
        player.ControlParticles(ePlayerState.DASH, true);

        dashPos.x = dashPos.x - player.transform.position.x;
        dashPos.y = dashPos.y - player.transform.position.y;

        player.RigidbodyComp.velocity = dashPos.normalized * player.GetPlayerStat.parryDashPower;

        player.AnimatorComp.SetTrigger("dashTrigger");
        player.PlayerFaceRight = dashPos.x > 0 ? true : false;

        yield return Yields.WaitSeconds(player.GetPlayerStat.parryDashTime);
        player.RigidbodyComp.velocity = Vector2.zero;
        player.RigidbodyComp.gravityScale = originGravityScale;
        player.RigidbodyComp.drag = originLiniearDrag;
        player.RigidbodyComp.mass = originMass;

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
        player.ColliderComp.forceReceiveLayers |= (1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        player.ColliderComp.forceSendLayers |= (1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));

        player.ControlParticles(ePlayerState.DASH, false);
        player.IsParryDash = false;

        player.CanChangeState = true;
        player.ChangeState(ePlayerState.IDLE);
        yield return Yields.WaitSeconds(player.GetPlayerStat.dashAfterDelay);
        canParryDash = true;
        canDashAfterParry = true;
    }


    public override void OnStateExit()
    {

    }
}
