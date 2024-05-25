using UnityEngine;
[CreateAssetMenu(fileName = "AntMonsterStats", menuName = "Scriptable Object/AntMonsterStat")]
public class AntMonsterStat : MonsterStat
{
    [Tooltip("베기 공격 데미지")]
    public int slashAttackDamage = 1;
    [Tooltip("베기 공격 이후 이동거리")]
    public float slashAttackRebound = 100.0f;
    [Tooltip("베기 공격 딜레이")]
    public float slashAttackDelay = 0.1f;
    [Tooltip("베기 공격 판정 지속 시간")]
    public float slashAttackTime = 0.2f;
    [Tooltip("검기 공격 데미지")]
    public int swordAttackDamage = 1;
    [Tooltip("검기 속도")]
    public float swordAttackSpeed = 2.0f;
    [Tooltip("검기 거리 (초 단위 계산)")]
    public float swordAuraRangePerTime = 2.0f;
    [Tooltip("찌르기 공격 데미지(상하단)")]
    public int stabAttackDamage = 1;
    [Tooltip("마지막 찌르기 공격 데미지")]
    public int lastStabAttackDamage = 2;
    [Tooltip("찌르기 속도")]
    public float stabAttackSpeed = 2.0f;
    [Tooltip("찌르기 마지막 타격 속도")]
    public float lastStabSpeed = 4.0f;
    [Tooltip("찌르기 상하단 딜레이")]
    public float stabAttackDelay = 0.5f;
    [Tooltip("찌르기 중단 딜레이")]
    public float middleStabAttackDelay = 1.0f;
    [Tooltip("반격 유지 시간")]
    public float counterDurationTime = 2.0f;
    [Tooltip("반격 공격 사거리")]
    public float counterAttackRange = 1.0f;
    [Tooltip("반격 공격 판정 지속 시간")]
    public float counterAttackDurationTime;
    [Tooltip("반격 공격 데미지")]
    public int counterAttackDamage = 2;
    [Tooltip("반격 공격 속도")]
    public int counterAttackSpeed = 6;
    [Tooltip("찌르기 공격 확률")]
    public int stabAttackPercent = 33;
    [Tooltip("검기 공격 확률")]
    public int slashAttackPercent = 33;
}
