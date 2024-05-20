using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : MonsterState
{
    public ChaseState(Monster monster) : base(monster)
    {
    }
    public override void Enter()
    {
        monster.SetState(Monster.EMonsterState.isWait, false);
        monster.SetState(Monster.EMonsterState.isBattle, false);
    }

    public override void Execute()
    {
        monster.MoveToPlayer();
    }

    public override void Exit()
    {
    }
}
