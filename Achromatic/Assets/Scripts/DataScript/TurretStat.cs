using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretStats", menuName = "Scriptable Object/TurretStats")]
public class TurretStat : MonsterStat
{
    [Tooltip("터렛 공격 투사체가 지속되는 시간")]
    public float turretAttackDuration = 2.0f;
    [Tooltip("터렛 공격 투사체 속도")]
    public float turretAttackSpeed = 300f;
    [Tooltip("터렛 공격 투사체 데미지")]
    public int turretAttackDamage = 2;
    [Tooltip("터렛 공격 딜레이")]
    public float turretAttackDelay = 2.0f;
}
