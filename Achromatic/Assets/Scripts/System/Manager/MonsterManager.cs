using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface ICheckMonster
{
    void Respawn(GameObject monsterPos, bool isRespawn);
}
public class MonsterManager : SingletonBehavior<MonsterManager>
{
    public UnityEvent<eActivableColor> GetColorEvent;
    protected override void OnAwake()
    {
        PlayManager.Instance.FilterColorAttackEvent.AddListener(CheckGetColorEvent);
        PlayManager.Instance.ActivationColorEvent.AddListener(CheckGetColorEvent);
    }
    public void CheckGetColorEvent(eActivableColor color)
    {
        GetColorEvent?.Invoke(color);
    }
}
