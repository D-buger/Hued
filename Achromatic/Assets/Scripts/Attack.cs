using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface iAttack
{
    void AfterAttack(Vector2 attackDir);
}

public class Attack : MonoBehaviour
{
    private Collider2D col;
    private SpriteRenderer render;

    private iAttack afterAttack;

    private string attackFrom;
    private Vector2 attackDir;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
    }

    public void SetAttack(string from, iAttack after)
    {
        attackFrom = from;
        afterAttack = after;
        AttackDisable();
    }

    public void AttackDisable()
    {
        col.enabled = false;
        render.enabled = false;
    }

    public void AttackAble(Vector2 dir)
    {
        col.enabled = true;
        render.enabled = true;
        attackDir = dir;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(attackFrom))
        {
            afterAttack.AfterAttack(attackDir);
        }
    }
}