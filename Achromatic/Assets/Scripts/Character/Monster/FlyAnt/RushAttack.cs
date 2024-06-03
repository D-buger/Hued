using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushAttack : MonoBehaviour, IParryConditionCheck
{
    private Collider2D col;
    [SerializeField]
    private FlyAntMonsterStat stat;

    private void Start()
    {
        col = GetComponent<Collider2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.rushAttackDamage,
            stat.rushAttackDamage, transform.position - collision.transform.position, this);
        }
    }
    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(stat.enemyColor);
    }
}
