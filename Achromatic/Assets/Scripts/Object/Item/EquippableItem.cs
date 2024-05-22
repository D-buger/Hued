using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquippableItem : Item
{
    public abstract void EquipItem();
    public abstract void DisarmItem();
    public override EItemType ItemType() => EItemType.EQUIPPABLE;
    protected override void OnEnable()
    {
        base.OnEnable();

    }
}
