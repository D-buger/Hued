using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="PlayerStat", menuName ="Scriptable Object/PlayerStat")]
public class PlayerStat : ScriptableObject
{
    [SerializeField]
    private int playerHP = 5;
    public int PlayerHP
    {
        get
        {
            return playerHP;
        }
        set
        {
            if (!isInvincibility)
            {
                playerHP = value;
            }
        }
    }

    [Space(10), SerializeField, Header("Move")]
    public float moveSpeed = 5;

    [Space(10), SerializeField, Header("Jump")]
    public float jumpPower = 30;
    [SerializeField]
    public float jumpCooldown = 0.8f;

    [Space(10), SerializeField, Header("Sit")]
    public float sitDeceleration = 0.5f;
    [SerializeField]
    public float sitDescentSpeed = 30f;

    [Space(10), SerializeField, Header("Dash")]
    public float dashPower = 20;
    [SerializeField]
    public float dashingTime = 0.1f;
    [SerializeField]
    public float invincibilityTimeafterDash = 0.2f;
    [SerializeField]
    public float dashDamage = 3;
    [SerializeField]
    public float dashCooldown = 1;

    [Space(10), SerializeField, Header("Attack")]
    public float attackReboundPower = 30;
    [SerializeField]
    public float attackReboundTime = 0.05f;
    [Space(5), SerializeField]
    public float lightAttackTime = 0.1f;
    [SerializeField]
    public float lightAttackCooldown = 0.3f;
    [SerializeField]
    public int lightAttackDamage = 1;
    [Space(5), SerializeField]
    public float heavyAttackTime = 0.1f;
    [SerializeField]
    public float heavyAttackCooldown = 0.5f;
    [SerializeField]
    public int heavyAttackDamage = 4;

    [Space(10), SerializeField, Header("Other")]

    [HideInInspector]
    public bool isSit = false;
    [HideInInspector]
    public bool isJump = false;
    [HideInInspector]
    public bool canJump = true;
    [HideInInspector]
    public bool isAttack = false;
    [HideInInspector]
    public bool canAttack = true;
    [HideInInspector]
    public bool isAttackRebound = false;
    [HideInInspector]
    public bool isDash = false;
    [HideInInspector]
    public bool canDash = true;
    [HideInInspector]
    public bool isInvincibility = false;

    [HideInInspector]
    public float rayRange = 0.1f;
    [HideInInspector]
    public float horizontalMove = 0;
    [HideInInspector]
    public bool playerFaceRight = true;
}
