using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour, iAttack
{
    private Rigidbody2D rigid;
    private BoxCollider2D coll;

    private GameObject attackPoint;
    private Attack lightAttack;
    private Attack heavyAttack;

    [Space(10), SerializeField, Header("Move")]
    private float moveSpeed = 1;

    [Space(10), SerializeField, Header("Jump")]
    private float jumpPower = 5;
    [SerializeField]
    private float jumpCooldown = 0.1f;

    [Space(10), SerializeField, Header("Sit")]
    private float sitDeceleration = 0.7f;
    [SerializeField]
    private float sitDescentSpeed = 1.5f;

    [Space(10), SerializeField, Header("Dash")]
    private float dashPower = 5;
    [SerializeField]
    private float dashingTime = 1;
    [SerializeField]
    private float dashCooldown = 2;

    [Space(10), SerializeField, Header("Attack")]
    private float attackCooldown = 1;
    [SerializeField]
    private float attackReboundPower = 1;
    [SerializeField]
    private float attackReboundTime = 0.05f;
    [Space(5), SerializeField]
    private float lightAttackTime = 1;
    [SerializeField]
    private int lightAttackDamage = 1;
    [Space(5), SerializeField]
    private float heavyAttackTime = 1;
    [SerializeField]
    private int heavyAttackDamage = 2;


    private bool isSit = false;
    private bool isJump = false;
    private bool canJump = true;
    private bool isAttack = false;
    private bool canAttack = true;
    private bool isAttackRebound = false;
    private bool isDash = false;
    private bool canDash = true;

    private float rayRange = 0.1f;
    private float horizontalMove = 0;
    //-1 : Left, 1 : Right
    private float playerFace = 1;   
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
        if (isDash || isAttackRebound)
        {
            return;
        }

        rigid.velocity = new Vector2(horizontalMove, rigid.velocity.y);

        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, new Vector3(0, -1 * (coll.size.y / 2 + rayRange), 0), Color.red);

            RaycastHit2D rayHit =
                Physics2D.Raycast(rigid.position, Vector3.down, coll.size.y / 2 + rayRange, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                isJump = false;
            }
        }
        isSit = false;
    }

    void Move(float dir)
    {
        if (isDash || isAttackRebound) 
        {
            return;
        }

        playerFace = dir;
        if (!isSit)
        {
            horizontalMove = dir * moveSpeed; 
        }
        else
        {
            horizontalMove = dir * moveSpeed * sitDeceleration;
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
        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        yield return Yields.WaitSeconds(jumpCooldown);
        canJump = true;
    }

    void Sit()
    {
        if (isJump)
        {
            rigid.AddForce(Vector2.down * sitDescentSpeed, ForceMode2D.Force);
        }
        else
        {
            isSit = true;
        }
    }
    void Dash(Vector2 mousePos)
    {
        if (canDash)
        {
            StartCoroutine(DashSequence(VectorTo4Direction(mousePos)));
        }
    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        isDash = true;
        canDash = false;
        float originGravityScale = rigid.gravityScale;
        rigid.gravityScale = 0f;
        rigid.velocity = new Vector2(transform.localScale.x * dashPos.x * dashPower, transform.localScale.y * dashPos.y * dashPower / 2);
        if(dashPos.y > 0)
        {
            isJump = true;
        }
        else
        {
            playerFace = dashPos.x;
        }
        yield return Yields.WaitSeconds(dashPos.y > 0 ? dashingTime / 3 : dashingTime);
        rigid.gravityScale = originGravityScale;
        isDash = false;
        yield return Yields.WaitSeconds(dashCooldown);
        canDash = true;
    }

    private void LightAttack(Vector2 mousePos)
    {
        if (canAttack)
        {
            StartCoroutine(AttackSequence(VectorTo4Direction(mousePos), true));
        }
    }

    private void HeavyAttack(Vector2 mousePos)
    {
        if (canAttack)
        {
            StartCoroutine(AttackSequence(VectorTo4Direction(mousePos), false));
        }
    }

    IEnumerator AttackSequence(Vector2 attackAngle, bool isLightAttack)
    {
        canAttack = false;
        isAttack = true;
        float angle;
        if (attackAngle.x != 0)
        {
            if(attackAngle.x > 0)
            {
                angle = 0;
            }
            else
            {
                angle = 180;
            }
        }
        else
        {
            if (attackAngle.y > 0)
            {
                angle = 90;
            }
            else
            {
                angle = -90;
            }
        }
        attackPoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        if (isLightAttack)
        {
            lightAttack.AttackAble(attackAngle);
        }
        else
        {
            heavyAttack.AttackAble(attackAngle);
        }
        yield return Yields.WaitSeconds(isLightAttack ? lightAttackTime : heavyAttackTime);
        lightAttack.AttackDisable();
        heavyAttack.AttackDisable();
        isAttack = false;
        yield return Yields.WaitSeconds(attackCooldown);
        canAttack = true;
    }
    private Vector2 VectorTo4Direction(Vector2 vec)
    {
        float horizontalValue = vec.x - transform.position.x;
        float VerticalValue = vec.y - transform.position.y;

        float euler = Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg + 180;

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

    IEnumerator AfterAttackSequence(Vector2 attackDir)
    {
        isAttackRebound = true;
        rigid.AddForce(-attackDir * attackReboundPower, ForceMode2D.Impulse);
        PlayManager.Instance.cameraManager.ShakeCamera(0.1f);
        yield return Yields.WaitSeconds(attackReboundTime);
        isAttackRebound = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rigid.velocity = new Vector2(rigid.velocity.x, 0);
        if (collision.collider.transform.position.y < transform.position.y)
        {
            isJump = false;
        }
    }
}
