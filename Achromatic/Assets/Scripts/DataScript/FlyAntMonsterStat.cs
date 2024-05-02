using UnityEngine;
[CreateAssetMenu(fileName = "AntMonsterStats", menuName = "Scriptable Object/AntMonsterStat")]
public class FlyAntMonsterStat : MonsterStat
{
    [Tooltip("공격과 공격 사이의 추가 딜레이")]
    public float flyAntAttackDelay = 1.0f;
    [Tooltip("돌진 속도")]
    public float badyAttackSpeed = 1.0f;
    [Tooltip("돌진 데미지")]
    public int badyAttackDamage = 5;
    [Tooltip("연속 돌진까지 걸리는 시간")]
    public int repeatBadyAttackTime = 1;
    [Tooltip("창 던지기 발사 속도")]
    public float stabThrowSpeed = 50;
    [Tooltip("창 던지기 데미지")]
    public int stabThrowDamage = 7;
    [Tooltip("회수되는 창 데미지")]
    public int stabThrowReturnDamage = 4;
    [Tooltip("연속 돌진 확률")]
    public int doubleBadyAttackPer = 50;
}
