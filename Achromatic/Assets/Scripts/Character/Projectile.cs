using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private SpyderEnemy spyderEnemy;
    private Rigidbody2D rigid;

    private Vector2 moveDirection = Vector2.zero;
    private float moveSpeed = 1f;
    private float moveRange = 5f;
    private int damage = 1;

    private bool isHeavyAttack = true;
    public bool IsParryAllow => (!isHeavyAttack);

    private GameObject attackFrom;
    private Vector2 fromVector;
    private eActivableColor enemyColor;

    private bool isShooting = false;
    private bool isParried = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }


    /*private void Update()
    {
        if (isShooting)
        {
            if(Vector2.Distance(fromVector, new Vector2(transform.position.x, transform.position.y)) > moveRange)
            {
                Destroy(gameObject);
            }
        }
    }*/

    public void CheckIsHeavyAttack(eActivableColor color)
    {
        if (color == enemyColor)
        {
            isHeavyAttack = false;
        }
        else
        {
            isHeavyAttack = true;
        }
    }

    public void Shot(GameObject shotFrom, Vector2 from, Vector2 dir, float range, float speed, int dmg, bool isHeavy, eActivableColor color)
    {
        spyderEnemy?.spyderColorEvent.AddListener(CheckIsHeavyAttack);

        attackFrom = shotFrom;
        transform.position = from;
        moveDirection = dir;
        moveSpeed = speed;
        isHeavyAttack = isHeavy;
        damage = dmg;
        moveRange = range;
        fromVector = shotFrom.transform.position;
        enemyColor = color;
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
            isHeavyAttack = true;
            rigid.velocity = Vector2.zero;
            rigid.AddForce(moveDirection * moveSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.Equals(attackFrom) && !collision.CompareTag(PlayManager.ATTACK_TAG) && collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.GetComponent<IAttack>()?.Hit(damage, moveDirection, isHeavyAttack);
            Destroy(gameObject);
        }
    }
}
