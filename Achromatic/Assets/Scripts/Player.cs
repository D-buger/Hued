using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigid;
    private BoxCollider2D coll;

    [Space(10), SerializeField, Header("Move")]
    private float moveSpeed = 1;

    [Space(10), SerializeField, Header("Jump")]
    private float jumpPower = 5;

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

    [SerializeField]
    private bool isJump = false;
    private bool isSit = false;
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
    }

    void Start()
    {
        InputManager.Instance.MoveEvent.AddListener(Move);
        InputManager.Instance.JumpEvent.AddListener(Jump);
        InputManager.Instance.SitEvent.AddListener(Sit);
        InputManager.Instance.DashEvent.AddListener(Dash);
    }

    void Update()
    {
      
    }

    private void FixedUpdate()
    {
        if (isDash)
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
        if (isDash) 
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
        if (!isJump)
        {
            isJump = true;
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
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
            isJump = true;
            float horizontalValue = mousePos.x - transform.position.x;
            float VerticalValue = mousePos.y - transform.position.y;

            float euler = Mathf.Atan2(VerticalValue, horizontalValue) * Mathf.Rad2Deg + 180;
            Vector2 dashPos = new Vector2(0, 0);

            if (euler > 135 && euler <= 225)         
            {
                dashPos = Vector2.right;
            }
            else if(euler > 225 && euler <= 315)   
            {
                dashPos = Vector2.up;
            }
            else if(euler > 315 && euler <= 360 ||
                euler > 0 && euler <= 45)         
            {
                dashPos = Vector2.left;
            }
            else if(euler > 45 && euler <= 135)    
            {
                dashPos = Vector2.down;
            }

            StartCoroutine(DashSequence(dashPos));
        }
    }

    IEnumerator DashSequence(Vector2 dashPos)
    {
        isDash = true;
        canDash = false;
        float originGravityScale = rigid.gravityScale;
        rigid.gravityScale = 0f;
        rigid.velocity = new Vector2(transform.localScale.x * dashPos.x * dashPower, transform.localScale.y * dashPos.y * dashPower / 2);
        yield return Yields.WaitSeconds(dashPos.y > 0 ? dashingTime / 3 : dashingTime);
        rigid.gravityScale = originGravityScale;
        isDash = false;
        yield return Yields.WaitSeconds(dashCooldown);
        canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.position.y < transform.position.y)
        {
            isJump = false;
        }
    }

    private void LightAttack()
    {

    }

    private void HeavyAttack()
    {

    }
}
