using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Balances : MonoBehaviour
{
    private readonly int UP_DIR = -1;
    private readonly int DOWN_DIR = 1;
    private readonly float POSITION_RANGE = 0.1f;

    private readonly float CHECK_REACTION_FORCE_TIME = 0.05f;

    [SerializeField]
    private float motorSpeed = 1f;

    private SliderJoint2D balanceBottomLeft;
    private SliderJoint2D balanceBottomRight;

    private Rigidbody2D balanceRigidLeft;
    private Rigidbody2D balanceRigidRight;

    private JointMotor2D motor;

    private int leftReactionForce = 0;
    private int rightReactionForce = 0;

    private int curDist = 0;
    private int prevDist = 0;
    private float distTime = 0;
    private int toGetForceValue = 1;

    private bool isMove = false;

    private float translationForthDist;
    private void Awake()
    {
        balanceRigidLeft = transform.GetChild(0).GetComponent<Rigidbody2D>();
        balanceBottomLeft = transform.GetChild(0).GetComponent<SliderJoint2D>();
        balanceRigidRight = transform.GetChild(1).GetComponent<Rigidbody2D>();
        balanceBottomRight = transform.GetChild(1).GetComponent<SliderJoint2D>();
    }

    private void Start()
    {
        motor.maxMotorTorque = 0;
        motor.motorSpeed = 0;
        balanceBottomLeft.motor = motor;
        balanceBottomRight.motor = motor;

        translationForthDist = ((balanceBottomLeft.limits.max - balanceBottomLeft.limits.min) / 4);
    }


    private void Update()
    {
        if (balanceRigidLeft.velocity.y < 0)
        {
            motor.motorSpeed = UP_DIR * motorSpeed;
            balanceBottomRight.motor = motor;
        }
        else if (balanceRigidLeft.velocity.y > 0)
        {
            motor.motorSpeed = DOWN_DIR * motorSpeed;
            balanceBottomRight.motor = motor;
        }

        if (balanceRigidRight.velocity.y < 0)
        {
            motor.motorSpeed = UP_DIR * motorSpeed;
            balanceBottomLeft.motor = motor;
        }
        else if (balanceRigidRight.velocity.y > 0)
        {
            motor.motorSpeed = DOWN_DIR * motorSpeed;
            balanceBottomLeft.motor = motor;
        }


        JointMoveCheck();
    }
    private void JointMoveCheck()
    {
        leftReactionForce = Convert.ToInt32(balanceBottomLeft.reactionForce.y / Physics2D.gravity.y);
        rightReactionForce = Convert.ToInt32(balanceBottomRight.reactionForce.y / Physics2D.gravity.y);

        int dist = Mathf.Abs(leftReactionForce - rightReactionForce);
        if (prevDist == dist)
        {
            distTime += Time.deltaTime;
            if (distTime > CHECK_REACTION_FORCE_TIME)
            {
                JointMove(dist);
            }
        }
        else
        {
            distTime = 0;
        }
        prevDist = dist;
    }

    private void JointMove(int dist)
    {
            if (dist == 0)
            {
                curDist = dist;

                motor.maxMotorTorque = 0;
                balanceBottomLeft.motor = motor;
                balanceBottomRight.motor = motor;
            }
            else
            {
                curDist = dist;

                if (leftReactionForce < rightReactionForce)
                {
                    motor.maxMotorTorque = (rightReactionForce * Physics2D.gravity.y * -1) + toGetForceValue;

                    //motor.motorSpeed = UP_DIR * motorSpeed;
                    //balanceBottomLeft.motor = motor;
                    //motor.motorSpeed = DOWN_DIR * motorSpeed;
                    //balanceBottomRight.motor = motor;
                }
                else
                {
                    motor.maxMotorTorque = (leftReactionForce * Physics2D.gravity.y * -1) + toGetForceValue;

                    //motor.motorSpeed = DOWN_DIR * motorSpeed;
                    //balanceBottomLeft.motor = motor;
                    //motor.motorSpeed = UP_DIR * motorSpeed;
                    //balanceBottomRight.motor = motor;
                }
            }
    }

    private void JointStop()
    {

    }
}
