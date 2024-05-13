using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpiderMonsterStats", menuName = "Scriptable Object/SpiderMonsterStat")]
public class SpiderMonsterStats : MonsterStat
{
    [Tooltip("근접 어택 사거리")]
    public float meleeAttackRange = 1.0f;
    [Tooltip("근접 어택 범위")]
    public float attackRange = 0.8f;
    [Tooltip("원거리 공격 데미지")]
    public int rangedAttackDamege = 1;
    [Tooltip("원거리 공격 속도")]
    public float rangedAttackSpeed = 1.0f;
    [Tooltip("원거리 공격 사정거리")]
    public float rangedAttackRange = 2.0f;
    [Tooltip("고개치기 공격 반동 파워")]
    public float specialAttackRound = 1.0f;
    [Tooltip("고개치기 공격 속도")]
    public float specialAttackSpeed = 1.0f;
    [Tooltip("땅찍기 파동 속도")]
    public float earthAttackSpeed = 1.0f;
    [Tooltip("땅찍기 파동 데미지")]
    public int earthAttackDamege = 1;
    [Tooltip("땅찍기 파편 지속 시간")]
    public float earthAttackTime = 0.1f;
    [Tooltip("땅찍기 점프 높이")]
    public float earthAttackJump = 10.0f;
    [Tooltip("땅찍기 파동 딜레이")]
    public float earthAttackDalay = 0.3f;
    [Tooltip("고개치기 공격 확률")]
    public float specialAttackPercent = 60.0f;
    [Tooltip("원거리 공격 확률")]
    public int rangeAttackPercent = 50;
    [Tooltip("돌진 땅찍기 사거리")]
    public float compositeAttackRound = 1.0f;
}
