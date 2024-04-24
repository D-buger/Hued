using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFSM
{
    private PlayerBaseState prevState;
    private PlayerBaseState curState;
    public PlayerFSM(PlayerBaseState initState)
    {
        prevState = initState;
        curState = initState;
        ChangeState(curState);
    }

    public void ChangeState(PlayerBaseState nextState)
    {
        if(nextState == curState)
        {
            return;
        }

        if(curState != null)
        {
            curState.OnStateExit();
        }

        prevState = curState;
        curState = nextState;
        curState.OnStateEnter();
    }
    public void ChangePrevState()
    {
        ChangeState(prevState);
    }
    public void UpdateState()
    {
        if(curState != null)
        {
            curState.OnStateUpdate();
        }
    }
    public void FixedUpdateState()
    {
        if (curState != null)
        {
            curState.OnStateFixedUpdate();
        }
    }
}
