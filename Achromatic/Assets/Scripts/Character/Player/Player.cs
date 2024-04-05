using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.Burst.CompilerServices;
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
    private TrailRenderer DashTrail;

    private GameObject attackPoint;
    private Attack attack;

    public CameraFollowObject CameraObject { get; set; }

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

    private bool isJump = false;
    private bool canJump = true;
    private bool isAttack = false;
    private bool canAttack = true;
    private bool isCriticalAttack = false;
    private bool isAttackRebound = false;
    private bool isDash = false;
    private bool isParryDash = false;
    private bool canDash = true;
    private bool canParryDash = true;
    private bool parryCondition = false;
    private bool isParry = false;
    private bool isInvincibility = false;
    private bool isHit = false;
    private bool onGround = false;

    private float horizontalMove = 0;
    private bool playerFaceRight = true;
    private ePlayerState state;

    private LayerMask groundLayer;
    private float bottomOffset = 0.105f;
    private float fallSpeedYDampingChangeThreshold;

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
        DashTrail = effects.transform.GetChild(1).GetComponent<TrailRenderer>();

        for (int i = 0; i < effects.transform.childCount; i++)
        {
            effects.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void Start()
    {
        InputManager.Instance.MoveEvent.AddListener(Move);
        InputManager.Instance.JumpEvent.AddListener(Jump);
        InputManager.Instance.DashEvent.AddListener(Dash);
        InputManager.Instance.LightAttackEvent.AddListener(LightAttack);

        attack.SetAttack(PlayManager.PLAYER_TAG, this);

        stat.currentHP = stat.playerHP;

        groundLayer = (1 << LayerMask.NameToLayer("Platform")) | (1 << LayerMask.NameToLayer("Object"));

        fallSpeedYDampingChangeThreshold = CameraManager.Instance.fallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        Turn();

        if(rigid.velocity.y < fallSpeedYDampingChangeThreshold
            && !CameraManager.Instance.IsLerpingYDamping
            && !CameraManager.Instance.LerpedFromPlayerFalling)
        {
            Debug.Log("Player fall");
            CameraManager.Instance.LerpYDamping(true);
        }

        if(rigid.velocity.y >= 0f && 
            !CameraManager.Instance.IsLerpingYDamping
            && CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpedFromPlayerFalling = false;

            CameraManager.Instance.LerpYDamping(false);
        }
    }

    private void FixedUpdate()
    {
        if (rigid.velocity.y != 0)
        {
            onGround = false;
            ani.SetBool("onGround", false);
        }

        if (isDash || isParryDash || isAttackRebound || isHit)
        {
            return;
        }

        rigid.velocity = new Vector2(horizontalMove, rigid.velocity.y);
    }

    void SetAnimation()
    {

    }

    void Turn()
    {
        if (playerFaceRight && transform.rotation.y == 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            CameraObject.CallTurn();
        }
        else if (!playerFaceRight && (transform.rotation.y == -1))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            CameraObject.CallTurn();
        }
    }

    void Move(float dir)
    {
        if (isDash || isParryDash || isAttackRebound || isHit) 
        {
            return;
        }

        ani.SetBool("isRunning", dir != 0 ? true : false);
        if (dir != 0)
        {
            playerFaceRight = dir > 0 ? true : false;
        }
        horizontalMove = dir * stat.moveSpeed; 
        
    }

    void Jump()
    {
        if (canJump && onGround && !isDash && !isParryDash && !isHit)
        {
            StartCoroutine(JumpSequence());
        }
    }
    IEnumerator JumpSequence()
    {
        canJump = false;
        isJump = true;
        rigid.AddForce(Vector2.up * stat.jumpPower, ForceMode2D.Impulse);
        ani.SetTrigger("jumpTrigger");
        isJump = false;
        yield return Yields.WaitSeconds(stat.jumpCooldown);
        canJump = true;
    }

    void Dash(Vector2 mousePos)
    {
        if (!isDash && !isParryDash && canAttack && !isHit)
        {
            if (isParry && canParryDash)
            {
                StartCoroutine(ParryDashSequence(mousePos));
            }
            else if(canDash)
            {
                StartCoroutine(DashSequence(mousePos));
            }
        }
    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        isDash = true;
        canDash = false;
        canParryDash = false;

        float originGravityScale = rigid.gravityScale;
        float originLiniearDrag = rigid.drag;
        float originMass = rigid.mass;
        rigid.gravityScale = 0f;
        rigid.drag = 0;
        rigid.mass = 0;
        DashTrail.Clear();
        DashTrail.gameObject.SetActive(true);

        dashPos.x = dashPos.x - transform.position.x;
        dashPos.y = dashPos.y - transform.position.y;

        rigid.velocity = dashPos.normalized * stat.dashPower;

        if (dashPos.y > 0)
        {
            onGround = false;
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
        DashTrail.gameObject.SetActive(false);
        ani.SetTrigger("dashEndTrigger");
        if (isParry)
        {
            StartCoroutine(ParrySequence());
        }
        parryCondition = false;

        yield return Yields.WaitSeconds(stat.dashAfterDelay);
        canParryDash = true;

        yield return Yields.WaitSeconds(stat.dashCooldown - stat.dashAfterDelay);
        canDash = true;
    }

    IEnumerator ParryDashSequence(Vector2 dashPos)
    {
        isParry = false;
        isParryDash = true;
        canParryDash = false;
        isCriticalAttack = true;

        float originGravityScale = rigid.gravityScale;
        float originLiniearDrag = rigid.drag;
        float originMass = rigid.mass;
        coll.forceReceiveLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        coll.forceSendLayers &= ~(1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        rigid.gravityScale = 0f;
        rigid.drag = 0;
        rigid.mass = 0;
        DashTrail.Clear();
        DashTrail.gameObject.SetActive(true);

        dashPos.x = dashPos.x - transform.position.x;
        dashPos.y = dashPos.y - transform.position.y;

        rigid.velocity = dashPos.normalized * stat.parryDashPower;

        if (dashPos.y > 0)
        {
            onGround = false;
            ani.SetBool("onGround", false);
        }
        ani.SetTrigger("dashTrigger");
        playerFaceRight = dashPos.x > 0 ? true : false;

        yield return Yields.WaitSeconds(stat.parryDashTime);
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = originGravityScale;
        rigid.drag = originLiniearDrag;
        rigid.mass = originMass;

        if (null != parryDashCollision)
        {
            float horDistance = transform.position.x - parryDashCollision.collider.bounds.center.x;
            if(horDistance < 0)
            {
                transform.position = new Vector3(parryDashCollision.collider.bounds.min.x - stat.parryDashDistance, 
                    transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(parryDashCollision.collider.bounds.max.x + stat.parryDashDistance,
                    transform.position.y, transform.position.z);
            }
            parryDashCollision = null;
        }
        coll.forceReceiveLayers |= (1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));
        coll.forceSendLayers |= (1 << LayerMask.NameToLayer(PlayManager.ENEMY_TAG));

        isParryDash = false;
        DashTrail.gameObject.SetActive(false);

        yield return Yields.WaitSeconds(stat.dashAfterDelay);
        canParryDash = true;
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

        if (!isCriticalAttack)
        {
            attack.AttackAble(angleVec.normalized, stat.attackDamage, false, stat.colorAttackDamage);
        }
        else
        {
            isCriticalAttack = false;
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
        if (!isDash && !isParryDash)
        {
            StartCoroutine(AttackReboundSequence(attackDir.normalized, stat.attackReboundPower, stat.attackReboundTime, 0.05f));
        }
    }
    IEnumerator AttackReboundSequence(Vector2 dir, float reboundPower, float reboundTime, float shockAmount)
    {
        isAttackRebound = true;
        rigid.velocity = Vector2.zero;
        rigid.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(shockAmount);
        yield return Yields.WaitSeconds(reboundTime);
        isAttackRebound = false;
    }

    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        if (isDash || isParryDash)
        {
            return;
        }

        horizontalMove = 0;
        rigid.velocity = Vector2.zero;
        parryCondition = false;
        if (!isInvincibility)
        {
            attackDir.y = 0;
            currentHP -= damage;
            ani.SetTrigger("hitTrigger");
            StartCoroutine(HitReboundSequence(attackDir.normalized, stat.hitReboundPower, stat.hitReboundTime, 0.1f));
        }
    }

    IEnumerator HitReboundSequence(Vector2 dir, float reboundPower, float reboundTime, float shockAmount)
    {
        isHit = true;
        isInvincibility = true;
        rigid.AddForce(-dir * reboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(shockAmount);
        yield return Yields.WaitSeconds(reboundTime);

        yield return Yields.WaitSeconds(stat.hitBehaviourLimitTime);
        isHit = false;
        yield return Yields.WaitSeconds(Mathf.Max(0, Mathf.Abs(stat.hitInvincibilityTime - stat.hitBehaviourLimitTime)));
        isInvincibility = false;
    }

    private void Dead()
    {
        Debug.Log("Player Dead");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            if (isDash || isParryDash)
            {
                int damage = isParryDash ? stat.parryDashDamage : stat.dashDamage;
                collision.gameObject.GetComponent<TestEnemy>().Hit(damage, 
                    collision.transform.position - transform.position, false, damage);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if ((groundLayer & (1 << collision.gameObject.layer)) != 0 &&
            collision.contacts[0].point.y > collision.collider.bounds.center.y)
        {
            onGround = true;
            ani.SetBool("onGround", true);
        }

        if (isParryDash && collision.gameObject.CompareTag(PlayManager.ENEMY_TAG))
        {
            parryDashCollision = collision;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isParryDash)
        {
            parryDashCollision = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDash && collision.CompareTag(PlayManager.ATTACK_TAG))
        {
            if (collision.GetComponent<Attack>().isCanParryAttack(PlayManager.PLAYER_TAG))
            {
                parryCondition = true;
            }
        }

    }
}
