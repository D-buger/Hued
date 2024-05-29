using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/Items/ExpendableItem/Test")]
public class TestExpendableItem : ExpendableItem
{
    public override void UseItem()
    {
        PlayManager.Instance.GetPlayer.CurrentHP += 2;
    }
}
