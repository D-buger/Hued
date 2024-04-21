using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class MonsterFSM : MonoBehaviour
{
    private Dictionary<string, MonsterState> states = new Dictionary<string, MonsterState>();
    private MonsterState currentState;
    private Monster monster;

    private void Awake()
    {
        monster = GetComponent<Monster>();
        states.Add("Idle", new IdleState(monster));
        states.Add("Chase", new ChaseState(monster));
        states.Add("Attack", new AttackState(monster));
    }
    private void Update()
    {
        if (currentState != null)
        {
            currentState.Execute();
        }
    }

    public void ChangeState(string newStateName)
    {
        if (states.ContainsKey(newStateName))
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            currentState = states[newStateName];
            currentState.Enter();
        }
        else
        {
            Debug.LogError("State '" + newStateName + "' not found!");
        }
    }
}