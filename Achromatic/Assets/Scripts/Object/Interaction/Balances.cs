using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Balances : MonoBehaviour
{
    private readonly float MAX_TORQUE = float.MaxValue;

    [SerializeField]
    private float moterSpeed = 1f;

    private SliderJoint2D balanceBottomLeft;
    private SliderJoint2D balanceBottomRight;

    private JointMotor2D moter;
    private JointTranslationLimits2D limit;

    private int leftReactionForce = 0;
    private int rightReactionForce = 0;

    private int prevDist = 0;
    private bool isMove = false;

    private float lowerTranslation;
    private float upperTranslation;
    private float translationForthDist;
    private void Awake()
    {
        balanceBottomLeft = transform.GetChild(0).GetComponent<SliderJoint2D>();
        balanceBottomRight = transform.GetChild(1).GetComponent<SliderJoint2D>();
    }

    private void Start()
    {
        moter.maxMotorTorque = 0;
        moter.motorSpeed = -moterSpeed;
        balanceBottomLeft.motor = moter;
        balanceBottomRight.motor = moter;

        lowerTranslation = balanceBottomLeft.limits.max;
        upperTranslation = balanceBottomRight.limits.min;
        translationForthDist = ((lowerTranslation - upperTranslation) / 4);

        limit.min = upperTranslation + (translationForthDist * 2);
        limit.max = upperTranslation + (translationForthDist * 2);
        balanceBottomLeft.limits = limit;
        balanceBottomRight.limits = limit;
    }


    private void Update()
    {
        Debug.Log(balanceBottomLeft.transform.position);
        if (balanceBottomLeft.jointSpeed > 0 || balanceBottomRight.jointSpeed > 0) {
            if (!isMove)
            {
                JointMove();
            }
        }
        else
        {
            isMove = false;
            JointStop();
        }
    }

    private void JointMove()
    {
        leftReactionForce = Convert.ToInt32(balanceBottomLeft.reactionForce.y / Physics2D.gravity.y);
        rightReactionForce = Convert.ToInt32(balanceBottomRight.reactionForce.y / Physics2D.gravity.y);

        int dist = Mathf.Abs(leftReactionForce - rightReactionForce);
        if (dist == 0 && prevDist != dist)
        {
            isMove = true;
            prevDist = dist;
            limit.min = upperTranslation + (translationForthDist * 2);
            limit.max = upperTranslation + (translationForthDist * 2);

            balanceBottomLeft.limits = limit;
            balanceBottomRight.limits = limit;
        }
        else if (dist <= 1 && prevDist != dist)
        {
            isMove = true;
            prevDist = dist;
            if (leftReactionForce < rightReactionForce)
            {
                limit.min = upperTranslation + translationForthDist;
                limit.max = balanceBottomLeft.limits.max;
                balanceBottomLeft.limits = limit;

                limit.min = balanceBottomRight.limits.min;
                limit.max = lowerTranslation - translationForthDist;
                balanceBottomRight.limits = limit;
            }
            else
            {
                limit.min = upperTranslation + translationForthDist;
                limit.max = balanceBottomRight.limits.max;
                balanceBottomRight.limits = limit;

                limit.min = balanceBottomLeft.limits.min;
                limit.max = lowerTranslation - translationForthDist;
                balanceBottomLeft.limits = limit;
            }
        }
        else if(prevDist != dist)
        {
            isMove = true;
            prevDist = dist;

        }
    }

    private void JointStop()
    {

    }
}
