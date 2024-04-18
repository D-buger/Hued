using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Balances : MonoBehaviour
{
    private readonly float MAX_TORQUE = float.MaxValue;
    private readonly float CHECK_REACTION_FORCE_TIME = 0.05f;

    [SerializeField]
    private float moterSpeed = 1f;

    private SliderJoint2D balanceBottomLeft;
    private SliderJoint2D balanceBottomRight;

    private Rigidbody2D balanceRigidLeft;
    private Rigidbody2D balanceRigidRight;

    private JointMotor2D moter;
    private JointTranslationLimits2D limit;

    private int leftReactionForce = 0;
    private int rightReactionForce = 0;

    private int curDist = 0;
    private int prevDist = 0;
    private float distTime = 0;
    private bool isMove = false;

    private float lowerTranslation;
    private float upperTranslation;
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
        moter.maxMotorTorque = 0;
        moter.motorSpeed = -moterSpeed;
        balanceBottomLeft.motor = moter;
        balanceBottomRight.motor = moter;

        lowerTranslation = balanceBottomLeft.limits.max;
        upperTranslation = balanceBottomRight.limits.min;
        translationForthDist = ((lowerTranslation - upperTranslation) / 4);

        limit.min = balanceBottomLeft.limits.min;
        limit.max = upperTranslation + (translationForthDist * 2);
        balanceBottomLeft.limits = limit;
        limit.min = balanceBottomRight.limits.min;
        balanceBottomRight.limits = limit;
    }


    private void Update()
    {
         if (balanceRigidLeft.velocity.y == 0 || balanceRigidRight.velocity.y == 0)
         {
             JointMoveCheck();
         }
        else
        {
            isMove = false;
        }
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
                Debug.Log(dist);
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
        if (curDist != dist && !isMove)
        {
            if (dist == 0)
            {
                Debug.Log("0 dist");
                isMove = true;
                curDist = dist;

                limit.min = upperTranslation + (translationForthDist * 2);
                limit.max = lowerTranslation;

                balanceBottomLeft.limits = limit;
                balanceBottomRight.limits = limit;
            }
            else if (dist <= 1)
            {
                Debug.Log("1 dist");
                isMove = true;
                curDist = dist;

                if (leftReactionForce < rightReactionForce)
                {
                    limit.min = upperTranslation + translationForthDist;
                    limit.max = lowerTranslation - translationForthDist;

                    balanceBottomLeft.limits = limit;
                    balanceBottomRight.limits = limit;
                }
            }
            else
            {
                Debug.Log("2 dist");
                isMove = true;
                curDist = 2;

                //if (leftReactionForce < rightReactionForce)
                //{
                //    limit.min = upperTranslation;
                //    limit.max = balanceBottomLeft.limits.max;
                //    balanceBottomLeft.limits = limit;

                //    limit.min = balanceBottomRight.limits.min;
                //    limit.max = lowerTranslation;
                //    balanceBottomRight.limits = limit;
                //}
                //else
                //{
                //    limit.min = upperTranslation;
                //    limit.max = balanceBottomRight.limits.max;
                //    balanceBottomRight.limits = limit;

                //    limit.min = balanceBottomLeft.limits.min;
                //    limit.max = lowerTranslation;
                //    balanceBottomLeft.limits = limit;
                //}
            }
        }
    }

    private void JointStop()
    {

    }
}
