using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IAttack
{
    private Rigidbody2D rigid;

    private GameObject attackPoint;
    private Attack meleeAttack;

    [SerializeField]
    private MonsterStat stat;

    private bool isAttack = false;
    private bool canAttack = true;
    private bool detectTarget = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        attackPoint = transform.GetChild(0).gameObject;
        meleeAttack = attackPoint.GetComponentInChildren<Attack>();
    }

    private void Start()
    {
        meleeAttack.SetAttack(PlayManager.ENEMY_TAG, this);
    }

    private void Update()
    {
        CheckPlayer();
        if (canAttack && detectTarget)
        {
            Attack(PlayManager.Instance.GetPlayer.transform.position);
        }
    }

    private void CheckPlayer()
    {
        if (Vector2.Distance(PlayManager.Instance.GetPlayer.transform.position, transform.position) < stat.senseCircle)
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

        float angle = Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg;

        attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        meleeAttack.AttackAble(attackAngle, stat.attackDamage, false);
        yield return Yields.WaitSeconds(stat.attackTime);
        isAttack = false;
        meleeAttack.AttackDisable();
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

    private void CheckDead()
    {
        if (stat.MonsterHP <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
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
        }
    }
}
