using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabAttack : MonoBehaviour
{
    private Collider2D col;
    [SerializeField]
    private AntMonsterStat stat;
    [SerializeField]
    private bool lastAttack = false;

    private void Start()
    {
        col = GetComponent<Collider2D>();
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (!lastAttack)
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.stabAttackDamage, stat.stabAttackDamage,
                transform.position - collision.transform.position, null);
            }
            else
            {
                if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
                {
                    collision.gameObject.GetComponent<Player>().Hit(stat.lastStabAttackDamage,
                    stat.lastStabAttackDamage, transform.position - collision.transform.position, null);
                }
                else
                {
                    collision.gameObject.GetComponent<Player>().Hit(stat.lastStabAttackDamage,
                    stat.lastStabAttackDamage, transform.position - collision.transform.position, null);
                }
            }
        }

    }
}
