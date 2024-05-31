using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IAttack
{
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;
    private Animator anim;

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

    private int currentHP;

    private bool isAttack = false;
    private bool canAttack = true;
    private bool detectTarget = false;
    private bool isGroggy = false;
    private bool isDead = false;
    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (isMeleeMonster)
        {
            attackPoint = transform.GetChild(0).gameObject;
            meleeAttack = attackPoint.GetComponentInChildren<Attack>();
        }
    }

    private void Start()
    {
        currentHP = stat.MonsterHP;
        originLayer = gameObject.layer;
        colorVisibleLayer = LayerMask.NameToLayer("ColorEnemy");
        meleeAttack?.SetAttack(PlayManager.ENEMY_TAG, this, stat.enemyColor);

        PlayManager.Instance.FilterColorAttackEvent.AddListener(IsActiveColor);
        PlayManager.Instance.UpdateColorthing();
    }

    private void Update()
    {
        if (isGroggy || isDead)
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
        float verticalValue = attackAngle.y - transform.position.y;
        Vector2 value = new Vector2(horizontalValue, verticalValue);

        if(horizontalValue > 0)
        {
            renderer.flipX = false;
        }
        else
        {
            renderer.flipX = true;
        }

        anim.SetTrigger("attackTrigger");
        if (!isMeleeMonster)
        {
            /*Projectile attack = Instantiate(rangedAttack.gameObject).GetComponent<Projectile>();
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized, 
                    projectileRange ,projectileSpeed, stat.attackDamage, false, eActivableColor.RED);
            }
            else
            {
                attack.Shot(gameObject, transform.position, new Vector2(horizontalValue, verticalValue).normalized,
                   projectileRange, projectileSpeed, stat.attackDamage, true, eActivableColor.RED);
            }*/
        }
        else
        {
            float angle = Mathf.Atan2(verticalValue, horizontalValue) * Mathf.Rad2Deg;

            attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                meleeAttack?.AttackEnable(-value, stat.attackDamage, stat.attackDamage);
            }
            else
            {
                meleeAttack?.AttackEnable(-value, stat.attackDamage, stat.attackDamage);
            }
        }
        yield return Yields.WaitSeconds(stat.attackTime);
        isAttack = false;
        meleeAttack?.AttackDisable();
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }

    public void OnPostAttack(Vector2 attackDir)
    {

    }

    // 임시 테스트 코드
    public void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck, bool isInfinityRebound = false)
    {
        //if (!isHeavyAttack)
        //{
        //    if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
        //    {
        //        currentHP -= criticalDamage;
        //        rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        //    }
        //    else
        //    {
        //        currentHP -= damage;
        //        rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
        //    }
        //}
        //else
        //{
        //    if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
        //    {
        //        currentHP -= damage;
        //        rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        //    }
        //}
        CheckDead();
    }

    public void Parried()
    {
        StartCoroutine(Groggy());
    }

    IEnumerator Groggy()
    {
        Color originColor = renderer.color;
        isGroggy = true;
        renderer.color = Color.gray;
        yield return Yields.WaitSeconds(stat.groggyTime);
        renderer.color = originColor;
        isGroggy = false;
    }

    private void CheckDead()
    {
        if (currentHP <= 0 && !isDead)
        {
            isDead = true;
            anim.SetTrigger("deathTrigger");
        }
    }

    public void Dead()
    {
        gameObject.SetActive(false);
    }

    private void IsActiveColor(eActivableColor color)
    {
        if(color != stat.enemyColor)
        {
            gameObject.layer = originLayer;
        }
        else
        {
            gameObject.layer = colorVisibleLayer;
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.contactDamage, stat.contactDamage, 
                    transform.position - collision.transform.position);
        }
    }
}
