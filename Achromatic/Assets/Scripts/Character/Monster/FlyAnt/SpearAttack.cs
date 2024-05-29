using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpearAttack : Projectile
{
    [SerializeField]
    private FlyAntMonsterStat stat;
    [SerializeField]
    private GameObject flyAntMonster;
    private Rigidbody2D rigid2D;

    private Vector2 PlayerPos => PlayManager.Instance.GetPlayer.transform.position;

    [SerializeField]
    private bool isReturn = false;

    private void OnEnable()
    {
        isReturn = false;
    }

    private void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (isReturn)
        {
            ReturnObject(flyAntMonster);
        }
    }
    public override void Shot(GameObject shotFrom, Vector2 from, Vector2 dir, float range, float speed, int dmg, float shotAngle, eActivableColor color)
    {
        base.Shot(shotFrom, from, dir, range, speed, dmg, shotAngle, color);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Floor"))
        {
            isReturn = true;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        Vector2 originalPos = obj.transform.position;

        rigid2D.AddForce(originalPos * stat.spearThrowReturnSpeed);

        if (Vector2.Distance(originalPos, (Vector2)transform.position) > stat.returnPosValue)
        {
            isReturn = false;
        }
    }
}
