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
    private Collider2D col;
    private SpriteRenderer render;
    private Animator anim;

    private IAttack afterAttack;
    private IParry parried;
    public IParry Parried => parried;

    private string attackFrom;
    public bool isCanParryAttack(string me) => !string.Equals(me, attackFrom) && !isHeavyAttack;
    private Vector2 attackDir;
    private int attackDamage;
    private int criticalDamage;

    // 플레이어는 강공, 몬스터는 약공이여야 패링 가능
    // 몬스터의 약공 기준 => 색이 보이면 약공
    private bool isHeavyAttack;
    private bool isAttackEnable = false;

    private float attackTime = 0f;

    private LayerMask ignoreLayers;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        ignoreLayers = (1 << LayerMask.NameToLayer("Platform")) | (1 << LayerMask.NameToLayer("IgnoreAttack"));
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
        anim.SetTrigger("attackTrigger");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(attackFrom) && 
            ignoreLayers != (ignoreLayers | (1 << collision.gameObject.layer)))
        {
            if (null != afterAttack)
            {
                afterAttack.AfterAttack(attackDir);
            }
            collision.GetComponent<IAttack>()?.Hit(attackDamage, attackDir, isHeavyAttack, criticalDamage);
        }
    }
}