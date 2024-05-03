using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    void OnPostAttack(Vector2 attackDir);
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

    // 몬스터의 약공 기준 => 색이 보이면 약공
    private bool isHeavyAttack;
    private bool isAttackEnable = false;

    private eActivableColor attackColor;

    private float attackTime = 0f;

    private LayerMask ignoreLayers;

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        originLayer = gameObject.layer;
        ignoreLayers = (1 << LayerMask.NameToLayer("Platform")) | (1 << LayerMask.NameToLayer("IgnoreAttack"));
        colorVisibleLayer = LayerMask.NameToLayer("ColorObject");
        PlayManager.Instance.FilterColorAttackEvent.AddListener(CheckIsHeavy);

    }
    private void Update()
    {
        if (isAttackEnable)
        {
            attackTime += Time.deltaTime;
        }
    }

    public void SetAttack(string from, IAttack after, eActivableColor color = eActivableColor.MAX_COLOR)
    {
        attackFrom = from;
        afterAttack = after;
        attackColor = color;
        AttackDisable();
    }

    public void AttackDisable()
    {
        col.enabled = false;
        render.enabled = false;
        isAttackEnable = false;
        attackTime = 0f;
    }

    public void AttackAble(Vector2 dir, int damage, int critical = 0)
    {
        col.enabled = true;
        render.enabled = true;
        isAttackEnable = true;
        attackDir = dir;
        attackDamage = damage;
        criticalDamage = critical;
        anim.SetTrigger("attackTrigger");
    }

    private void CheckIsHeavy(eActivableColor color)
    {
        if(attackColor != color)
        {
            isHeavyAttack = true;
            gameObject.layer = colorVisibleLayer;
        }
        else
        {
            isHeavyAttack = false;
            gameObject.layer = originLayer;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(attackFrom) && 
            ignoreLayers != (ignoreLayers | (1 << collision.gameObject.layer)))
        {
            if (null != afterAttack)
            {
                afterAttack.OnPostAttack(attackDir);
            }
            collision.GetComponent<IAttack>()?.Hit(attackDamage, attackDir, isHeavyAttack, criticalDamage);
        }
    }
}