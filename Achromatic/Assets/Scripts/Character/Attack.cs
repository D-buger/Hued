using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    void OnPostAttack(Vector2 attackDir);
    void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck = null);
}

public interface IParryConditionCheck
{
    public bool CanParryAttack();
}

public class Attack : MonoBehaviour, IParryConditionCheck
{
    private Collider2D col;
    private SpriteRenderer render;
    private Animator anim;

    private IAttack afterAttack;

    private string attackFrom;

    private Vector2 attackDir;
    private int attackDamage;
    private int colorAttackDamage;

    private bool isCanParryAttack;
    private bool isAttackEnable = false;

    private eActivableColor attackColor;

    private LayerMask ignoreLayers;

    private LayerMask originLayer;
    private LayerMask colorVisibleLayer;
    public bool CanParryAttack() => !string.Equals(PlayManager.PLAYER_TAG, attackFrom) && isCanParryAttack;
    public bool IsPlayerAttack() => string.Equals(PlayManager.PLAYER_TAG, attackFrom);

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        render = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        ignoreLayers = LayerMask.GetMask("Platform") | LayerMask.GetMask("IgnoreAttack") | LayerMask.GetMask("Object");
        originLayer = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));
        colorVisibleLayer = LayerMask.GetMask("ColorObject");
        PlayManager.Instance.FilterColorAttackEvent.AddListener(CheckIsHeavy);
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
    }

    public void AttackEnable(Vector2 dir, int damage, int colorDamage)
    {
        col.enabled = true;
        render.enabled = true;
        isAttackEnable = true;
        attackDir = dir;
        attackDamage = damage;
        colorAttackDamage = colorDamage;
        anim.SetTrigger("attackTrigger");
    }

    private void CheckIsHeavy(eActivableColor color)
    {
        isCanParryAttack = attackColor != color ? false : true;
        gameObject.layer = SOO.Util.LayerMaskToNumber(attackColor != color ? colorVisibleLayer : originLayer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(attackFrom) &&
            ignoreLayers != (ignoreLayers | (1 << collision.gameObject.layer)))
        {
            if (!ReferenceEquals(afterAttack, null))
            {
                afterAttack.OnPostAttack(attackDir);
            }
            collision.GetComponent<IAttack>()?.Hit(attackDamage, colorAttackDamage, attackDir);
        }
    }
}