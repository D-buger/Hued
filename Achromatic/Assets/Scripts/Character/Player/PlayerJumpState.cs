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
            if (player.CanChangeState)
            {
                player.ChangeState(ePlayerState.JUMP);
            }
            
        });
    }

    public override void OnStateEnter()
    {
        if (canJump && player.OnGround)
        {
            jumpCoroutine = CoroutineHandler.StartCoroutine(JumpSequence());
        }
        player.ChangePrevState();
    }

    public override void OnStateUpdate()
    {
    }
    IEnumerator JumpSequence()
    {
        float elapsedTime = 0;
        float airHangedTime = -1;
        float oriGravityValue = player.RigidbodyComp.gravityScale;
        bool passedAirHangTime = false;
        canJump = false;
        player.AnimatorComp.SetTrigger("jumpTrigger");
        player.RigidbodyComp.AddForce(Vector2.up * player.GetPlayerStat.jumpPower, ForceMode2D.Impulse);
        while (true)
        {
            elapsedTime += Time.deltaTime;

            if (!passedAirHangTime)
            {
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

            if (elapsedTime > player.GetPlayerStat.jumpCooldown)
            {
                canJump = true;
                if (player.OnGround)
                {
                    player.RigidbodyComp.gravityScale = oriGravityValue;
                    break;
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
