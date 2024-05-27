using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/Items/EquippableItem/Test")]
public class TestEquippableItem : EquippableItem
{
    public override void EquipItem()
    {
        PlayManager.Instance.GetPlayer.MaxHP += 2;
    }

    public override void DisarmItem()
    {
        PlayManager.Instance.GetPlayer.MaxHP -= 2;
    }

}
