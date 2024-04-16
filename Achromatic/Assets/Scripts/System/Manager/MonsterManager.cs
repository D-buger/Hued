using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class MonsterManager : SingletonBehavior<MonsterManager>
{
    public UnityEvent<eActivableColor> getColorEvent;
    protected override void OnAwake()
    {
        PlayManager.Instance.FilterColorAttackEvent.AddListener(CheckGetColorEvent);
        PlayManager.Instance.ActivationColorEvent.AddListener(CheckGetColorEvent);
    }
    public void CheckGetColorEvent(eActivableColor color)
    {
        getColorEvent?.Invoke(color);
    }
}
