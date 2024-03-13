using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    void AfterAttack(Vector2 attackDir);
    void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0);
}

public class Attack : MonoBehaviour
{
    private const float PARRY_ALLOW_TIME = 0.5f; 

    private Collider2D col;
    private SpriteRenderer render;

    private IAttack afterAttack;

    private string attackFrom;
    private Vector2 attackDir;
    private int attackDamage;
    private int criticalDamage;

    private bool isHeavyAttack;
    private bool isAttackEnable = false;

    private float attackTime = 0f;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (isAttackEnable)
        {
            attackTime += Time.deltaTime;
        }
    }
    public void SetAttack(string from, IAttack after)
    {
        attackFrom = from;
        afterAttack = after;
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHeavyAttack && collision.CompareTag(PlayManager.ATTACK_TAG) && attackTime < PARRY_ALLOW_TIME)
        {

        }
        else if (!collision.CompareTag(attackFrom))
        {
            afterAttack.AfterAttack(attackDir);
            collision.GetComponent<IAttack>()?.Hit(attackDamage, attackDir, isHeavyAttack, criticalDamage);
        }
    }
}