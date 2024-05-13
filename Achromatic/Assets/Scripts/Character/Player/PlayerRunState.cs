using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PlayerRunState : PlayerBaseState
{
    private float horizontalMove = 0;
    private float moveDir;
    public PlayerRunState(Player player) : base(player)
    {
        InputManager.Instance.MoveEvent += (int moveDir) =>
        {
            this.moveDir = moveDir;

            if (moveDir != 0 && !player.IsDash && !player.IsParryDash && player.CanChangeState)
            {
                player.ChangeState(ePlayerState.RUN);
            }
            else
            {
                player.ChangeState(ePlayerState.IDLE);
            }
        };
    }

    public override void OnStateEnter()
    {
        //Debug.Log("Player State : Run");

        player.AnimatorComp.SetBool("isRunning", true);
    }
    public override void OnStateUpdate()
    {
        player.PlayerFaceRight = moveDir > 0 ? true : false;
        horizontalMove = moveDir * player.GetPlayerStat.moveSpeed;

        player.ControlParticles(ePlayerState.RUN, player.OnGround);
    }
    public override void OnStateFixedUpdate()
    {
        player.RigidbodyComp.velocity = new Vector2(horizontalMove, player.RigidbodyComp.velocity.y);
    }
    public override void OnStateExit()
    {
        player.RigidbodyComp.velocity = new Vector2(0, player.RigidbodyComp.velocity.y);

        player.AnimatorComp.SetBool("isRunning", false);

        player.ControlParticles(ePlayerState.RUN, false);
    }
}
