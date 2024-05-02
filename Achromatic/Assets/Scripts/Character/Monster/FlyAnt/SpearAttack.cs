using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpearAttack : Projectile
{
    public override void Shot(GameObject shotFrom, Vector2 from, Vector2 dir, float range, float speed, int dmg, bool isHeavy, float shotAngle, eActivableColor color)
    {
        base.Shot(shotFrom, from, dir, range, speed, dmg, isHeavy, shotAngle, color);

    }
    new public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.GetComponent<IAttack>()?.Hit(damage, moveDirection, isHeavyAttack);
            ReturnToPool();
        }
    }
}
