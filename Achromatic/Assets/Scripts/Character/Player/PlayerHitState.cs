using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private Coroutine hitCoroutine;

    private Color originalRendererColor;
    private Color hitChangeColor = Color.black;

    private float hitAnimationTimescale = 0.5f;
    public PlayerHitState(Player player) : base(player)
    {
        //originalRendererColor = player.RendererComp.material.color;
    }

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
            player.CurrentHP -= damage;
            hitCoroutine = CoroutineHandler.StartCoroutine(HitReboundSequence(attackDir.normalized, player.GetPlayerStat.hitReboundPower, player.GetPlayerStat.hitReboundTime));
        }
    }

    IEnumerator HitReboundSequence(Vector2 dir, float reboundPower, float reboundTime)
    {
        player.CanChangeState = false;
        player.IsInvincibility = true;

        player.AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.HIT_ANIMATIONS[Random.Range(0, PlayerAnimationNameCaching.HIT_ANIMATIONS.Length)], false).TimeScale = hitAnimationTimescale;
        player.ControlParticles(EPlayerState.HIT, true);
        //player.RendererComp.material.color = hitChangeColor;

        player.RigidbodyComp.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(reboundTime);
        float elapsedTime = 0f;
        while (true)
        {
            if (ReferenceEquals(player.RendererComp, null) || elapsedTime > reboundTime)
            {
                break;
            }
            else
            {
                elapsedTime += Time.deltaTime;
                //player.RendererComp.material.color = Vector4.Lerp(hitChangeColor, originalRendererColor, elapsedTime / reboundTime);

                yield return null;
            }
        }

        yield return Yields.WaitSeconds(player.GetPlayerStat.hitBehaviourLimitTime);
        if (!ReferenceEquals(player.RendererComp, null))
        {
            player.ControlParticles(EPlayerState.HIT, false);
            //player.RendererComp.material.color = originalRendererColor;
        }

        player.CanChangeState = true;
        player.ChangeState(EPlayerState.IDLE);

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
