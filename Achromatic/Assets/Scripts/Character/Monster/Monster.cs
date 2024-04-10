using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMove
{
    void WaitMove(MonsterStat stat, Vector2 sartPos, Vector2 endPos);
    void BettleMove(MonsterStat stat, Vector2 playerPos, Vector2 distancePos);
}
public class Monster : MonoBehaviour
{
    private SpriteRenderer render;
    private Animator anim;
    private Collider2D col;

    bool isDead = false;


    public void CheckDead(bool Dead, bool isAnim, int MonsterHP)
    {
        if (anim != null)
        {
            isAnim = true;
        }
        if (MonsterHP <= 0)
        {
            Dead = true;
        }
    }

    public void Dead()
    {
        anim.SetTrigger("deathTrigger");
        gameObject.SetActive(false);
    }
}
