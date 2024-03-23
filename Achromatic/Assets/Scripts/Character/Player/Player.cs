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
        InputManager.Instance.HeavyAttackEvent.AddListener(HeavyAttack);

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
            ani.SetBool("onGround", true);
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
        isInvincibility = true;
        renderer.color = Color.gray;
        float originGravityScale = rigid.gravityScale;
        rigid.gravityScale = 0f;
        dashPos.x = dashPos.x - transform.position.x;
        dashPos.y = dashPos.y - transform.position.y;
        rigid.velocity = new Vector2(transform.localScale.x * dashPos.normalized.x * stat.dashPower, 
            transform.localScale.y * dashPos.normalized.y * stat.dashPower / 2);
        if(dashPos.y > 0)
        {
            isJump = true;
        }
        playerFaceRight = dashPos.x > 0 ? true : false;
        ani.SetTrigger("dashTrigger");
        yield return Yields.WaitSeconds(dashPos.y > 0 ? stat.dashingTime / 3 : stat.dashingTime);
        rigid.gravityScale = originGravityScale;
        isDash = false;
        yield return Yields.WaitSeconds(stat.invincibilityTimeAfterDash);
        isInvincibility = false;
        yield return Yields.WaitSeconds(Mathf.Max(0, stat.dashCooldown - stat.invincibilityTimeAfterDash));
        renderer.color = originColor;
        canDash = true;
    }

    IEnumerator ParryDashSequence(Vector2 dashPos)
    {
        Color originColor = renderer.color;
        isDash = true;
        canDash = false;
        canParryDash = false;
        isInvincibility = true;
        renderer.color = Color.gray;
        float originGravityScale = rigid.gravityScale;
        rigid.gravityScale = 0f;
        rigid.velocity = new Vector2(transform.localScale.x * dashPos.x * stat.parryDashPower,
            transform.localScale.y * dashPos.y * stat.parryDashPower / 2);
        if (dashPos.y > 0)
        {
            isJump = true;
        }
        else
        {
            playerFaceRight = dashPos.x > 0 ? true : false;
        }
        ani.SetTrigger("dashTrigger");
        yield return Yields.WaitSeconds(dashPos.y > 0 ? stat.dashingTime / 3 : stat.dashingTime);
        rigid.gravityScale = originGravityScale;
        isDash = false;
        canDash = true;
        yield return Yields.WaitSeconds(stat.invincibilityTimeAfterDash);
        isInvincibility = false;
        renderer.color = originColor;
    }

    private void LightAttack(Vector2 mousePos)
    {
        if (canAttack)
        {
            ani.SetTrigger("attackTrigger");
            //StartCoroutine(AttackSequence(VectorTo4Direction(mousePos), false));
            StartCoroutine(AttackSequence(mousePos, false));
        }
    }

    private void HeavyAttack(Vector2 mousePos)
    {
        if (canAttack)
        {
            ani.SetTrigger("attackTrigger");
            //StartCoroutine(AttackSequence(VectorTo4Direction(mousePos), true));
            StartCoroutine(AttackSequence(mousePos, true));
        }
    }

    IEnumerator AttackSequence(Vector2 attackAngle, bool isHeavyAttack)
    {
        canAttack = false;
        isAttack = true;
        float angle;
        angle = VectorToEulerAngle(attackAngle) - 180;
        //if (attackAngle.x != 0)
        //{
        //    if(attackAngle.x > 0)
        //    {
        //        angle = 0;
        //    }
        //    else
        //    {
        //        angle = 180;
        //    }
        //}
        //else
        //{
        //    if (attackAngle.y > 0)
        //    {
        //        angle = 90;
        //    }
        //    else
        //    {
        //        angle = -90;
        //    }
        //}
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        Vector2 angleVec = new Vector2(attackAngle.x - transform.position.x, attackAngle.y - transform.position.y);

        lightAttack.AttackAble(angleVec.normalized, stat.lightAttackDamage, false, stat.lightCriticalAttackDamage);
        yield return Yields.WaitSeconds(!isHeavyAttack ? stat.lightAttackTime : stat.heavyAttackTime);
        lightAttack.AttackDisable();
        isAttack = false;
        yield return Yields.WaitSeconds(!isHeavyAttack ? stat.lightAttackCooldown : stat.heavyAttackCooldown);
        canAttack = true;
    }
    private float VectorToEulerAngle(Vector2 vec)
    {
        float horizontalValue = vec.x - transform.position.x;
        float VerticalValue = vec.y - transform.position.y;

        return Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg + 180;
    }
    private Vector2 VectorTo4Direction(Vector2 vec)
    {
        float euler = VectorToEulerAngle(vec);

        if (euler > 115 && euler <= 245)
        {
            return Vector2.right;
        }
        else if (euler > 245 && euler <= 295)
        {
            return Vector2.up;
        }
        else if (euler > 295 && euler <= 360 ||
            euler > 0 && euler <= 65)
        {
            return Vector2.left;
        }
        else if (euler > 65 && euler <= 115)
        {
            return Vector2.down;
        }
        else
        {
            return Vector2.zero;
        }
    }
    public void AfterAttack(Vector2 attackDir)
    {
        StartCoroutine(AfterAttackSequence(attackDir, 0.05f));
    }
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        ani.SetTrigger("hitTrigger");
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

        if (collision.collider.transform.position.y < transform.position.y)
        {
            ani.SetBool("onGround", true);
            isJump = false;
        }

        if (collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            if (isDash)
            {
                //TODO : Change to enemy abstract class
                collision.gameObject.GetComponent<TestEnemy>().Hit(stat.dashDamage, 
                    collision.transform.position - transform.position, false);
            }
        }

        if (collision.gameObject.CompareTag(PlayManager.ATTACK_TAG))
        {
            if (isDash)
            {
                canParryDash = true;
            }
        }
    }
}
