using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rigid;

    private Vector2 moveDirection = Vector2.zero;
    private float moveSpeed = 1f;
    private float moveRange = 5f;
    private int damage = 1;

    private bool isHeavyAttack;
    public bool IsParryAllow => (!isHeavyAttack);

    private GameObject attackFrom;
    private Vector2 fromVector;

    private bool isShooting = false;
    private bool isParried = false;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isShooting)
        {
            if(Vector2.Distance(fromVector, new Vector2(transform.position.x, transform.position.y)) > moveRange)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Shot(GameObject shotFrom, Vector2 from, Vector2 dir, float range, float speed, int dmg, bool isHeavy)
    {
        attackFrom = shotFrom;
        transform.position = from;
        moveDirection = dir;
        moveSpeed = speed;
        isHeavyAttack = isHeavy;
        damage = dmg;
        moveRange = range;
        fromVector = shotFrom.transform.position;
        rigid.AddForce(moveDirection * moveSpeed);
    }

    public void Parried(GameObject shotFrom, Vector2 dir, int dmg)
    {
        if (!isParried)
        {
            attackFrom = shotFrom;
            moveDirection = dir;
            damage = dmg;
            fromVector = shotFrom.transform.position;
            isParried = true;
            rigid.velocity = Vector2.zero;
            rigid.AddForce(moveDirection * moveSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(attackFrom) && !collision.CompareTag(PlayManager.ATTACK_TAG))
        {
            collision.GetComponent<IAttack>()?.Hit(damage, moveDirection, false);
            Destroy(gameObject);
        }
    }
}
