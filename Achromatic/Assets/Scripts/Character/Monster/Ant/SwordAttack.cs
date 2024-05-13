using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour, IParryConditionCheck
{
    private Collider2D col;
    [SerializeField]
    private AntMonsterStat stat;

    private void Start()
    {
        col = GetComponent<Collider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.cuttingAttackDamage,
                stat.cuttingAttackDamage, transform.position - collision.transform.position, this);
            }
            else
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.cuttingAttackDamage,
                stat.cuttingAttackDamage, transform.position - collision.transform.position, null);
            }
            gameObject.SetActive(false);
        }
    }

    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(stat.enemyColor);
    }
}
