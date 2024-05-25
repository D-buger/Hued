using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemBoss : BossParent
{
    [SerializeField]
    private MovingCrystal[] crystals;
    public override int CurrentHp
    {
        get
        {
            return currentHp;
        }
        set
        {
            base.CurrentHp = value;
            if(currentHp <= GetBossStatus.maxHp * 0.2f)
            {
                for(int i = 0; i < crystals.Length; i++)
                {
                    crystals[i].ChangeSpeed();
                }
            }
        }
    }
 
    protected override void OnAwake()
    {
        for (int i = 0; i < crystals.Length; i++)
        {
            crystals[i].gameObject.SetActive(false);
        }
    }
    protected override void OnUpdate()
    {

    }

    public override void OnChangePhaseBehaviour()
    {
        Debug.Log("페이즈 전환");
        for(int i = 0; i < crystals.Length; i++)
        {
            crystals[i].gameObject.SetActive(true);
        }
    }
}
