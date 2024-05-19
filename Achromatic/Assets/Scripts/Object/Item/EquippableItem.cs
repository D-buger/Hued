using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/Items/EquippableItem")]
public abstract class EquippableItem : Item
{
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
}
