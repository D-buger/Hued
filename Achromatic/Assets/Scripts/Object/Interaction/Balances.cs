using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Balances : MonoBehaviour
{
    private readonly int UP_DIR = -1;
    private readonly int DOWN_DIR = 1;

    private readonly float CHECK_REACTION_FORCE_TIME = 0.05f;

    [SerializeField]
    private float motorSpeed = 1f;

    private SliderJoint2D balanceBottomLeft;
    private SliderJoint2D balanceBottomRight;

    private Rigidbody2D balanceRigidLeft;
    private Rigidbody2D balanceRigidRight;

    private JointMotor2D leftMotor;
    private JointMotor2D rightMotor;

    private float leftReactionForce = 0;
    private float rightReactionForce = 0;

    private int curDist = 0;
    private int prevDist = 0;
    private float distTime = 0;

    private float limitMiddle;
    private float toGetForceValue = 2;
    private void Awake()
    {
        balanceRigidLeft = transform.GetChild(0).GetComponent<Rigidbody2D>();
        balanceBottomLeft = transform.GetChild(0).GetComponent<SliderJoint2D>();
        balanceRigidRight = transform.GetChild(1).GetComponent<Rigidbody2D>();
        balanceBottomRight = transform.GetChild(1).GetComponent<SliderJoint2D>();
    }

    private void Start()
    {
        leftMotor.maxMotorTorque = 0;
        leftMotor.motorSpeed = 0;
        balanceBottomLeft.motor = leftMotor;

        rightMotor.maxMotorTorque = 0;
        rightMotor.motorSpeed = 0;
        balanceBottomRight.motor = rightMotor;

        limitMiddle = (balanceBottomLeft.limits.max - balanceBottomLeft.limits.min) / 2 + balanceBottomLeft.limits.min;
    }


    private void Update()
    {
        JointMoveCheck();
    }
    private void JointMoveCheck()
    {
        leftReactionForce = balanceBottomLeft.reactionForce.y;
        rightReactionForce = balanceBottomRight.reactionForce.y;

        int dist = Mathf.CeilToInt(Mathf.Abs(leftReactionForce - rightReactionForce));
        if (balanceBottomLeft.motor.motorSpeed == 0 && balanceBottomRight.motor.motorSpeed == 0)
        {
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
        else
        {
            bool isCheckMiddle = dist == 0 ? true : false;

            CheckStop(balanceBottomLeft, leftMotor, isCheckMiddle);
            CheckStop(balanceBottomRight, rightMotor, isCheckMiddle);
        }
    }

    private void JointMove(int dist)
    {
        float leftReactionForce = balanceBottomRight.reactionForce.y - balanceBottomLeft.reactionForce.y;
        float rightReactionForce = balanceBottomLeft.reactionForce.y - balanceBottomRight.reactionForce.y;
        //Debug.Log(leftReactionForce + " " + rightReactionForce + " " + dist);

        leftMotor.maxMotorTorque = (Mathf.CeilToInt(Mathf.Abs(leftReactionForce)) * Physics2D.gravity.y * -1) + toGetForceValue;
        rightMotor.maxMotorTorque = (Mathf.CeilToInt(Mathf.Abs(rightReactionForce)) * Physics2D.gravity.y * -1) + toGetForceValue;

        if (leftReactionForce < 0)
        {
            leftMotor.motorSpeed = UP_DIR * motorSpeed;
        }
        else if(leftReactionForce > 0)
        {
            leftMotor.motorSpeed = DOWN_DIR * motorSpeed;
        }
        else 
        {
            if (dist == 0)
            {
                if(balanceBottomLeft.jointTranslation > limitMiddle)
                {
                    leftMotor.motorSpeed = DOWN_DIR * motorSpeed;
                }
                else
                {
                    leftMotor.motorSpeed = DOWN_DIR * motorSpeed;
                }
            }
            else
            {
                leftMotor.motorSpeed = 0;
            }
        }

        if (rightReactionForce < 0)
        {
            rightMotor.motorSpeed = UP_DIR * motorSpeed;
        }
        else if(rightReactionForce > 0)
        {
            rightMotor.motorSpeed = DOWN_DIR * motorSpeed;
        }
        else
        {
            if (dist == 0)
            {

            }
            else
            {
                rightMotor.motorSpeed = 0;
            }
        }

        if(dist == 0)
        {
            CheckStop(balanceBottomLeft, leftMotor, true);
            CheckStop(balanceBottomRight, rightMotor, true);
        }

        balanceBottomLeft.motor = leftMotor;
        balanceBottomRight.motor = rightMotor;
    }

    private void CheckStop(SliderJoint2D joint, JointMotor2D motor, bool isStopMiddle)
    {
        if (isStopMiddle) { 
            if ((joint.motor.motorSpeed < 0 && joint.jointTranslation >= limitMiddle) ||
                joint.motor.motorSpeed > 0 && joint.jointTranslation <= limitMiddle)
            {
                Debug.Log(joint.name + " touch middle");
                motor.motorSpeed = 0;
                joint.motor = motor;
            }
        }

        if (joint.motor.motorSpeed != 0 && 
            (joint.limitState == JointLimitState2D.UpperLimit ||
            joint.limitState == JointLimitState2D.LowerLimit))
        {
            Debug.Log(joint.name + " touch up or down");
            motor.motorSpeed = 0;
            joint.motor = motor;
        }
    }
}
