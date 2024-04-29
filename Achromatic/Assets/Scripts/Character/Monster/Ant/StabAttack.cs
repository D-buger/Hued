using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabAttack : MonoBehaviour
{
    [SerializeField]
    private AntMonsterStat stat;
    [SerializeField]
    private bool lastAttack = false;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (!lastAttack)
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.stabAttackDamage,
                transform.position - collision.transform.position, true, stat.stabAttackDamage);
            }
            else
            {
                if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
                {
                    collision.gameObject.GetComponent<Player>().Hit(stat.lastStabAttackDamage,
                    transform.position - collision.transform.position, false, stat.lastStabAttackDamage);
                }
                else
                {
                    collision.gameObject.GetComponent<Player>().Hit(stat.lastStabAttackDamage,
                    transform.position - collision.transform.position, true, stat.lastStabAttackDamage);
                }
            }
        }

    }
}
