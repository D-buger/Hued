using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    void AfterAttack(Vector2 attackDir);
    void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0);
}

public interface IParry
{
    void Parried();
}

public class Attack : MonoBehaviour
{
    private const float PARRY_ALLOW_TIME = 0.2f; 

    private Collider2D col;
    private SpriteRenderer render;
    private Animator anim;

    private IAttack afterAttack;
    private IParry parried;
    public IParry Parried => parried;

    private string attackFrom;
    public bool isAttackFromMe(string me) => string.Equals(me, attackFrom);
    private Vector2 attackDir;
    private int attackDamage;
    private int criticalDamage;

    // 플레이어는 강공, 몬스터는 약공이여야 패링 가능
    // 몬스터의 약공 기준 => 색이 보이면 약공
    private bool isHeavyAttack;
    private bool isAttackEnable = false;

    private float attackTime = 0f;

    private bool isParriedAttack = false;
    public bool IsParryAllow => (!isHeavyAttack && attackTime < PARRY_ALLOW_TIME);

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (isAttackEnable)
        {
            attackTime += Time.deltaTime;
        }
    }
    public void SetAttack(string from, IAttack after, IParry parry = null)
    {
        attackFrom = from;
        afterAttack = after;
        parried = parry;
        AttackDisable();
    }

    public void AttackDisable()
    {
        col.enabled = false;
        render.enabled = false;
        isAttackEnable = false;
        attackTime = 0f;
    }

    public void AttackAble(Vector2 dir, int damage, bool isHeavy, int critical = 0)
    {
        col.enabled = true;
        render.enabled = true;
        isAttackEnable = true;
        attackDir = dir;
        attackDamage = damage;
        criticalDamage = critical;
        isHeavyAttack = isHeavy;
        isParriedAttack = false;
        anim.SetTrigger("attackTrigger");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (string.Equals(attackFrom, PlayManager.PLAYER_TAG) && isHeavyAttack && collision.CompareTag(PlayManager.ATTACK_TAG))
        {
            Attack enemy = collision.GetComponent<Attack>();
            Projectile misile = collision.GetComponent<Projectile>();
            if (null != enemy)
            {
                if (enemy.IsParryAllow)
                {
                    enemy.parried.Parried();
                    Debug.Log("패링 성공");
                    isParriedAttack = true;
                }
                else
                {
                    Debug.Log("패링실패");
                }
            }
            else if(null != misile)
            {
                if (misile.IsParryAllow)
                {
                    Vector2 dir = new Vector2(attackDir.x, attackDir.y);
                    misile.Parried(gameObject, dir ,attackDamage);
                    Debug.Log("패링 성공");
                }
                else
                {
                    Debug.Log("패링실패");
                }
            }
        }
        else if (!collision.CompareTag(attackFrom) && !isParriedAttack)
        {
            if (null != afterAttack)
            {
                afterAttack.AfterAttack(attackDir);
            }
            collision.GetComponent<IAttack>()?.Hit(attackDamage, attackDir, isHeavyAttack, criticalDamage);
        }
    }
}