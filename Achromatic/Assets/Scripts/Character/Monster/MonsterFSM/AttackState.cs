using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : MonsterState
{
    public AttackState(Monster monster) : base(monster)
    {
    }
    public override void Enter()
    {
    }

    public override void Execute()
    {
        monster.Attack();
    }

    public override void Exit()
    {
    }
}
