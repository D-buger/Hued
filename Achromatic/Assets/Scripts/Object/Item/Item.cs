using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EItemType
{
    EQUIPPABLE,
    EXPENDABLE
}

public abstract class Item : ScriptableObject
{
    public Sprite itemSprite;
    public string itemName;
    public string itemExplanation;
    public bool isEquipped = false;
    public abstract EItemType ItemType();

    protected virtual void OnEnable()
    {
        isEquipped = false;
    }
}
