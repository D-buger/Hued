using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private GameObject attackPoint;
    private Attack meleeAttack;
    public PlayerAttackState(Player player, GameObject atkPnt) : base(player) 
    {
        attackPoint = atkPnt;
        meleeAttack = attackPoint.transform.GetComponentInChildren<Attack>();

        meleeAttack.SetAttack(PlayManager.PLAYER_TAG, player);
    }

    public override void OnStateEnter()
    {

    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {

    }

}
