using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 
/// isRunning
/// isGroggy
/// onGround
/// 
/// dashTrigger
/// attackTrigger
/// parryTrigger
/// hitTrigger
/// jumpTrigger
/// 
/// </summary>


enum ePlayerState : int
{
    IDLE = 0,
    RUNNING = 1,
    JUMPING = 2,
    DASH = 4,
    DASHING = 8,
    GROGGY = 16,
    GROGGING = 32,
    HIT = 64,
    ATTACK = 128
}

public class Player : MonoBehaviour, IAttack
{
    private Rigidbody2D rigid;
    private BoxCollider2D coll;
    private SpriteRenderer renderer;
    private Animator ani;

    private GameObject effects;

    private GameObject parryEffect;

    private GameObject attackPoint;
    private Attack attack;

    [SerializeField]
    private PlayerStatus stat;
    public PlayerStatus GetPlayerStat() => stat;

    private int currentHP
    {
        get
        {
            return stat.currentHP;
        }
        set
        {
            stat.currentHP = value;
            if (stat.currentHP < 0)
            {
                Dead();
            }
            
        }
    } 

    private bool isSit = false;
    private bool isJump = false;
    private bool canJump = true;
    private bool isAttack = false;
    private bool canAttack = true;
    private bool isAttackRebound = false;
    private bool isDash = false;
    private bool isParryDash = false;
    private bool canDash = true;
    private bool parryCondition = false;
    private bool isParry = false;
    private bool isInvincibility = false;
    private bool isHit = false;
    private bool onGround = false;

    private float rayRange = 0.01f;
    private float horizontalMove = 0;
    private bool playerFaceRight = true;
    private ePlayerState state;

    private Collision2D parryDashCollision;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        attackPoint = transform.GetChild(0).gameObject;
        attack = transform.GetChild(0).GetChild(0).GetComponent<Attack>();

        effects = transform.GetChild(1).gameObject;
        parryEffect = effects.transform.GetChild(0).gameObject;

