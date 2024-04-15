using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Balances : MonoBehaviour
{
    [SerializeField]
    private float moterSpeed = 1f;

    private SliderJoint2D balanceBottomLeft;
    private SliderJoint2D balanceBottomRight;

    private JointMotor2D moterLeft;
    private JointMotor2D moterRight;

    private void Awake()
    {
        balanceBottomLeft = transform.GetChild(0).GetComponent<SliderJoint2D>();
        balanceBottomRight = transform.GetChild(1).GetComponent<SliderJoint2D>();

        moterLeft.maxMotorTorque = 0;
        moterLeft.motorSpeed = -moterSpeed;

        moterRight.maxMotorTorque = 0;
        moterRight.motorSpeed = -moterSpeed;
    }

    private void Update()
    {
        if(balanceBottomLeft.jointSpeed > 0)
        {
            
        }

        if(balanceBottomRight.jointSpeed > 0)
        {

        }
    }
}
