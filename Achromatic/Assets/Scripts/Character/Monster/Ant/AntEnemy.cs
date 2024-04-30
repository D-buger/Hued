using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpiderEnemy;

public class AntEnemy : Monster, IAttack
{
    private MonsterFSM fsm;

    private Rigidbody2D rigid;
    private GameObject attackPoint;
    private Attack meleeAttack;
    [SerializeField]
    private AntMonsterStat stat;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Attack()
    {
        throw new System.NotImplementedException();
    }
    public override void Dead()
    {
        throw new System.NotImplementedException();
    }
    public override void CheckStateChange()
    {
        if (isWait)
        {
            fsm.ChangeState("Idle");
        }
        if (isPlayerBetween)
        {
            fsm.ChangeState("Chase");
        }
        if (isBattle)
        {
            fsm.ChangeState("Attack");
        }
    }
    public override void Hit(int damage, int colorDamage, Vector2 attackDir, IParryConditionCheck parryCheck)
    {
        if (PlayManager.Instance.ContainsActivationColors(stat.enemyColor))
        {
            HPDown(colorDamage);
            rigid.AddForce(attackDir * stat.heavyHitReboundPower, ForceMode2D.Impulse);
        }
        else
        {
            HPDown(damage);
            rigid.AddForce(attackDir * stat.hitReboundPower, ForceMode2D.Impulse);
        }

        if (!isDead)
        {
            CheckDead();
        }
    }
}