        for(int i = 0; i < effects.transform.childCount; i++)
        {
            effects.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void Start()
    {
        InputManager.Instance.MoveEvent.AddListener(Move);
        InputManager.Instance.JumpEvent.AddListener(Jump);
        InputManager.Instance.SitEvent.AddListener(Sit);
        InputManager.Instance.DashEvent.AddListener(Dash);
        InputManager.Instance.LightAttackEvent.AddListener(LightAttack);

        attack.SetAttack(PlayManager.PLAYER_TAG, this);

        stat.currentHP = stat.playerHP;
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (isDash || isAttackRebound)
        {
            return;
        }

        rigid.velocity = new Vector2(horizontalMove, rigid.velocity.y);

        if(rigid.velocity.y != 0)
        {
            ani.SetBool("onGround", false);
        }

        onGround = false;
        isSit = false;
    }

    void SetAnimation()
    {

    }

    void Move(float dir)
    {
        if (isDash || isAttackRebound) 
        {
            return;
        }

        ani.SetBool("isRunning", dir != 0 ? true : false);
        if (dir != 0)
        {
            playerFaceRight = dir > 0 ? true : false;
        }
        renderer.flipX = playerFaceRight;
        if (!isSit)
        {
            horizontalMove = dir * stat.moveSpeed; 
        }
        else
        {
            horizontalMove = dir * stat.moveSpeed * stat.sitDeceleration;
        }
    }

    void Jump()
    {
        if (!isJump && canJump && onGround && !isDash)
        {
            StartCoroutine(JumpSequence());
        }
    }
    IEnumerator JumpSequence()
    {
        isJump = true;
        canJump = false;
        rigid.AddForce(Vector2.up * stat.jumpPower, ForceMode2D.Impulse);
        ani.SetTrigger("jumpTrigger");
        yield return Yields.WaitSeconds(stat.jumpCooldown);
        canJump = true;
    }

    void Sit()
    {
        if (isJump)
        {
            rigid.AddForce(Vector2.down * stat.sitDescentSpeed, ForceMode2D.Force);
        }
        else
        {
            isSit = true;
        }
    }
    void Dash(Vector2 mousePos)
    {
        if (canDash && canAttack)
        {
            if (isParry)
            {
                StartCoroutine(ParryDashSequence(mousePos));
            }
            else
            {
                StartCoroutine(DashSequence(mousePos));
            }
        }
    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        isDash = true;
        canDash = false;

        float originGravityScale = rigid.gravityScale;
        float originLiniearDrag = rigid.drag;
        float originMass = rigid.mass;
        rigid.gravityScale = 0f;
        rigid.drag = 0;
        rigid.mass = 0;

        dashPos.x = dashPos.x - transform.position.x;
        dashPos.y = dashPos.y - transform.position.y;

        rigid.velocity = dashPos.normalized * stat.dashPower;

        if (dashPos.y > 0)
        {
            isJump = true;
            ani.SetBool("onGround", false);
        }
        ani.SetTrigger("dashTrigger");
        playerFaceRight = dashPos.x > 0 ? true : false;

        yield return Yields.WaitSeconds(stat.dashingTime);
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = originGravityScale;
        rigid.drag = originLiniearDrag;
        rigid.mass = originMass;
        isDash = false;

        isParry = parryCondition;
        parryCondition = false;
        ani.SetTrigger("dashEndTrigger");
        if (isParry)
        {
            StartCoroutine(ParrySequence());
        }

        yield return Yields.WaitSeconds(stat.dashCooldown);
        canDash = true;
    }

    IEnumerator ParryDashSequence(Vector2 dashPos)
    {
        isParry = false;
        isDash = true;
        isParryDash = true;
        canDash = false;

        float originGravityScale = rigid.gravityScale;
        float originLiniearDrag = rigid.drag;
        float originMass = rigid.mass;
        coll.forceReceiveLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        coll.forceSendLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        rigid.gravityScale = 0f;
        rigid.drag = 0;
        rigid.mass = 0;

        dashPos.x = dashPos.x - transform.position.x;
        dashPos.y = dashPos.y - transform.position.y;

        rigid.velocity = dashPos.normalized * stat.parryDashPower;

        if (dashPos.y > 0)
        {
            isJump = true;
            ani.SetBool("onGround", false);
        }
        ani.SetTrigger("dashTrigger");
        playerFaceRight = dashPos.x > 0 ? true : false;

        yield return Yields.WaitSeconds(stat.parryDashTime);
        coll.forceReceiveLayers |= (1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        coll.forceSendLayers |= (1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = originGravityScale;
        rigid.drag = originLiniearDrag;
        rigid.mass = originMass;
        isDash = false;
        isParryDash = false;

        yield return Yields.WaitSeconds(stat.dashCooldown);
        canDash = true;
    }

    IEnumerator ParrySequence()
    {
        Color originColor = renderer.color;
        renderer.color = Color.gray;

        isInvincibility = true;

        Time.timeScale = stat.parryProduceTimescale;
        ani.SetTrigger("parryTrigger");
        parryEffect.SetActive(true);
        yield return Yields.WaitSeconds(stat.parryProduceTime);
        Time.timeScale = 1f;
        parryEffect.SetActive(false);

        yield return Yields.WaitSeconds(stat.invincibilityAfterParry);
        isInvincibility = false;

        do {
            yield return null;
        } while (isParry);

        renderer.color = originColor;
    }

    private void LightAttack(Vector2 mousePos)
    {
        if (canAttack)
        {
            ani.SetTrigger("attackTrigger");
            StartCoroutine(AttackSequence(mousePos));
        }
    }

    IEnumerator AttackSequence(Vector2 attackAngle)
    {
        canAttack = false;
        isAttack = true;
        float angle;
        angle = VectorToEulerAngle(attackAngle) - 180;
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        Vector2 angleVec = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);

        if (!isParry)
        {
            attack.AttackAble(angleVec.normalized, stat.attackDamage, false, stat.colorAttackDamage);
        }
        else
        {
            isParry = false;
            attack.AttackAble(angleVec.normalized, stat.criticalAttackDamage, false, stat.colorCriticalAttackDamage);
        }

        yield return Yields.WaitSeconds(stat.attackTime);
        attack.AttackDisable();
        isAttack = false;
        yield return Yields.WaitSeconds(stat.attackCooldown);
        canAttack = true;
    }
    private float VectorToEulerAngle(Vector2 vec)
    {
        float horizontalValue = vec.x - transform.position.x;
        float VerticalValue = vec.y - transform.position.y;

        return Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg + 180;
    }

    public void AfterAttack(Vector2 attackDir)
    {
        StartCoroutine(ReboundSequence(attackDir, stat.attackReboundPower, stat.attackReboundTime, 0.05f));
    }

    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        if (isDash)
        {
            return;
        }

        parryCondition = false;
        if (!isInvincibility)
        {
            attackDir.y = 0;
            isHit = true;
            currentHP -= damage;
            ani.SetTrigger("hitTrigger");
            StartCoroutine(ReboundSequence(attackDir.normalized, stat.hitReboundPower, stat.hitReboundTime, 0.1f));
        }
    }
    IEnumerator ReboundSequence(Vector2 dir, float reboundPower, float reboundTime,float shockAmount)
    {
        isAttackRebound = true;
        rigid.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(shockAmount);
        yield return Yields.WaitSeconds(reboundTime);
        isAttackRebound = false;
        isHit = false;
    }

    private void Dead()
    {
        Debug.Log("Player Dead");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rigid.velocity = new Vector2(rigid.velocity.x, 0);

        if (collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            if (isDash)
            {
                collision.gameObject.GetComponent<TestEnemy>().Hit(isParry ? stat.parryDashDamage : stat.dashDamage, 
                    collision.transform.position - transform.position, false);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.transform.position.y < transform.position.y)
        {
            ani.SetBool("onGround", true);
            onGround = true;
            isJump = false;
        }

        if (isParryDash)
        {
            parryDashCollision = collision;
            Debug.Log(collision.gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.ATTACK_TAG))
        {
            parryCondition = !collision.GetComponent<Attack>().isAttackFromMe(PlayManager.PLAYER_TAG);
            Debug.Log(parryCondition);
        }

    }
}
