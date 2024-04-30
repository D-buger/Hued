using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShotLaserPattern : BossPattern
{
    public ShotLaserPattern(BossParent boss) : base(boss)
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
