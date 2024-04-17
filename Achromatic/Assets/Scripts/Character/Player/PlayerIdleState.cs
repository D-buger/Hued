using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(Player player) : base(player) { }

    public override void OnStateEnter()
    {
        //Debug.Log("Player State : Idle");
    }
    public override void OnStateUpdate()
    {

    }
    public override void OnStateFixedUpdate()
    {

    }

    public override void OnStateExit()
    {

    }

}
