using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour, IAttack
{
    private Rigidbody2D rigid;

    [SerializeField]
    private int enemyHp = 5;

    [SerializeField]
    private eActivableColor enemyColor;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void AfterAttack(Vector2 attackDir)
    {

    }

    // 임시 테스트 코드
    public void Hit(int damage, Vector2 attackDir, bool isHeavyAttack)
    {
        if (!isHeavyAttack)
        {
            enemyHp -= damage;
            rigid.AddForce(attackDir * 5, ForceMode2D.Impulse);
            CheckDead();
        }
        else
        {
            if (PlayManager.Instance.ContainsActivationColors(enemyColor))
            {
                enemyHp -= damage;
                rigid.AddForce(attackDir * 10, ForceMode2D.Impulse);
                CheckDead();
            }
        }
    }

    private void CheckDead()
    {
        if (enemyHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
