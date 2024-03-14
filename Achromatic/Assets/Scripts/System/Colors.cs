using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colors : Item
{
    [SerializeField]
    eActivableColor color;

    protected override void TriggerEnterBehaviour()
    {
        PlayManager.Instance.ActivationColors = color;
    }
}
