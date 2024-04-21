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
        Debug.Log("대기 시퀀스 진입");
    }

    public override void Execute()
    {
        monster.WaitSituation();
    }

    public override void Exit()
    {
    }
}