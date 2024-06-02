using UnityEngine;
[CreateAssetMenu(fileName = "FlyAntMonsterStat", menuName = "Scriptable Object/FlyAntMonsterStat")]
public class FlyAntMonsterStat : MonsterStat
{
    [Tooltip("사각형 인지범위")]
    public Vector3 senseCube = new(2,2,0);
    [Tooltip("공격과 공격 사이의 추가 딜레이")]
    public float flyAntAttackDelay = 1.0f;
    [Tooltip("돌진 속도")]
    public float rushAttackSpeed = 1.0f;
    [Tooltip("리턴 속도")]
    public float returnSpeed = 1.0f;
    [Tooltip("돌진 데미지")]
    public int rushAttackDamage = 5;
    [Tooltip("돌진 준비 애니메이션 시간")]
    public float rushReadyAnimaionDuration = 1.5f;
    [Tooltip("연속 돌진이 딜레이되는 시간")]
    public float rushAttackDelay = 1.0f;
    [Tooltip("연속 돌진까지 걸리는 시간")]
    public int doubleRushAttackBetweenDelay = 1;
    [Tooltip("창 던지기 발사 속도")]
    public float spearThrowSpeed = 50;
    [Tooltip("창 던지기 데미지")]
    public int spearThrowDamage = 7;
    [Tooltip("회수되는 창 데미지")]
    public int spearThrowReturnDamage = 4;
    [Tooltip("회수되는 창의 속도")]
    public int spearThrowReturnSpeed = 50;
    [Tooltip("창이 날아가는 범위")]
    public float spearThrowAttackRange = 6.0f;
    [Tooltip("연속 돌진 확률")]
    public int doubleBodyAttackPercent = 50;
    [Tooltip("원래 자리로 돌아왔다는 판정 조건")]
    public float returnPosValue = 0.3f;
    [Tooltip("연속 돌진시 뒤로 빠지는 반동")]
    public float doubleBodyAttackRebound = 300f;
}
