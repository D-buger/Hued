using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour, IAttack
{
    private Rigidbody2D rigid;
    private BoxCollider2D coll;

    private GameObject attackPoint;
    private Attack lightAttack;
    private Attack heavyAttack;

    [SerializeField]
    private PlayerStat stat;
    public PlayerStat GetPlayerStat() => stat;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        attackPoint = transform.GetChild(0).gameObject;
        lightAttack = transform.GetChild(0).GetChild(0).GetComponent<Attack>();
        heavyAttack = transform.GetChild(0).GetChild(1).GetComponent<Attack>();
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
        heavyAttack.SetAttack(PlayManager.PLAYER_TAG, this);
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (stat.isDash || stat.isAttackRebound)
        {
            return;
        }

        rigid.velocity = new Vector2(stat.horizontalMove, rigid.velocity.y);

        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, new Vector3(0, -1 * (coll.size.y / 2 + stat.rayRange), 0), Color.red);

            RaycastHit2D rayHit =
                Physics2D.Raycast(rigid.position, Vector3.down, coll.size.y / 2 + stat.rayRange, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                stat.isJump = false;
            }
        }
        stat.isSit = false;
    }

    void Move(float dir)
    {
        if (stat.isDash || stat.isAttackRebound) 
        {
            return;
        }

        stat.playerFaceRight = dir > 0 ? true : false;
        if (!stat.isSit)
        {
            stat.horizontalMove = dir * stat.moveSpeed; 
        }
        else
        {
            stat.horizontalMove = dir * stat.moveSpeed * stat.sitDeceleration;
        }
    }

    void Jump()
    {
        if (!stat.isJump && stat.canJump)
        {
            StartCoroutine(JumpSequence());
        }
    }
    IEnumerator JumpSequence()
    {
        stat.isJump = true;
        stat.canJump = false;
        rigid.AddForce(Vector2.up * stat.jumpPower, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(stat.jumpCooldown);
        stat.canJump = true;
    }

    void Sit()
    {
        if (stat.isJump)
        {
            rigid.AddForce(Vector2.down * stat.sitDescentSpeed, ForceMode2D.Force);
        }
        else
        {
            stat.isSit = true;
        }
    }
    void Dash(Vector2 mousePos)
    {
        if (stat.canDash)
        {
            StartCoroutine(DashSequence(VectorTo4Direction(mousePos)));
        }
    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        stat.isDash = true;
        stat.canDash = false;
        stat.isInvincibility = true;
        float originGravityScale = rigid.gravityScale;
        rigid.gravityScale = 0f;
        rigid.velocity = new Vector2(transform.localScale.x * dashPos.x * stat.dashPower, 
            transform.localScale.y * dashPos.y * stat.dashPower / 2);
        if(dashPos.y > 0)
        {
            stat.isJump = true;
        }
        else
        {
            stat.playerFaceRight = dashPos.x > 0 ? true : false;
        }
        yield return Yields.WaitSeconds(dashPos.y > 0 ? stat.dashingTime / 3 : stat.dashingTime);
        rigid.gravityScale = originGravityScale;
        stat.isDash = false;
        yield return Yields.WaitSeconds(stat.invincibilityTimeafterDash);
        stat.isInvincibility = false;
        yield return Yields.WaitSeconds(Mathf.Max(0, stat.dashCooldown - stat.invincibilityTimeafterDash));
        stat.canDash = true;
    }

    private void LightAttack(Vector2 mousePos)
    {
        if (stat.canAttack)
        {
            //StartCoroutine(AttackSequence(VectorTo4Direction(mousePos), false));
            StartCoroutine(AttackSequence(mousePos, false));
        }
    }

    private void HeavyAttack(Vector2 mousePos)
    {
        if (stat.canAttack)
        {
            //StartCoroutine(AttackSequence(VectorTo4Direction(mousePos), true));
            StartCoroutine(AttackSequence(mousePos, true));
        }
    }

    IEnumerator AttackSequence(Vector2 attackAngle, bool isHeavyAttack)
    {
        stat.canAttack = false;
        stat.isAttack = true;
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
        if (!isHeavyAttack)
        {
            lightAttack.AttackAble(attackAngle, stat.lightAttackDamage, false);
        }
        else
        {
            heavyAttack.AttackAble(attackAngle, stat.heavyAttackDamage, true);
        }
        yield return Yields.WaitSeconds(!isHeavyAttack ? stat.lightAttackTime : stat.heavyAttackTime);
        lightAttack.AttackDisable();
        heavyAttack.AttackDisable();
        stat.isAttack = false;
        yield return Yields.WaitSeconds(!isHeavyAttack ? stat.lightAttackCooldown : stat.heavyAttackCooldown);
        stat.canAttack = true;
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
        StartCoroutine(AfterAttackSequence(attackDir));
    }
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack, int criticalDamage)
    {
        stat.PlayerHP -= damage;
        StartCoroutine(AfterAttackSequence(-attackDir));
    }
    IEnumerator AfterAttackSequence(Vector2 attackDir)
    {
        stat.isAttackRebound = true;
        rigid.AddForce(-attackDir * stat.attackReboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(0.1f);
        yield return Yields.WaitSeconds(stat.attackReboundTime);
        stat.isAttackRebound = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rigid.velocity = new Vector2(rigid.velocity.x, 0);
        if (collision.collider.transform.position.y < transform.position.y)
        {
            stat.isJump = false;
        }
    }

}
