using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    private float horizontalMove = 0;
    private float moveDir;
    public PlayerRunState(Player player) : base(player)
    {
        InputManager.Instance.MoveEvent.AddListener((float moveDir) =>
        {
            player.ChangeState(ePlayerState.RUN);
            this.moveDir = moveDir;
        });
    }

    public override void OnStateEnter()
    {

    }
    public override void OnStateUpdate()
    {
        player.AnimatorComp.SetBool("isRunning", moveDir != 0 ? true : false);

        if (moveDir != 0)
        {
            playerFaceRight = moveDir > 0 ? true : false;
        }
        horizontalMove = moveDir * player.GetPlayerStat().moveSpeed;


        if (!player.OnGround || moveDir == 0)
        {
            player.ControlParticles(ePlayerState.RUN, false);

        }
        else
        {
            player.ControlParticles(ePlayerState.RUN, true);
        }
    }
    public override void OnStateExit()
    {

    }
}
