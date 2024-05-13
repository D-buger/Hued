using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAuraPattern : BossPattern
{
    [SerializeField]
    private float moveToPlayerSpeed = 2;
    [SerializeField]
    private float stopDistance = 2;
    [SerializeField]
    private float slashAttackDistance = 2;
    [SerializeField]
    private float slashAttackSpeed = 1.5f;
    [SerializeField]
    private int slashAttackDamage = 15;
    [SerializeField]
    private float swordAuraChargingTime = 0.6f;
    [Space(10)]
    [SerializeField]
    private float swordAuraMoveSpeed = 0.3f;
    [SerializeField]
    private int swordAuraDamage = 10;

    public override void OnStart()
    {

    }

    public override void OnUpdate()
    {

    }
    

    public override bool CanParryAttack()
    {
        return base.CanParryAttack();
    }

    public override void DrawGizmos()
    {

    }
}
