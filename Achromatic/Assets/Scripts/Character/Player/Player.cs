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

    private GameObject attackPoint;
    private Attack lightAttack;

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
            if (!isInvincibility)
            {
                ani.SetTrigger("hitTrigger");
                stat.currentHP = value;
                if (stat.currentHP < 0)
                {
                    Dead();
                }
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
    private bool canDash = true;
    private bool canParryDash = false;
    private bool isInvincibility = false;

    private float rayRange = 0.01f;
    private float horizontalMove = 0;
    private bool playerFaceRight = true;
    private ePlayerState state;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();

        attackPoint = transform.GetChild(0).gameObject;
        lightAttack = transform.GetChild(0).GetChild(0).GetComponent<Attack>();
    }

    void Start()
    {
        InputManager.Instance.MoveEvent.AddListener(Move);
        InputManager.Instance.JumpEvent.AddListener(Jump);
        InputManager.Instance.SitEvent.AddListener(Sit);
        InputManager.Instance.DashEvent.AddListener(Dash);
        InputManager.Instance.LightAttackEvent.AddListener(LightAttack);

        lightAttack.SetAttack(PlayManager.PLAYER_TAG, this);

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
        else
        {
            //ani.SetBool("onGround", true);
        }

        //if (rigid.velocity.y < 0)
        //{
        //    Debug.DrawRay(rigid.position, new Vector3(0, -1 * (coll.size.y / 2 + rayRange), 0), Color.red);

        //    RaycastHit2D rayHit =
        //        Physics2D.Raycast(rigid.position, Vector3.down, coll.size.y / 2 + rayRange, LayerMask.GetMask("Platform"));

        //    if (rayHit.collider != null)
        //    {
        //        ani.SetBool("onGround", true);
        //        isJump = false;
        //    }
        //}
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
        if (!isJump && canJump)
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
            if (canParryDash)
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
        Color originColor = renderer.color;
        isDash = true;
        canDash = false;
        renderer.color = Color.gray;

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
        yield return Yields.WaitSeconds(dashPos.y > 0 ? stat.dashingTime : stat.dashingTime);
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = originGravityScale;
        rigid.drag = originLiniearDrag;
        rigid.mass = originMass;
        isDash = false;
        ani.SetTrigger("dashEndTrigger");
        yield return Yields.WaitSeconds(stat.invincibilityTimeAfterDash);
        renderer.color = originColor;
        yield return Yields.WaitSeconds(Mathf.Max(0, stat.dashCooldown));
        canDash = true;
    }

    IEnumerator ParryDashSequence(Vector2 dashPos)
    {
        yield return null;
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

        lightAttack.AttackAble(angleVec.normalized, stat.lightAttackDamage, false, stat.lightCriticalAttackDamage);
        yield return Yields.WaitSeconds(stat.lightAttackTime);
        lightAttack.AttackDisable();
        isAttack = false;
        yield return Yields.WaitSeconds(stat.lightAttackCooldown);
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
        StartCoroutine(AfterAttackSequence(attackDir, 0.05f));
    }
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        canParryDash = false;
        currentHP -= damage;
        if (!isInvincibility)
        {
            StartCoroutine(AfterAttackSequence(attackDir.normalized, 0.1f));
        }
    }
    IEnumerator AfterAttackSequence(Vector2 attackDir, float shockAmount)
    {
        isAttackRebound = true;
        rigid.AddForce(-attackDir * stat.attackReboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(shockAmount);
        yield return Yields.WaitSeconds(stat.attackReboundTime);
        isAttackRebound = false;
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
                collision.gameObject.GetComponent<TestEnemy>().Hit(stat.dashDamage, 
                    collision.transform.position - transform.position, false);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.transform.position.y < transform.position.y)
        {
            ani.SetBool("onGround", true);
            isJump = false;
        }
    }
}
