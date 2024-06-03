using Spine.Unity;
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
        InputManager.Instance.MoveEvent.AddListener((int moveDir) =>
        {
            if (moveDir != 0 && !player.IsDash && !player.IsParryDash && player.CanChangeState)
            {
                this.moveDir = moveDir;
                player.ChangeState(EPlayerState.RUN);
            }
            else
            {
                player.ChangeState(EPlayerState.IDLE);
            }
        });
    }

    public override void OnStateEnter()
    {
    }
    public override void OnStateUpdate()
    {
        player.PlayerFaceRight = moveDir > 0 ? true : false;
        horizontalMove = moveDir * player.GetPlayerStat.moveSpeed;
        if(!string.Equals(player.AnimationComp.AnimationName, PlayerAnimationNameCaching.RUN_ANIMATION)
            && player.OnGround)
        {
            player.AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.RUN_ANIMATION, true);
        }
        player.ControlParticles(EPlayerState.RUN, player.OnGround);
    }
    public override void OnStateFixedUpdate()
    {
        player.RigidbodyComp.velocity = new Vector2(horizontalMove, player.RigidbodyComp.velocity.y);
    }
    public override void OnStateExit()
    {
        player.RigidbodyComp.velocity = new Vector2(0, player.RigidbodyComp.velocity.y);

        player.AnimationComp.AnimationState.SetAnimation(0, PlayerAnimationNameCaching.IDLE_ANIMATION, true);

        player.ControlParticles(EPlayerState.RUN, false);
    }
}
