using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class BossPattern
{
    protected BossParent boss;
    public BossPattern(BossParent boss)
    {
        this.boss = boss;
    }
    public virtual void Start() { }
    public virtual void Update() { }

    public virtual void OnDrawGizmos() { }
    protected void PatternEnd()
    {
        boss.CurrentPatternEnd();
    }


}
