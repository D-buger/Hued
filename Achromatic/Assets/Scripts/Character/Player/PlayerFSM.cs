using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFSM
{

    private PlayerBaseState curState;
    public PlayerFSM(PlayerBaseState initState)
    {
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

        curState = nextState;
        curState.OnStateEnter();
    }

    public void UpdateState()
    {
        if(curState != null)
        {
            curState.OnStateUpdate();
        }
    }
}
