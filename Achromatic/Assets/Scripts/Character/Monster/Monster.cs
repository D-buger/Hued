using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;
using Spine.Unity;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Monster : MonoBehaviour, IAttack
{
    private Collider2D col;
    private Rigidbody rigid;

    private float elapsedTime = 0;

    [HideInInspector]
    public int currentHP;
    [HideInInspector]
    public bool isDead = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<Collider2D>();
    }

    public void OnPostAttack(Vector2 attackDir)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage = 0)
    {

    }
    public void CheckDead()
    {
        if (currentHP <= 0)
        {
            isDead = true;
        }
    }
}
