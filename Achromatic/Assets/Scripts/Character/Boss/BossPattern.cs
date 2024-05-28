using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossPattern : MonoBehaviour, IParryConditionCheck
{
    protected BossParent boss;
    protected eActivableColor patternColor;
    protected void PatternEnd()
    {
        boss.CurrentPatternEnd();
    }
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void DrawGizmos() { }

    public BossPattern SetBossPattern(BossParent boss)
    {
        this.boss = boss;
        patternColor = boss.GetBossStatus.bossColor;
        return this;
    }
    public BossPattern SetBossPattern(BossParent boss, eActivableColor color)
    {
        this.boss = boss;
        patternColor = color;
        return this;
    }
    public virtual bool CanParryAttack()
    {
        return PlayManager.Instance.ContainsActivationColors(patternColor);
    }
}
