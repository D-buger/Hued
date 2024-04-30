using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingPattern : BossPattern
{
    public ThrowingPattern(BossParent boss) : base(boss)
    {
    }
    public override void OnStart()
    {

    }
    public override void OnUpdate()
    {

    }
    public override bool CanParryAttack()
    {
        return true;
    }
}
