using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : MonsterState
{
    public override void Enter()
    {
        Debug.Log("Entering Attack State");
    }

    public override void Execute()
    {
        // �÷��̾ �����ϴ� ����
    }

    public override void Exit()
    {
        Debug.Log("Exiting Attack State");
    }
}
