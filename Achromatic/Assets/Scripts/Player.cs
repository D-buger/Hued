using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigid;
    private BoxCollider2D coll;

    private bool isJump = false;
    private bool isSit = false;

    [SerializeField]
    private float moveSpeed = 1;
    [SerializeField]
    private float jumpPower = 5;
    [SerializeField]
    private float sitDeceleration = 0.7f;
    [SerializeField]
    private float sitDescentSpeed = 1.5f;

    private float rayRange = 0.1f;
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
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {

        if (isJump && rigid.velocity.y < 0)
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
        if (!isSit)
        {
            transform.position = new Vector2(transform.position.x + (dir * moveSpeed * Time.deltaTime), transform.position.y);
        }
        else
        {
            transform.position = new Vector2(transform.position.x + (dir * moveSpeed * sitDeceleration * Time.deltaTime), transform.position.y);
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
            rigid.AddForce(Vector2.down * sitDescentSpeed, ForceMode2D.Impulse);
        }
        else
        {
            isSit = true;
        }
    }

}
