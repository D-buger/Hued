using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    void AfterAttack(Vector2 attackDir);
    void Hit(int damage, Vector2 attackDir, bool isHeavyAttack);
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

    public void AttackAble(Vector2 dir, int damage, bool isHeavy)
    {
        col.enabled = true;
        render.enabled = true;
        isAttackEnable = true;
        attackDir = dir;
        attackDamage = damage;
        isHeavyAttack = isHeavy;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(attackFrom))
        {
            afterAttack.AfterAttack(attackDir);
            collision.GetComponent<IAttack>()?.Hit(attackDamage, attackDir, isHeavyAttack);
        }
    }
}