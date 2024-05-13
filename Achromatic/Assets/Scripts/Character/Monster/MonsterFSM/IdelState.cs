using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonsterState
{
    public IdleState(Monster monster) : base(monster)
    {
    }
    public override void Enter()
    {
    }

    public override void Execute()
    {
        monster.WaitSituation();
    }

    public override void Exit()
    {
    }
}