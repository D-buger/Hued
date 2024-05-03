using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthAttackDamege : MonoBehaviour
{
    [SerializeField]
    private SpiderMonsterStats stat;
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.earthAttackDamege, stat.earthAttackDamege,
                transform.position - collision.transform.position);
            }
            else
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.earthAttackDamege, stat.earthAttackDamege,
                transform.position - collision.transform.position, null);
            }
        }
    }
}
