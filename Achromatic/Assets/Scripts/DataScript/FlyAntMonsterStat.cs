using UnityEngine;
[CreateAssetMenu(fileName = "FlyAntMonsterStat", menuName = "Scriptable Object/FlyAntMonsterStat")]
public class FlyAntMonsterStat : MonsterStat
{
    [Tooltip("���ݰ� ���� ������ �߰� ������")]
    public float flyAntAttackDelay = 1.0f;
    [Tooltip("���� �ӵ�")]
    public float badyAttackSpeed = 1.0f;
    [Tooltip("���� ������")]
    public int badyAttackDamage = 5;
    [Tooltip("���� �������� �ɸ��� �ð�")]
    public int repeatBadyAttackTime = 1;
    [Tooltip("â ������ �߻� �ӵ�")]
    public float stabThrowSpeed = 50;
    [Tooltip("â ������ ������")]
    public int stabThrowDamage = 7;
    [Tooltip("ȸ���Ǵ� â ������")]
    public int stabThrowReturnDamage = 4;
    [Tooltip("ȸ���Ǵ� â�� �ӵ�")]
    public int stabThrowReturnSpeed = 50;
    [Tooltip("â�� ���ư��� ����")]
    public float stabThrowAttackRange = 6.0f;
    [Tooltip("���� ���� Ȯ��")]
    public int doubleBadyAttackPer = 50;
    [Tooltip("���� �ڸ��� ���ƿԴٴ� ���� ����")]
    public float returnPosValue = 0.3f;
}
