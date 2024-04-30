using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossPattern : IParryConditionCheck
{
    protected BossParent boss;
    public BossPattern(BossParent boss)
    {
        this.boss = boss;
    }
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }

    protected void PatternEnd()
    {
        boss.CurrentPatternEnd();
    }

    public virtual bool CanParryAttack()
    {
        return false;
    }
}
