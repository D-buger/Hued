using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : SingletonBehavior<InputManager>
{
    const KeyCode JUMP = KeyCode.W;
    const KeyCode LEFT = KeyCode.A;
    const KeyCode RIGHT = KeyCode.D;
    const KeyCode SIT = KeyCode.S;

    public UnityEvent JumpEvent;
    public UnityEvent<float> MoveEvent;
    public UnityEvent SitEvent;

    protected override void OnAwake()
    {

    }

    void Update()
    {
        if (Input.GetKey(JUMP))
        {
            JumpEvent?.Invoke();
        }
        if (Input.GetKey(LEFT))
        {
            MoveEvent?.Invoke(-1);
        }
        if (Input.GetKey(RIGHT))
        {
            MoveEvent?.Invoke(1);
        }
        if (Input.GetKey(SIT))
        {
            SitEvent?.Invoke();
        }
    }
}
