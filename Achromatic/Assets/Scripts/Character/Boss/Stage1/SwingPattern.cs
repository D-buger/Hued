using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingPattern : BossPattern
{
    [SerializeField]
    private float swingRange = 2.5f;

    [SerializeField]
    private float swingFirstHandSpeed = 1.2f;
    [SerializeField]
    private float swingSecondHandSpeed = 0.7f;

    [SerializeField]
    private float swingDelay = 0.5f;

    [SerializeField]
    private int damage = 5;

    public SwingPattern(BossParent boss) : base(boss)
    {

    }
    public override void OnStart()
    {

    }
    public override void OnUpdate()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boss.transform.position, new Vector2(swingRange, swingRange), 0, Vector2.left, swingFirstHandSpeed);

        if(!ReferenceEquals(hit, null))
        {

        }
    }
    public override bool CanParryAttack()
    {
        return true;
    }
}
