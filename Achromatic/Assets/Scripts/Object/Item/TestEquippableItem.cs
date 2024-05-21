using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/Items/EquippableItem/Test")]
public class TestEquippableItem : EquippableItem
{
    public override void AffectStat(bool equipped)
    {
        PlayManager.Instance.GetPlayer.MaxHP += 2;
    }
}
