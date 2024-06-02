using UnityEngine;
[CreateAssetMenu(fileName = "FlyAntMonsterStat", menuName = "Scriptable Object/FlyAntMonsterStat")]
public class FlyAntMonsterStat : MonsterStat
{
    [Tooltip("�簢�� ��������")]
    public Vector3 senseCube = new(2,2,0);
    [Tooltip("���ݰ� ���� ������ �߰� ������")]
    public float flyAntAttackDelay = 1.0f;
    [Tooltip("���� �ӵ�")]
    public float rushAttackSpeed = 1.0f;
    [Tooltip("���� �ӵ�")]
    public float returnSpeed = 1.0f;
    [Tooltip("���� ������")]
    public int rushAttackDamage = 5;
    [Tooltip("���� �غ� �ִϸ��̼� �ð�")]
    public float rushReadyAnimaionDuration = 1.5f;
    [Tooltip("���� ������ �����̵Ǵ� �ð�")]
    public float rushAttackDelay = 1.0f;
    [Tooltip("���� �������� �ɸ��� �ð�")]
    public int doubleRushAttackBetweenDelay = 1;
    [Tooltip("â ������ �߻� �ӵ�")]
    public float spearThrowSpeed = 50;
    [Tooltip("â ������ ������")]
    public int spearThrowDamage = 7;
    [Tooltip("ȸ���Ǵ� â ������")]
    public int spearThrowReturnDamage = 4;
    [Tooltip("ȸ���Ǵ� â�� �ӵ�")]
    public int spearThrowReturnSpeed = 50;
    [Tooltip("â�� ���ư��� ����")]
    public float spearThrowAttackRange = 6.0f;
    [Tooltip("���� ���� Ȯ��")]
    public int doubleBodyAttackPercent = 50;
    [Tooltip("���� �ڸ��� ���ƿԴٴ� ���� ����")]
    public float returnPosValue = 0.3f;
    [Tooltip("���� ������ �ڷ� ������ �ݵ�")]
    public float doubleBodyAttackRebound = 300f;
}
