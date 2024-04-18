using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using Spine.Unity;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static SpiderEnemy;
using TMPro;
using Cinemachine;
using static UnityEditor.VersionControl.Asset;

public abstract class Monster : MonoBehaviour, IAttack
{

    [SerializeField]
    private MonsterStat stat;
    private MonsterFSM fsm;
    [HideInInspector]
    public int currentHP;

    private Vector2 leftPosition;
    private Vector2 rightPosition;
    private Vector2 thisPosition;
    private Vector2 startSpiderPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    [SerializeField, Tooltip("몬스터 기준 이동 범위")]
    private float runPosition;
    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    private float distanceToPlayer = 0;
    private float angleThreshold = 52f;

    private bool isPlayerBetween = false;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public bool isWait = true;

    private void Start()
    {
        leftPosition.y = transform.position.y;
        rightPosition.y = transform.position.y;
        leftPosition.x += transform.position.x + runPosition;
        rightPosition.x += transform.position.x - runPosition;

        thisPosition = rightPosition;
    }
    private void Update()
    {
        CheckPlayer();
        if (isWait)
        {
            WaitSituation();
        }
    }

    public void CheckPlayer()
    {
        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
        float distanceToMonster = Vector2.Distance(startSpiderPosition, PlayerPos);
        if (distanceToMonster <= runPosition)
        {
            isPlayerBetween = true;
            // isBattle = true; 추적
            isWait = false;
            elapsedTime = 0f;
        }
        else
        {
            isPlayerBetween = false;
        }
    }
    public void WaitSituation()
    {
        currentHP = stat.MonsterHP;
        // isBattle = false; 추적
        transform.position = Vector2.MoveTowards(transform.position, thisPosition, stat.moveSpeed * Time.deltaTime);
        if (thisPosition == leftPosition)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (thisPosition == rightPosition)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (HasArrived((Vector2)transform.position, rightPosition))
        {
            thisPosition = leftPosition;
        }
        if (HasArrived((Vector2)transform.position, leftPosition))
        {
            thisPosition = rightPosition;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }
    public virtual void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {

    }
    public void CheckDead()
    {
        if (currentHP <= 0)
        {
            isDead = true;
        }
    }

    void IAttack.AfterAttack(Vector2 attackDir)
    {
    }
}
