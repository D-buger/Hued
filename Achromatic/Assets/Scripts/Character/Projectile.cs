using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TerrainUtils;

public class Projectile : MonoBehaviour, IParryConditionCheck
{
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;

    private Vector2 moveDirection = Vector2.zero;
    private float moveSpeed = 1f;
    private float moveRange = 5f;
    private int damage = 1;

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


    public virtual void Shot(GameObject shotFrom, Vector2 from, Vector2 dir, float range, float speed, int dmg, float shotAngle, eActivableColor color)
    {
        float shotDir = shotAngle;
        attackFrom = shotFrom;
        transform.position = from;
        moveDirection = dir;
        moveSpeed = speed;
        damage = dmg;
        moveRange = range;
        fromVector = shotFrom.transform.position;
        enemyColor = color;
        transform.rotation = Quaternion.Euler(1, 1, shotDir);
        rigid.AddForce(moveDirection * moveSpeed);
        gameObject.SetActive(true);
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
            collision.GetComponent<IAttack>()?.Hit(damage, damage, -moveDirection, this);
            ReturnToPool();
        }
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(enemyColor);
    }
}
