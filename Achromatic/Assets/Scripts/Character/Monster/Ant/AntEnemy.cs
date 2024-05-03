using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static SpiderEnemy;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.Rendering.DebugUI;

public class AntEnemy : Monster, IAttack
{
    private MonsterFSM fsm;

    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private GameObject attackTransform;
    [SerializeField]
    private AntMonsterStat stat;
    [SerializeField]
    private GameObject[] stabAttackOBJ;
  
    public UnityEvent<eActivableColor> antColorEvent;

    private float angleThreshold = 52f;

    private enum EMonsterAttackState
    {
        None = 0,
        IsAttack = 1 << 0,
        isStabAttack = 1 << 1,
        isCounter = 1 << 2
    }
    private EMonsterAttackState currentState = EMonsterAttackState.None;

    private bool isHeavy = false;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private void Start()
    {
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);
    }

    private void CheckIsHeavy(eActivableColor color)
    {
        if (color == stat.enemyColor)
        {
            isHeavy = false;
        }
        antColorEvent?.Invoke(color);
    }
    public override void Attack()
    {
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle)
        {
            SetState(EMonsterState.isBattle, false);
            SetState(EMonsterState.isPlayerBetween, true);
            SetState(EMonsterState.isWait, false);
        }
        else if (canAttack && !currentState.HasFlag(EMonsterAttackState.IsAttack))
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        currentState |= EMonsterAttackState.IsAttack;
        canAttack = false;
        float horizontalValue = attackAngle.x - transform.position.x;
        float verticalValue = attackAngle.y - transform.position.y;
        float ZAngle = (Mathf.Atan2(verticalValue, horizontalValue) * Mathf.Rad2Deg);
        Vector2 value = new Vector2(horizontalValue, verticalValue);
        Vector2 check;
        if (value.x <= 0)
        {
            transform.localScale = new Vector2(1, 1);
            check = new Vector2(-1f, 0);
        }
        else
        {
            transform.localScale = new Vector2(-1, 1);
            check = new Vector2(1f, 0);
        }
        // TODO 공격 전조 애니메이션
        yield return Yields.WaitSeconds(0.0f); // TODO 공격 실행 이전 대기 시간

        // TODO 공격 패턴 구현

        int checkRandomAttackType = UnityEngine.Random.Range(1, 100);
        if (checkRandomAttackType <= stat.swordAttackPercent)
        {
            StartCoroutine(SwordAttack(value, check, ZAngle));
        }
        else if (checkRandomAttackType >= stat.stabAttackPercent && stat.stabAttackPercent + stat.swordAttackPercent >= checkRandomAttackType)
        {
            currentState |= EMonsterAttackState.isStabAttack;
            StabAttack(check);
        }    
        else
        {
            StartCoroutine(CounterAttackStart());
        }

        yield return Yields.WaitSeconds(stat.attackTime);
        currentState &= ~EMonsterAttackState.IsAttack;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    private IEnumerator SwordAttack(Vector2 dir, Vector2 check, float ZAngle)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield return null;
        }
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, ZAngle);
        meleeAttack?.AttackAble(-dir, stat.attackDamage);
        rigid.AddForce(check * stat.swordAttackRebound, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(0.0f); // FIX 근접 공격 애니메이션 JSON 파싱
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(1);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, new Vector2(dir.x, dir.y).normalized,
                    stat.swordAttackRange, stat.swordAttackSpeed, stat.swordAttackDamage, isHeavy, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.swordAttackRange);
            }
        }
    }

    private IEnumerator StabAttack(Vector2 check)
    {
        if (IsStateActive(EMonsterState.isWait))
        {
            yield break;
        }
        float delayToAttack = 0.2f;
        float delayToDestory = 0.05f;
        int objectCount = stabAttackOBJ.Length/2;
        while (currentState.HasFlag(EMonsterAttackState.isStabAttack))
        {
            currentState &= ~EMonsterAttackState.isStabAttack;
            int satbValue = (check.x > 0) ? satbValue = 3 : satbValue = 0;
            objectCount += satbValue;
            for (int i = satbValue; i < objectCount; i += 1)
            {
                if (i % 3 == 0)
                {
                    ActivateObjects(stabAttackOBJ, i, i + 1, true, true);
                    yield return new WaitForSeconds(delayToAttack);
                    ActivateObjects(stabAttackOBJ, i, i + 1, false, true);
                    yield return new WaitForSeconds(delayToDestory);
                }
                else
                {
                    ActivateObjects(stabAttackOBJ, i, i + 1, true, false);
                    yield return new WaitForSeconds(delayToAttack);
                    ActivateObjects(stabAttackOBJ, i, i + 1, false, false);
                    yield return new WaitForSeconds(delayToDestory);
                }
            }
        }
    }
    private IEnumerator ActivateObjects(GameObject[] objects, int startIndex, int endIndex, bool isSet, bool lastAttack)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            if (objects[i] = null)
            {
                if (!lastAttack)
                {
                    yield return Yields.WaitSeconds(stat.stabAttackDelay);
                    objects[i].SetActive(isSet);
                }
                else
                {
                    yield return Yields.WaitSeconds(stat.middleStabAttackDelay);
                    objects[i].SetActive(isSet);
                }
            }
        }
    }
    private IEnumerator CounterAttackStart()
    {
        currentState |= EMonsterAttackState.isCounter;
        yield return Yields.WaitSeconds(stat.counterAttackTime);
        currentState &= ~EMonsterAttackState.isCounter;
    }
    private void CounterAttackPlay(Vector2 dir, float ZAngle)
    {
        GameObject projectileObj = ObjectPoolManager.instance.GetProjectileFromPool(0);
        if (projectileObj != null)
        {
            projectileObj.SetActive(true);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Shot(gameObject, attackTransform.transform.position, new Vector2(dir.x, dir.y).normalized,
                    stat.counterAttackRange, stat.counterAttackSpeed, stat.counterAttackDamage, isHeavy, ZAngle, eActivableColor.RED);
                projectileObj.transform.position = transform.position;

                PlayManager.Instance.UpdateColorthing();
                projectile.ReturnStartRoutine(stat.counterAttackRange);
            }
        }
    }
    public override void Dead()
    {
        StartCoroutine(DeadSequence());
    }
    private IEnumerator DeadSequence()
    {
        float deadDelayTime = 1.3f;
        SetState(EMonsterState.isBattle, false);
        SetState(EMonsterState.isWait, false);
        SetState(EMonsterState.isPlayerBetween, false);
        isDead = false;
        StopCoroutine(AttackSequence(PlayerPos));
        yield return new WaitForSeconds(deadDelayTime);
        gameObject.SetActive(false);
    }
    public override void CheckStateChange()
    {
        switch (state)
        {
            case EMonsterState.isBattle:
                fsm.ChangeState("Attack");
                break;
            case EMonsterState.isPlayerBetween:
                fsm.ChangeState("Chase");
                break;
            case EMonsterState.isWait:
                fsm.ChangeState("Idle");
                break;
            default:
                break;
        }
    }
    public override void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (currentState.HasFlag(EMonsterAttackState.isCounter))
        {
            CounterAttackPlay(new Vector2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y), Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg);
            currentState &= ~EMonsterAttackState.IsAttack;
        }
        else if (!isHeavyAttack)
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                HPDown(criticalDamage);
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                HPDown(damage);
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            HPDown(damage);
            rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        }
        if (!isDead)
        {
            CheckDead();
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage,
                    transform.position - collision.transform.position, false, stat.contactDamage);
        }
    }
}
