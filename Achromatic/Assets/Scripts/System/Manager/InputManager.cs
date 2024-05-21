using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : SingletonBehavior<InputManager>
{
    const KeyCode EXIT = KeyCode.Escape;
    const KeyCode JUMP = KeyCode.Space;
    const KeyCode LEFT = KeyCode.A;
    const KeyCode RIGHT = KeyCode.D;
    const KeyCode LOOK_UP = KeyCode.W;
    const KeyCode LOOK_DOWN = KeyCode.S;
    const KeyCode DASH = KeyCode.Mouse1;
    const KeyCode LIGHT_ATTACK = KeyCode.Mouse0;
    const KeyCode FILTER = KeyCode.F;
    const KeyCode INVENTORY = KeyCode.I;

    [HideInInspector]
    public UnityEvent ExitEvent;
    [HideInInspector]
    public UnityEvent JumpEvent;
    [HideInInspector]
    public UnityEvent<int> MoveEvent;
    [HideInInspector]
    public UnityEvent<int> LookEvent;
    [HideInInspector]
    public UnityEvent<Vector2> DashEvent;
    [HideInInspector]
    public UnityEvent<Vector2> LightAttackEvent;
    [HideInInspector]
    public UnityEvent FilterEvent;
    [HideInInspector]
    public UnityEvent InventoryEvent;

    [SerializeField]
    private float jumpBufferTime = 0.1f;

    public Vector2 MouseVec { get; private set; }
    public int ArrowVec { get; private set; }

    public bool CanInput { get; set; } = true;

    private Camera mainCamera;
    private float prevGetJumpTime = 0f;

    protected override void OnAwake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        MouseVec = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        prevGetJumpTime += Time.deltaTime;

        if (Input.GetKey(EXIT))
        {
            ExitEvent?.Invoke();
        }

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
            prevGetJumpTime = 0;
            JumpEvent?.Invoke();
        }
        else if(prevGetJumpTime < jumpBufferTime)
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

        if (Input.GetKey(INVENTORY))
        {
            InventoryEvent?.Invoke();
        }

        if (Input.GetKey(LOOK_DOWN))
        {
            LookEvent?.Invoke(-1);
        }
        else if (Input.GetKey(LOOK_UP))
        {
            LookEvent?.Invoke(1);
        }
    }
}
