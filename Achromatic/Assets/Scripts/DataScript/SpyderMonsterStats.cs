using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpyderMonsterStats", menuName = "Scriptable Object/SpyderMonsterStat")]
public class SpyderMonsterStats : MonsterStat
{
    [Tooltip("근접 어택 사거리")]
    public float meleeAttackRange = 1.0f;
    [Tooltip("근접 어택 범위")]
    public float AttackRange = 0.8f;
    [Tooltip("대기 상태까지 걸리는 시간")]
    public float usualTime = 1.0f;
    [Tooltip("원거리 공격 데미지")]
    public int rangedAttackDamege = 1;
    [Tooltip("원거리 공격 속도")]
    public float rangedAttackSpeed = 1.0f;
    [Tooltip("원거리 공격 사정거리")]
    public float rangedAttackRange = 1.0f;
    [Tooltip("고개치기 공격 데미지")]
    public int specialAttackDamege = 1;
    [Tooltip("고개치기 공격 이동 거리")]
    public float specialAttackRange = 1.0f;
    [Tooltip("고개치기 공격 속도")]
    public float specialAttackSpeed = 1.0f;
    [Tooltip("땅찍기 파동 속도")]
    public float earthAttackSpeed = 1.0f;
    [Tooltip("땅찍기 파동 데미지")]
    public int earthAttackDamege = 1;
    [Tooltip("땅찍기 파동 이동 거리")]
    public float earthAttackRange = 1.0f;
    [Tooltip("고개치기 공격 확률")]
    public int specialAttackPercent = 50;
    [Tooltip("땅찍기 공격 확률")]
    public int earthAttackPercent = 50;
}
