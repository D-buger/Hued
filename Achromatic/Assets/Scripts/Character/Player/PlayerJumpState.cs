using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private Coroutine jumpCoroutine;

    private bool canJump = true;
    public PlayerJumpState(Player player) : base(player) 
    {
        InputManager.Instance.JumpEvent.AddListener(() =>
        {
            if (player.CanChangeState && canJump && 
            (player.OnGround || (player.FootOffGroundTime < player.GetPlayerStat.koyoteTime && player.FootOffGroundTime > 0)))
            {
                player.ChangeState(EPlayerState.JUMP);
            }
            
        });
    }

    public override void OnStateEnter()
    {
        player.FootOffGroundTime = -1;
        jumpCoroutine = CoroutineHandler.StartCoroutine(JumpSequence());
        
        player.ChangePrevState();
    }

    public override void OnStateUpdate()
    {
    }
    IEnumerator JumpSequence()
    {
        canJump = false;
        float elapsedTime = 0;
        float airHangedTime = -1;
        float oriGravityValue = player.RigidbodyComp.gravityScale;
        bool passedAirHangTime = false;
        player.AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.JUMP_ANIMATION[0], false);
        player.RigidbodyComp.velocity = Vector2.zero;
        player.RigidbodyComp.velocity = Vector2.up * player.GetPlayerStat.jumpPower;
        while (true)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > player.GetPlayerStat.jumpCooldown)
            {
                canJump = true;
                if (player.OnGround)
                {
                    player.RigidbodyComp.gravityScale = oriGravityValue;
                    break;
                }
            }

            if (!passedAirHangTime)
            {
                player.AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.JUMP_ANIMATION[1], false);
                if (player.RigidbodyComp.velocity.y <= 0 && airHangedTime < 0)
                {
                    airHangedTime = elapsedTime;
                    player.RigidbodyComp.gravityScale *= 0.5f;
                }
                if (elapsedTime - airHangedTime > player.GetPlayerStat.airHangTime && airHangedTime > 0)
                {
                    passedAirHangTime = true;
                    player.RigidbodyComp.gravityScale = oriGravityValue;
                }
            }

            yield return null;
        }
    }
    public override void OnStateFixedUpdate()
    {

    }
    public override void OnStateExit()
    {

    }

}
