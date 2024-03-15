using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IAttack, IParry
{
    private Rigidbody2D rigid;

    private GameObject attackPoint;
    private Attack meleeAttack;

    [SerializeField]
    private MonsterStat stat;
    [SerializeField]
    private bool isMeleeMonster = true;

    [SerializeField, Space(10)]
    private Projectile rangedAttack;
    [SerializeField]
    private float projectileSpeed = 5f;
    [SerializeField]
    private float projectileRange = 5f;


    private bool isAttack = false;
    private bool canAttack = true;
    private bool detectTarget = false;
    private bool isGroggy = false;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        if (isMeleeMonster)
        {
            attackPoint = transform.GetChild(0).gameObject;
            meleeAttack = attackPoint.GetComponentInChildren<Attack>();
        }
    }

    private void Start()
    {
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, this);
    }

    private void Update()
    {
        if (isGroggy)
        {
            return;
        }

        CheckPlayer();
        if (canAttack && detectTarget)
        {
            Attack(PlayerPos);
        }
    }

    private void CheckPlayer()
    {
        if (Vector2.Distance(PlayerPos, transform.position) < stat.senseCircle)
        {
            detectTarget = true;
        }
        else
        {
            detectTarget = false;
        }
    }

    public void Attack(Vector2 vec)
    {
        StartCoroutine(AttackSequence(vec));
    }

    IEnumerator AttackSequence(Vector2 attackAngle)
    {
        isAttack = true;
        canAttack = false; 
        float horizontalValue = attackAngle.x - transform.position.x;
        float VerticalValue = attackAngle.y - transform.position.y;

        if (!isMeleeMonster)
        {
            Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, VerticalValue).normalized, 
                    projectileRange ,projectileSpeed, stat.attackDamage, false);
            }
            else
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, VerticalValue).normalized,
                   projectileRange, projectileSpeed, stat.attackDamage, true);
            }
        }
        else
        {
            float angle = Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg;

            attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                meleeAttack?.AttackAble(attackAngle, stat.attackDamage, false);
            }
            else
            {
                meleeAttack?.AttackAble(attackAngle, stat.attackDamage, true);
            }
        }
        yield return Yields.WaitSeconds(stat.attackTime);
        isAttack = false;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    public void AfterAttack(Vector2 attackDir)
    {

    }

    // 임시 테스트 코드
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {
        if (!isHeavyAttack)
        {

            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                stat.MonsterHP -= criticalDamage;
                rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
            }
            else
            {
                stat.MonsterHP -= damage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
            }
            CheckDead();
        }
        else
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                stat.MonsterHP -= damage;
                rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
                CheckDead();
            }
        }
    }

    public void Parried()
    {
        StartCoroutine(Groggy());
    }

    IEnumerator Groggy()
    {
        isGroggy = true;
        yield return Yields.WaitSeconds(stat.groggyTime);
        isGroggy = false;
    }

    private void CheckDead()
    {
        if (stat.MonsterHP <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        if (isGroggy)
        {
            return;
        }

        if (null != stat)
        {
            if (detectTarget)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle);

            if (!isMeleeMonster)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + transform.forward, stat.senseCircle * projectileRange);
            }
        }
    }
}
