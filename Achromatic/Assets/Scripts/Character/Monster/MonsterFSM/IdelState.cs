using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MonsterState
{
    private Monster monster;
    public override void Enter()
    {
        monster.isWait = true;
    }

    public override void Execute()
    {
    }

    public override void Exit()
    {
        monster.isWait = false;
    }
}
