using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : SingletonBehavior<InputManager>
{
    const KeyCode JUMP = KeyCode.Space;
    const KeyCode LEFT = KeyCode.A;
    const KeyCode RIGHT = KeyCode.D;
    const KeyCode LOOK_UP = KeyCode.W;
    const KeyCode LOOK_DOWN = KeyCode.S;
    const KeyCode DASH = KeyCode.Mouse1;
    const KeyCode LIGHT_ATTACK = KeyCode.Mouse0;
    const KeyCode FILTER = KeyCode.F;
    const KeyCode INTERECTION = KeyCode.E;

    public UnityEvent JumpEvent;
    public UnityEvent<int> MoveEvent;
    public UnityEvent<int> LookEvent;
    public UnityEvent<Vector2> DashEvent;
    public UnityEvent<Vector2> LightAttackEvent;
    public UnityEvent FilterEvent;
    public UnityEvent InterectionEvent;

    private Camera mainCamera;
    public Vector2 MouseVec { get; private set; }
    public int ArrowVec { get; private set; }

    public bool CanInput { get; set; } = true;
    protected override void OnAwake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        MouseVec = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (!CanInput)
        {
            return;
        }

        if (Input.GetKey(LEFT))
        {
            MoveEvent?.Invoke(-1);
            ArrowVec = -1;
        }
        else if (Input.GetKey(RIGHT))
        {
            MoveEvent?.Invoke(1);
            ArrowVec = 1;
        }
        else
        {
            MoveEvent?.Invoke(0);
            ArrowVec = 0;
        }

        if (Input.GetKey(LIGHT_ATTACK))
        {
            LightAttackEvent?.Invoke(MouseVec);
        }

        if (Input.GetKey(JUMP))
        {
            JumpEvent?.Invoke();
        }
        if(Input.GetKey(DASH))
        {
            DashEvent?.Invoke(MouseVec);
        }
        if (Input.GetKey(FILTER))
        {
            FilterEvent?.Invoke();
        }

        if (Input.GetKey(LOOK_DOWN))
        {
            LookEvent?.Invoke(-1);
        }
        if (Input.GetKey(LOOK_UP))
        {
            LookEvent?.Invoke(1);
        }

        if (Input.GetKeyDown(INTERECTION))
        {
            InterectionEvent?.Invoke();
        }
    }
}
