using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="PlayerStatus", menuName ="Scriptable Object/PlayerStatus")]
public class PlayerStatus : ScriptableObject
{
    [HideInInspector]
    public int maxHP;
    [HideInInspector]
    public int currentHP;

    public int playerHP = 5;

    [Space(10), Header("Move")]
    public float moveSpeed = 5;

    [Space(10), Header("Jump")]
    public float jumpPower = 30;
    public float jumpCooldown = 0.8f;
    public float koyoteTime = 0.5f;
    public float airHangTime = 0.5f;

    [Space(10), Header("Dash")]
    public float dashPower = 20;
    public float dashingTime = 0.1f;
    public int dashDamage = 3;
    public float dashCooldown = 1;
    public float dashAfterDelay = 0.2f;
    [Space(10)]
    public float parryDashPower = 40;
    public float parryDashTime = 0.1f;
    public int parryDashDamage = 3;
    [Space(10)]
    public float parryProduceTime = 1f;
    public float parryProduceTimescale = 0.5f;
    public float invincibilityAfterParry = 0.2f;
    public float parryDashDistance = 5;

    [Space(10), Header("Attack")]
    public float attackReboundPower = 30;
    public float attackReboundTime = 0.05f;
    [Space(5)]
    public float attackTime = 0.1f;
    public float attackCooldown = 0.3f;
    [Space(5)]
    public int attackDamage = 1;
    public int criticalAttackDamage = 2;
    public int colorAttackDamage = 3;
    public int colorCriticalAttackDamage = 4;

    [Space(10), Header("Hit")]
    public float hitReboundPower = 30;
    public float hitReboundTime = 0.05f;
    public float hitBehaviourLimitTime = 1;
    public float hitInvincibilityTime = 0.5f;
}
