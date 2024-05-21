using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquippableItem : Item
{
    [HideInInspector]
    public bool isEquipped = false;

    public virtual void EquipItem()
    {
        isEquipped = true;
        AffectStat(isEquipped);
    }

    public virtual void DisarmItem()
    {
        isEquipped = false;
        AffectStat(isEquipped);
    }

    public abstract void AffectStat(bool equipped);
    public override EItemType ItemType() => EItemType.EQUIPPABLE;
    protected override void OnEnable()
    {
        base.OnEnable();

    }
}
