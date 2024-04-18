using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStat", menuName = "Scriptable Object/MonsterStat")]
public class MonsterStat : ScriptableObject
{
    [SerializeField]
    private int monsterHP = 5;
    public int MonsterHP
    {
        get
        {
            return monsterHP;
        }
        set
        {
            monsterHP = value;
        }
    }

    public eActivableColor enemyColor;

    public float senseCircle = 2f;

    public float moveSpeed = 1f;

    public int attackDamage = 1;
    public float attackTime = 0.3f;
    public float attackCooldown = 1f;
    public int contactDamage = 1;

    public float hitReboundPower = 5f;
    public float heavyHitReboundPower = 10f;
    public float groggyTime = 0.5f;
    [Tooltip("대기 상태까지 걸리는 시간")]
    public float usualTime = 1.0f;


}
