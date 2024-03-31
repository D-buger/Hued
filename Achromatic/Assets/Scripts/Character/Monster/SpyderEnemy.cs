using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class SpyderEnemy : MonoBehaviour, IAttack
{
    [Header("Components")]
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;
    private Animator anim;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private SpyderMonsterStats stat;
    [SerializeField, Space(10)]
    private Projectile rangedAttack;

    private float elapsedTime = 0;
    private float arrivalThreshold = 1f;
    private float distanceToPlayer = 0;
    private float angleThreshold = 40f;


    [SerializeField, Tooltip("대기 상태 중 이동을 시작하는 범위")]
    private Vector2 startPosition;
    [SerializeField, Tooltip("대기 상태 중 이동을 끝내는 범위")]
    private Vector2 targetPosition;
    private Vector2 thisPosition;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private int currentHP;

    private bool isDead = false;
    private bool isBettle = false;
    private bool canAttack = true;
    private bool isAttack = false;
    private bool isWait = true;
    private bool isfirstAttack = false;
    private bool playerBetweenPositions = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }
    private void Start()
    {
        currentHP = stat.MonsterHP;
        thisPosition = targetPosition;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }
        CheckPlayer();
        if (isWait)
        {
            WaitSituation();
        }
        else if (canAttack && isBettle)
        {
            Attack(PlayerPos);
        }

        if (!playerBetweenPositions)
        {
            CheckWaitTime();
        }
    }
    private void CheckPlayer()
    {
        if (IsBetween(PlayerPos.x, startPosition.x, targetPosition.x))
        {
            playerBetweenPositions = true;
            isBettle = true;
            isWait = false;
            elapsedTime = 0f;
        }
        else
        {
            playerBetweenPositions = false;
        }

        distanceToPlayer = Vector2.Distance(transform.position, PlayerPos);
    }
    private bool IsBetween(float value, float start, float end)
    {
        return value >= Mathf.Min(start, end) && value <= Mathf.Max(start, end);
    }
    private void CheckWaitTime()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= stat.usualTime)
        {
            elapsedTime = 0f;
            isWait = true;
        }
    }

    private void WaitSituation()
    {
        currentHP = stat.MonsterHP;
        isfirstAttack = true;
        isBettle = false;
        transform.position = Vector2.MoveTowards(transform.position, thisPosition, stat.moveSpeed * Time.deltaTime);

        if (HasArrived((Vector2)transform.position, targetPosition))
        {
            thisPosition = startPosition;
        }
        if (HasArrived((Vector2)transform.position, startPosition))
        {
            thisPosition = targetPosition;
        }
    }
    private bool HasArrived(Vector2 currentPosition, Vector2 targetPosition)
    {
        return Vector2.Distance(currentPosition, targetPosition) <= arrivalThreshold;
    }


    public void Attack(Vector2 vec)
    {
        StartCoroutine(MoveToPlayer());
    }

    IEnumerator AttackSequence(Vector2 attackAngle)
    {
        isAttack = true;
        canAttack = false;
        float horizontalValue = attackAngle.x - transform.position.x;
        float verticalValue = attackAngle.y - transform.position.y;
        Vector2 value = new Vector2(horizontalValue, verticalValue);

        float angleToPlayer = Mathf.Atan2(value.y, value.x) * Mathf.Rad2Deg;
        bool facingPlayer = Mathf.Abs(angleToPlayer - transform.eulerAngles.z) < angleThreshold;


        anim.SetTrigger("attackTrigger");

        if (isfirstAttack && distanceToPlayer > stat.meleeAttackRange && distanceToPlayer < stat.rangedAttackRange)
        {
            //Fix 거미줄 공격에 맞게 수정
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            isfirstAttack = false;
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, false);
            }
            else
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                   stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, true);
            }
        }
        else if (distanceToPlayer < stat.meleeAttackRange)
        {
            float randomChance = UnityEngine.Random.value;
            Debug.Log("근거리 공격");

            if (facingPlayer && randomChance <= stat.specialAttackPercent*100)
            {
                if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
                {
                    //TODO : 고개치기 공격 구현
                }
                else
                {
                    
                }
            }
            else
            {
                Debug.Log("땅찍기"); 
                //땅찍기 공격 구현
            }
            
        }
        else if (distanceToPlayer > stat.meleeAttackRange && distanceToPlayer < stat.rangedAttackRange)
        {
            Debug.Log("원거리 공격");
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            isfirstAttack = false;
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                    stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, false);
            }
            else
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                   stat.rangedAttackRange, stat.rangedAttackSpeed, stat.rangedAttackDamege, true);
            }
        }


        yield return Yields.WaitSeconds(stat.attackTime);
        isAttack = false;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    IEnumerator MoveToPlayer()
    {
        while (!isAttack && !isWait)
        {
            yield return new WaitForSeconds(stat.attackCooldown);
            float horizontalValue = PlayerPos.x - transform.position.x;
            float verticalValue = PlayerPos.y - transform.position.y;

            if (horizontalValue > 0)
            {
                renderer.flipX = false;
            }
            else
            {
                renderer.flipX = true;
            }

            if (PlayerPos == null)
                yield break;

            if (distanceToPlayer <= stat.rangedAttackRange && canAttack)
            {
                StartCoroutine(AttackSequence(PlayerPos));
                Debug.Log("어택시퀀스 실행");
                yield break;
            }
            else if (distanceToPlayer > stat.rangedAttackRange)
            {
                //FIX X좌표로만 이동하게 변경
                transform.position = Vector2.MoveTowards(transform.position, PlayerPos, stat.moveSpeed * Time.deltaTime);
            }
        }
    }
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (!isHeavyAttack)
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= criticalDamage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                currentHP -= damage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
        }
        CheckDead();
    }
    private void CheckDead()
    {
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            anim.SetTrigger("deathTrigger");
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }

    public void AfterAttack(Vector2 attackDir)
    {
        throw new NotImplementedException();
    }
}
