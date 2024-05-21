using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestExpendableItem : ExpendableItem
{
    public override void UseItem()
    {
        PlayManager.Instance.GetPlayer.CurrentHP += 50;
    }
}
