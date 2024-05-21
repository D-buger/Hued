using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/Items/ExpendableItem")]
public abstract class ExpendableItem : Item
{
    public int maxCount = 5;
    public int currentCount = 5;

    public bool isDiscovered = false;

    public abstract void UseItem();
    public override EItemType ItemType() => EItemType.EXPENDABLE;
    protected override void OnEnable()
    {
        base.OnEnable();
        currentCount = maxCount;
        isDiscovered = false;
    }
}
