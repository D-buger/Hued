using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static SpiderEnemy;
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

    private bool isAttack = false;
    private bool isHeavy = false;
    private bool isStabAttack = false;
    private bool isCounter = false;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (canAttack && !isAttack)
        {
            StartCoroutine(AttackSequence(PlayerPos));
        }
        if (Vector2.Distance(transform.position, PlayerPos) >= stat.senseCircle)
        {
            isBattle = false;
            isPlayerBetween = true;
            isWait = false;
        }
    }
    private IEnumerator AttackSequence(Vector2 attackAngle)
    {
        isAttack = true;
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

        int randomChance = UnityEngine.Random.Range(1, 100);
        if (randomChance <= stat.swordAttackPercent)
        {
            StartCoroutine(SwordAttack(value, check, ZAngle));
        }
        else if (randomChance >= stat.stabAttackPercent && stat.stabAttackPercent + stat.swordAttackPercent >= randomChance)
        {
            isStabAttack = true;
            StabAttack(check);
        }    
        else
        {
            StartCoroutine(CounterAttackStart());
        }

        yield return Yields.WaitSeconds(stat.attackTime);
        yield return Yields.WaitSeconds(stat.attackCooldown);
        isAttack = false;
        canAttack = true;
    }

    private IEnumerator SwordAttack(Vector2 dir, Vector2 check, float ZAngle)
    {
        if (isWait)
        {
            yield return null;
        }
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, ZAngle);
        meleeAttack?.AttackAble(-dir, stat.attackDamage);
        rigid.AddForce(check * stat.swordAttackRebound, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(0.0f); // FIX 근접 공격 애니메이션 JSON 파싱
        GameObject projectileObj = ObjectPoolManager.Instance.GetProjectileFromPool();
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
                projectile.ReturnStart(stat.swordAttackRange);
            }
        }
    }

    private IEnumerator StabAttack(Vector2 check)
    {
        if (isWait)
        {
            yield break;
        }
        float delayToAttack = 0.2f;
        float delayToDestory = 0.05f;
        int objectCount = stabAttackOBJ.Length/2;
        while (isStabAttack)
        {
            isStabAttack = false;
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
        isCounter = true;
        yield return Yields.WaitSeconds(stat.counterAttackTime);
        isCounter = false;
    }
    private void CounterAttackPlay(Vector2 dir, float ZAngle)
    {
        GameObject projectileObj = ObjectPoolManager.Instance.GetProjectileFromPool();
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
                projectile.ReturnStart(stat.counterAttackRange);
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
        isBattle = false;
        isWait = false;
        isPlayerBetween = false;
        isDead = false;
        StopCoroutine(AttackSequence(PlayerPos));
        yield return new WaitForSeconds(deadDelayTime);
        gameObject.SetActive(false);
    }
    public override void CheckStateChange()
    {
        if (isWait)
        {
            fsm.ChangeState("Idle");
        }
        if (isPlayerBetween)
        {
            fsm.ChangeState("Chase");
        }
        if (isBattle)
        {
            fsm.ChangeState("Attack");
        }
    }
    public override void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (isCounter)
        {
            CounterAttackPlay(new Vector2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y), Mathf.Atan2(PlayerPos.x - transform.position.x, PlayerPos.y - transform.position.y) * Mathf.Rad2Deg);
            isCounter = false;
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
