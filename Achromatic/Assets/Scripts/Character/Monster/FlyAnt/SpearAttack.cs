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

    private bool isReturn = false;

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
        isReturn = true;
    }

    public void ReturnObject(GameObject obj)
    {
        Vector2 originalPos = obj.transform.position;
        Vector2 attackDir = (originalPos - (Vector2)transform.position).normalized;

        transform.Translate(attackDir * stat.stabThrowReturnSpeed * Time.deltaTime);

        if (Vector2.Distance(originalPos, (Vector2)transform.position) > stat.returnPosValue)
        {
            isReturn = false;
        }
    }
}
