using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AntMonsterStats", menuName = "Scriptable Object/AntMonsterStat")]
public class AntMonsterStat : MonsterStat
{
    [Tooltip("베기 공격 데미지")]
    public int cuttingAttackDamage = 1;
    [Tooltip("베기 공격 이후 이동거리")]
    public float cuttingAttackRebound = 100.0f;
    [Tooltip("검기 공격 데미지")]
    public float swordAttackDamage = 1.0f;
    [Tooltip("검기 속도")]
    public float swordAttackSpeed = 2.0f;
    [Tooltip("찌르기 속도")]
    public float stabAttackSpeed = 2.0f;
    [Tooltip("찌르기 마지막 타격 속도")]
    public float lastStabSpeed = 4.0f;
    [Tooltip("찌르기 상하단 딜레이")]
    public float stabAttackDelay = 1.0f;
    [Tooltip("찌르기 중단 딜레이")]
    public float middleStabAttackDelay = 2.0f;
    [Tooltip("반격 유지 시간")]
    public float counterAttackTime = 2.0f;
    [Tooltip("찌르기 공격 확률")]
    public int stabAttackPercent = 33;
    [Tooltip("검기 공격 확률")]
    public int swordAttackPercent = 33;
}
