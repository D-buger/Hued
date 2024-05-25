using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthAttackDamege : MonoBehaviour, IParryConditionCheck
{
    [SerializeField]
    private SpiderMonsterStats stat;

    public bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(stat.enemyColor);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            collision.gameObject.GetComponent<Player>().Hit(stat.earthAttackDamage, stat.earthAttackDamage,
            transform.position - collision.transform.position, this);
        }
    }
}
