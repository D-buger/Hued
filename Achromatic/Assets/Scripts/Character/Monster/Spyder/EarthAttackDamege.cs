using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthAttackDamege : MonoBehaviour
{
    [SerializeField]
    private SpyderMonsterStats stat;
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PlayManager.PLAYER_TAG))
        {
            if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.earthAttackDamege,
                transform.position - collision.transform.position, false, stat.earthAttackDamege);
            }
            else
            {
                collision.gameObject.GetComponent<Player>().Hit(stat.earthAttackDamege,
                transform.position - collision.transform.position, true, stat.earthAttackDamege);
            }
        }
    }
}
