using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected Player player;

    protected PlayerBaseState(Player player)
    {
        this.player = player;
    }

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public virtual void OnStateFixedUpdate() { }
    public abstract void OnStateExit();
}
