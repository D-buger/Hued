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
            player.ChangeState(ePlayerState.JUMP);
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
        canJump = false;
        player.AnimatorComp.SetTrigger("jumpTrigger");
        player.RigidbodyComp.AddForce(Vector2.up * player.GetPlayerStat.jumpPower, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(player.GetPlayerStat.jumpCooldown);
        canJump = true;
    }
    public override void OnStateFixedUpdate()
    {

    }
    public override void OnStateExit()
    {

    }

}
