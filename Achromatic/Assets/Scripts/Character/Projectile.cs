using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TerrainUtils;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;

    public Vector2 moveDirection = Vector2.zero;
    private float moveSpeed = 1f;
    private float moveRange = 5f;
    public int damage = 1;

    public bool isHeavyAttack = true;
    public bool IsParryAllow => (!isHeavyAttack);

    private GameObject attackFrom;
    private Vector2 fromVector;
    private eActivableColor enemyColor;

    private bool isShooting = false;
    private bool isParried = false;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
    }

    public void CheckIsHeavyAttack(eActivableColor color)
    {
        if (color == enemyColor)
        {
            isHeavyAttack = false;
            renderer.color = Color.white;
        }
        else
        {
            isHeavyAttack = true;
            renderer.color = Color.black;
        }
    }

    public virtual void Shot(GameObject shotFrom, Vector2 from, Vector2 dir, float range, float speed, int dmg, bool isHeavy, float shotAngle, eActivableColor color)
    {
        float spitDir = shotAngle + 180;
        attackFrom = shotFrom;
        transform.position = from;
        moveDirection = dir;
        moveSpeed = speed;
        isHeavyAttack = isHeavy;
        damage = dmg;
        moveRange = range;
        fromVector = shotFrom.transform.position;
        enemyColor = color;
        transform.rotation = Quaternion.Euler(1, 1, spitDir);
        rigid.AddForce(moveDirection * moveSpeed);
        gameObject.SetActive(true);


        isHeavyAttack = (PlayManager.Instance.ContainsActivationColors(enemyColor)) ? false : true;
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
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
    public void ReturnStartRoutine(float delayTime)
    {
        StartCoroutine(TimeToReturnObject(delayTime));
    }
    public IEnumerator TimeToReturnObject(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.GetComponent<IAttack>()?.Hit(damage, moveDirection, isHeavyAttack);
            ReturnToPool();
        }
    }
}
